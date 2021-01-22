using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using Deli.Patcher;

namespace Deli.Setup
{
	[BepInPlugin(DeliMetadata.Guid, DeliMetadata.Name, DeliMetadata.Version)]
	public class PluginEntrypoint : BaseUnityPlugin
	{
		private void Awake()
		{
			PatcherEntrypoint.Handoff(Entrypoint);
		}

		private void Entrypoint(ManualLogSource logger, ImmediateReaderCollection immediateReaders)
		{
			var stage = new DeliSetupStage(logger, immediateReaders);
			// TODO: call stuff
		}
	}
}
