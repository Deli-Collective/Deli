using System.Collections;
using System.Linq;
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
			var manager = new GameObject(DeliConstants.Metadata.Name);

			var setup = new SetupStage(blob.StageData, manager);
			var runtime = new RuntimeStage(blob.StageData);

			// Eagerly evaluate; do not leave this to runtime to enumerate or it will be too late.
			var mods = setup.LoadMods(blob.Mods).ToList();

			StartCoroutine(runtime.LoadMods(mods, StartCoroutine));
		}
	}
}
