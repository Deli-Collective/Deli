using System.Collections;
using BepInEx;
using Deli.Patcher.Bootstrap;
using UnityEngine;

namespace Deli.Setup
{
	public delegate Coroutine CoroutineRunner(IEnumerator method);

	[BepInPlugin(DeliConstants.Metadata.Guid, DeliConstants.Metadata.Name, DeliConstants.Metadata.Version)]
	public class PluginEntrypoint : BaseUnityPlugin
	{
		private void Awake()
		{
			var blob = PatcherEntrypoint.Handoff();
			var stage = new SetupStage(blob.StageData);
			var loader = stage.LoadMods(blob.Mods, StartCoroutine);

			StartCoroutine(loader);
		}
	}
}
