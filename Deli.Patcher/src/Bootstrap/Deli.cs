using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
		private readonly static ManualLogSource _log;
		private readonly static IServiceKernel _kernel;
		private readonly static Dictionary<string, IAssetLoader> _assetLoaders;
		private readonly static List<IPatcher> _patchers;

		public static IServiceResolver Services => _kernel;

		public static IEnumerable<Mod> Mods { get; }

		static Deli()
		{
			_log = Logger.CreateLogSource(Constants.Name);
			_kernel = new StandardServiceKernel();
			_assetLoaders = new Dictionary<string, IAssetLoader>();

			Bind();

			Mods = new DeliBootstrap(_log, _kernel).CreateMods();
		}

		private static void Bind()
		{
			_kernel.Bind<ManualLogSource>().ToConstant(_log);

			BindJson();
			BindAssetReaders();
			BindAssetLoaders();
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

		private static void BindAssetReaders()
		{
			_kernel.Bind<IAssetReader<byte[]>>().ToConstant(new ByteArrayAssetReader());
			_kernel.Bind<IAssetReader<Assembly>>().ToConstant(new AssemblyAssetReader());
			_kernel.Bind<IAssetReader<Option<JObject>>>().ToConstant(new JObjectAssetReader(_log));
		}

		private static void BindAssetLoaders()
		{
			_kernel.Bind<IDictionary<string, IAssetLoader>>().ToConstant(_assetLoaders);

			_kernel.Bind<IEnumerable<IAssetLoader>>().ToRecursiveMethod(x => x.Get<IDictionary<string, IAssetLoader>>().Map(v => (IEnumerable<IAssetLoader>) v.Values)).InTransientScope();

			_kernel.Bind<IAssetLoader, string>().ToWholeMethod((services, context) => services.Get<IDictionary<string, IAssetLoader>>().Map(x => x.OptionGetValue(context)).Flatten()).InTransientScope();
		}

		private static void BindBepInEx()
		{
			_kernel.Bind<ManualLogSource, string>().ToContextualNopMethod(Logger.CreateLogSource).InSingletonScope();
			_kernel.Bind<ConfigFile, string>().ToContextualNopMethod(x => new ConfigFile(Path.Combine(Constants.ConfigDirectory, $"{x}.cfg"), false)).InSingletonScope();
		}

		private static void LoadMods(Func<Mod.Assets, Dictionary<string, string>> assetSelector)
		{
			foreach (var mod in Mods)
			{
				_log.LogInfo("Loading " + mod);

				// For each asset inside the mod, load it
				foreach (var asset in assetSelector(mod.Info.Assets))
				{
					var assetPath = asset.Key;
					var assetLoader = asset.Value;

					_log.LogDebug($"Loading asset [{assetLoader}: {assetPath}]");

					var loader = _kernel.Get<IAssetLoader, string>(assetLoader).Expect("Loader not present: " + assetLoader);
					loader.LoadAsset(_kernel, mod, assetPath);
				}
			}
		}

		private enum Stage
		{
			Patch,
			Runtime
		}

		private static Stage _stage;

		private static void StageCheck(Stage stage)
		{
			if (_stage != stage)
			{
				throw new InvalidOperationException("Invalid stage.");
			}

			++_stage;
		}

		private const string PatcherLoader = "assembly.patcher";
		internal static Dictionary<string, List<IPatcher>> Patch()
		{
			StageCheck(Stage.Patch);
			_assetLoaders.Add(PatcherLoader, new AssemblyAssetLoader(_log));

			LoadMods(x => x.Patcher);

			var dllPatchers = new Dictionary<string, List<IPatcher>>();
			foreach (var patcher in _patchers)
			{
				var patchersForDll = dllPatchers.GetOrInsertWith(patcher.TargetDLL, () => new List<IPatcher>());
				patchersForDll.Add(patcher);
			}

			return dllPatchers;
		}

		public static void Runtime(IModule module)
		{
			StageCheck(Stage.Runtime);
			_assetLoaders.Remove(PatcherLoader);

			module.Load(_kernel);
			LoadMods(x => x.Runtime);
		}
	}
}
