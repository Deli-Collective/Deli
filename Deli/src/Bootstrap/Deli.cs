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
		private static readonly Dictionary<string, IVersionChecker> _versionCheckers;

		private static Stage _stage;

		public static IServiceResolver Services => _kernel;

		public static IEnumerable<KeyValuePair<string, IAssetLoader>> AssetLoaders => _assetLoaders;

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

		public static IEnumerable<Mod> Mods { get; }


		public delegate void PatcherCompleteHandler();
		public delegate void RuntimeCompleteHandler();

		public static event PatcherCompleteHandler PatcherComplete;
		public static event RuntimeCompleteHandler RuntimeComplete;

		static Deli()
		{
			_log = Logger.CreateLogSource(DeliConstants.Name);
			_kernel = new StandardServiceKernel();
			_assetLoaders = new Dictionary<string, IAssetLoader>
			{
				[DeliConstants.AssemblyLoaderName] = new AssemblyAssetLoader(_log, Enumerable.Empty<AssemblyAssetLoader.TypeLoadHandler>())
			};
			_versionCheckers = new Dictionary<string, IVersionChecker>();
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
			_kernel.Bind<IDictionary<string, IVersionChecker>>().ToConstant(_versionCheckers);
			_kernel.Bind<IVersionChecker, string>().ToWholeMethod((services, context) => services.Get<IDictionary<string, IVersionChecker>>().Map(x => x.OptionGetValue(context)).Flatten()).InTransientScope();
		}

		private static void BindBepInEx()
		{
			_kernel.Bind<ManualLogSource, string>().ToContextualNopMethod(Logger.CreateLogSource).InSingletonScope();
			_kernel.Bind<ConfigFile, string>().ToContextualNopMethod(x => new ConfigFile(Path.Combine(DeliConstants.ConfigDirectory, $"{x}.cfg"), false)).InSingletonScope();
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
					var pattern = "^" + Regex.Escape(asset.Key).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
					var loaderName = asset.Value;

					foreach (var assetPath in mod.Resources.Find(pattern))
					{
						_log.LogDebug($"Loading asset [{loaderName}: {assetPath}]");

						var loader = _kernel.Get<IAssetLoader, string>(loaderName).Expect("Loader not present: " + loaderName);
						loader.LoadAsset(_kernel, mod, assetPath);
					}
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

		internal static void RuntimeStage(IDeliRuntime module)
		{
			const Stage stage = Stage.Runtime;

			StageCheck(stage);
			_assetLoaders[DeliConstants.AssemblyLoaderName] = module.Load(_log);
			LoadMods(stage, x => x.Runtime);

			RuntimeComplete?.Invoke();
		}

		public static void AddLoader(string name, IAssetLoader loader)
		{
			_assetLoaders.Add(name, loader);
		}

		public static void AddPatcher(string fileName, IPatcher patcher)
		{
			if (_stage != Stage.Patcher)
			{
				throw new InvalidOperationException("Patching has already been performed.");
			}

			_patchers.GetOrInsertWith(fileName, () => new List<IPatcher>()).Add(patcher);
		}

		public static void AddVersionChecker(string domain, IVersionChecker checker)
		{
			_versionCheckers.Add(domain, checker);
		}

		public static Option<IVersionChecker> GetVersionChecker(string domain)
		{
			return _versionCheckers.OptionGetValue(domain);
		}
	}
}
