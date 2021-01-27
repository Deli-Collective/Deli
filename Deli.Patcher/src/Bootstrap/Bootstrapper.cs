using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Deli.Patcher.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Deli.Patcher.Bootstrap
{
	internal class Bootstrapper
	{
		internal readonly ManualLogSource Logger;

		private Stage.Blob? _stageData;
		private PatcherStage? _stage;
		private List<Mod>? _mods;

		private Stage.Blob StageData
		{
			get
			{
				Stage.Blob Init()
				{
					var jsonReaders = new ImmediateReaderCollection(Logger);
					var serializer = JsonSerializer.Create(new JsonSerializerSettings
					{
						Formatting = Formatting.Indented,
						ContractResolver = new DefaultContractResolver
						{
							NamingStrategy = new SnakeCaseNamingStrategy()
						},
						Converters =
						{
							new AssetLoaderIDJsonConverter()
						}
					});
					var sharedLoaders = new NestedServiceCollection<Mod, string, ImmediateAssetLoader<Stage>>();
					var immediateReaders = new ImmediateReaderCollection(Logger);
					var data = new Stage.Blob(jsonReaders, serializer, Logger, sharedLoaders, immediateReaders);

					return data;
				}

				return _stageData ??= Init();
			}
		}

		private PatcherStage Stage => _stage ??= new PatcherStage(StageData);

		private List<Mod> Mods
		{
			get
			{
				List<Mod> Init()
				{
					var manifestReader = Stage.RegisterJson<Mod.Manifest>();
					var discovery = new Discovery(Logger, manifestReader);
					var sorter = new Sorter(Logger);

					var mods = discovery.Run();
					mods = sorter.Run(mods);
					mods = Stage.LoadMods(mods);

					return mods.ToList();
				}

				return _mods ??= Init();
			}
		}

		internal NestedServiceCollection<string, Mod, Patcher> Patchers
		{
			get
			{
				var stage = Stage;
				// Ensure mods have loaded
				var _ = Mods;

				return stage.Patchers;
			}
		}

		internal HandoffBlob Blob => new HandoffBlob(StageData, Mods);

		public Bootstrapper(ManualLogSource logger)
		{
			Logger = logger;
		}
	}
}
