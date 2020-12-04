using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ADepIn;
using ADepIn.Fluent;
using ADepIn.Impl;
using BepInEx.Configuration;
using BepInEx.Logging;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Linq;
using Valve.Newtonsoft.Json.Serialization;

namespace Deli
{
	/// <summary>
	/// 	The core of the Deli modding framework, accessible at patch-time and runtime
	/// </summary>
	public static class Deli
	{
		private enum Stage
		{
			None,
			Patcher,
			Runtime
		}

		private static readonly ManualLogSource _log;
		private static readonly IServiceKernel _kernel;
		private static readonly Dictionary<string, IAssetLoader> _assetLoaders;
		private static readonly Dictionary<string, IList<IPatcher>> _patchers;
		private static readonly Dictionary<string, IVersionCheckable> _versionCheckable;

		private static Stage _stage;

		/// <summary>
		/// 	All of the services available to Deli
		/// </summary>
		public static IServiceResolver Services => _kernel;

		/// <summary>
		/// 	All of the named <see cref="IAssetLoader"/>s available to Deli
		/// </summary>
		public static IEnumerable<KeyValuePair<string, IAssetLoader>> AssetLoaders => _assetLoaders;

		/// <summary>
		/// 	All of the <see cref="IPatcher"/>s and corresponding DLL targets available to Deli
		/// </summary>
		public static IEnumerable<KeyValuePair<string, IEnumerable<IPatcher>>> Patchers
		{
			get
			{
				foreach (var pair in _patchers)
				{
					yield return new KeyValuePair<string, IEnumerable<IPatcher>>(pair.Key, pair.Value);
				}
			}
		}

		/// <summary>
		/// 	All of the Deli mods that were able to be created
		/// </summary>
		public static IEnumerable<Mod> Mods { get; }

		/// <summary>
		/// 	Called when patching is complete
		/// </summary>
		public static event Action PatcherComplete;

		/// <summary>
		/// 	Called when runtime initialization is complete
		/// </summary>
		public static event Action RuntimeComplete;

		static Deli()
		{
			_log = Logger.CreateLogSource(DeliConstants.Name);
			_kernel = new StandardServiceKernel();
			_assetLoaders = new Dictionary<string, IAssetLoader>
			{
				[DeliConstants.AssemblyLoaderName] = new AssemblyAssetLoader(_log, Enumerable.Empty<AssemblyAssetLoader.TypeLoadHandler>())
			};
			_versionCheckable = new Dictionary<string, IVersionCheckable>();
			_patchers = new Dictionary<string, IList<IPatcher>>();

			_stage = Stage.None;

			Bind();

			Mods = new DeliBootstrap(_log, _kernel).CreateMods();
		}

		private static void Bind()
		{
			_kernel.Bind<ManualLogSource>().ToConstant(_log);

			BindJson();
			BindPatchers();
			BindAssetReaders();
			BindAssetLoaders();
			BindVersionCheckers();
			BindBepInEx();
		}

		private static void BindJson()
		{
			// The serializer itself
			_kernel.Bind<JsonSerializerSettings>().ToConstant(new JsonSerializerSettings
			{
				ContractResolver = new DefaultContractResolver
				{
					NamingStrategy = new CamelCaseNamingStrategy()
				},
				Converters = new List<JsonConverter>()
				{
					new OptionJsonConverter()
				}
			});
			_kernel.Bind<JsonSerializer>().ToRecursiveNopMethod(x =>
			{
				var settings = x.Get<JsonSerializerSettings>().Expect("JSON settings not found.");

				return JsonSerializer.Create(settings);
			}).InSingletonNopScope();

			// Schemas
			_kernel.BindJson<Mod.Manifest>();
		}

		private static void BindPatchers()
		{
			_kernel.Bind<IDictionary<string, IList<IPatcher>>>().ToConstant(_patchers);
			_kernel.Bind<IList<IPatcher>, string>().ToWholeMethod((services, context) => services.Get<IDictionary<string, IList<IPatcher>>>().Map(v => v.GetOrInsertWith(context, () => new List<IPatcher>()))).InTransientScope();
		}

		private static void BindAssetReaders()
		{
			_kernel.Bind<IAssetReader<byte[]>>().ToConstant(new ByteArrayAssetReader());
			_kernel.Bind<IAssetReader<Assembly>>().ToConstant(new AssemblyAssetReader());
			_kernel.Bind<IAssetReader<Option<JObject>>>().ToConstant(new JObjectAssetReader(_log));
		}

