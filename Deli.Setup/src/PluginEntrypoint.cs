using System.Collections;
using System.Collections.Generic;
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

			var behaviours = new Dictionary<Mod, List<DeliBehaviour>>();
			var setup = new SetupStage(blob.StageData, manager, behaviours);
			var runtime = new RuntimeStage(blob.StageData, behaviours);

			// Eagerly evaluate; do not leave this to runtime to enumerate or it will be too late.
			var mods = setup.RunInternal(blob.Mods).ToList();

			StartCoroutine(runtime.Run(mods, StartCoroutine));
		}
	}
}
