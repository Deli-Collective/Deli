using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using Deli.Patcher;
using Deli.Patcher.Readers;
using Newtonsoft.Json;

namespace Deli.Setup
{
	[BepInPlugin(DeliConstants.Metadata.Guid, DeliConstants.Metadata.Name, DeliConstants.Metadata.Version)]
	public class PluginEntrypoint : BaseUnityPlugin
	{
		private void Awake()
		{
			PatcherEntrypoint.Handoff(Entrypoint);
		}

		private void Entrypoint(ManualLogSource logger, JsonSerializer serializer, JObjectImmediateReader jObjectImmediateReader, Dictionary<string, ISharedAssetLoader> sharedLoaders, ImmediateReaderCollection immediateReaders)
		{
			var stage = new SetupStage(logger, serializer, jObjectImmediateReader, sharedLoaders, immediateReaders);
		}
	}
}