		private static void BindAssetLoaders()
		{
			_kernel.Bind<IDictionary<string, IAssetLoader>>().ToConstant(_assetLoaders);
			_kernel.Bind<IAssetLoader, string>().ToWholeMethod((services, context) => services.Get<IDictionary<string, IAssetLoader>>().Map(x => x.OptionGetValue(context)).Flatten()).InTransientScope();
		}

		private static void BindVersionCheckers()
		{
			_kernel.Bind<IDictionary<string, IVersionCheckable>>().ToConstant(_versionCheckable);
			_kernel.Bind<IVersionCheckable, string>().ToWholeMethod((services, context) => services.Get<IDictionary<string, IVersionCheckable>>().Map(x => x.OptionGetValue(context)).Flatten()).InTransientScope();
		}

		private static void BindBepInEx()
		{
			_kernel.Bind<ManualLogSource, string>().ToContextualNopMethod(Logger.CreateLogSource).InSingletonScope();
			_kernel.Bind<ConfigFile, string>().ToContextualNopMethod(x => new ConfigFile(Path.Combine(DeliConstants.ConfigDirectory, x + "." + DeliConstants.ConfigExtension), false)).InSingletonScope();
		}

		private static void LoadMods(Stage stage, Func<Mod.Manifest, Option<Dictionary<string, string>>> assetSelector)
		{
			var stageLoading = stage + "-loading ";

			foreach (var mod in Mods)
			{
				_log.LogInfo(stageLoading + mod);

				if (!assetSelector(mod.Info).MatchSome(out var assets)) continue;

				// For each asset inside the mod, load it
				foreach (var asset in assets)
				{
					var pattern = DeliConstants.GlobReplacements.Aggregate(Regex.Escape(asset.Key), (s, r) => s.Replace(r.Key, r.Value));
					pattern = $"^{pattern}$";

					var loaderName = asset.Value;
					using var glob = mod.Resources.Find(pattern).GetEnumerator();

					if (!glob.MoveNext())
					{
						_log.LogWarning("Path matched no files: " + pattern);
						continue;
					}

					do
					{
						var assetPath = glob.Current;
						_log.LogDebug($"Loading asset {{{loaderName}: {assetPath}}}");

						var loader = _kernel.Get<IAssetLoader, string>(loaderName).Expect("Loader not present: " + loaderName);
						loader.LoadAsset(_kernel, mod, assetPath);
					} while (glob.MoveNext());
				}
			}
		}

		private static void StageCheck(Stage stage)
		{
			if (_stage + 1 != stage)
			{
				throw new InvalidOperationException("Invalid stage.");
			}

			++_stage;
		}

		internal static void PatchStage()
		{
			const Stage stage = Stage.Patcher;

			StageCheck(stage);
			LoadMods(stage, x => x.Patcher);

			PatcherComplete?.Invoke();
		}

		internal static void RuntimeStage(IDeliPlugin module)
		{
			const Stage stage = Stage.Runtime;

			StageCheck(stage);
			_assetLoaders[DeliConstants.AssemblyLoaderName] = module.Load(_log);
			LoadMods(stage, x => x.Runtime);

			RuntimeComplete?.Invoke();
		}

		/// <summary>
		/// 	Adds an asset loader name and associated asset loader
		/// </summary>
		/// <param name="name">The name of the asset loader</param>
		/// <param name="loader">The asset loader itself</param>
		public static void AddAssetLoader(string name, IAssetLoader loader)
		{
			_assetLoaders.Add(name, loader);
		}

		/// <summary>
		/// 	Adds a patcher for the specified file name
		/// </summary>
		/// <param name="fileName">The name of the file (not path) to patch</param>
		/// <param name="patcher">The patcher itself</param>
		public static void AddPatcher(string fileName, IPatcher patcher)
		{
			if (_stage != Stage.Patcher)
			{
				throw new InvalidOperationException("Patching has already been performed.");
			}

			_patchers.GetOrInsertWith(fileName, () => new List<IPatcher>()).Add(patcher);
		}

		/// <summary>
		/// 	Adds a version checkable for the specified domain
		/// </summary>
		/// <param name="domain">The domain the version checkable is responsible for</param>
		/// <param name="checker">The version checkable itself</param>
		public static void AddVersionCheckable(string domain, IVersionCheckable checker)
		{
			_versionCheckable.Add(domain, checker);
		}

		/// <summary>
		/// 	Gets a version checkable for a specified domain
		/// </summary>
		/// <param name="domain">The domain of the version checkable to get</param>
		public static Option<IVersionCheckable> GetVersionCheckable(string domain)
		{
			return _versionCheckable.OptionGetValue(domain);
		}
	}
}
