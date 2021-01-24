using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using Deli.Patcher;

namespace Deli.Setup
{
	[BepInPlugin(DeliConstants.Metadata.Guid, DeliConstants.Metadata.Name, DeliConstants.Metadata.Version)]
	public class PluginEntrypoint : BaseUnityPlugin
	{
		private void Awake()
		{
			PatcherEntrypoint.Handoff(Entrypoint);
		}

		private void Entrypoint(ManualLogSource logger, Dictionary<string, ISharedAssetLoader> sharedAssetLoaders, ImmediateReaderCollection immediateReaders)
		{
			var stage = new SetupStage(logger, sharedAssetLoaders, immediateReaders);
		}
	}
}
