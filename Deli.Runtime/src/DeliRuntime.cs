using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ADepIn;
using ADepIn.Fluent;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace Deli
{
	[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
	public class DeliRuntime : BaseUnityPlugin, IModule
	{
		private IServiceKernel _kernel;

		public DeliRuntime()
		{
			Entrypoint.Postpatch(this);

			CheckModVersions();
		}

		public void Load(IServiceKernel kernel)
		{
			_kernel = kernel;

			var log = kernel.Get<ManualLogSource>().Unwrap();
			log.LogDebug("Injecting runtime loader...");

			var manager = new GameObject("Deli Manager");
			void DeliBehaviourLoader(IServiceKernel _0, Mod _1, string _2, Type type)
			{
				if (type.IsAbstract || !type.IsAssignableFrom(typeof(DeliBehaviour))) return;

				manager.AddComponent(type);
			}

			var loaders = kernel.Get<IDictionary<string, IAssetLoader>>().Unwrap();
			loaders[Constants.AssemblyLoaderName] = new AssemblyAssetLoader(log, new AssemblyAssetLoader.TypeLoadHandler[]
			{
				DeliBehaviourLoader
			});
		}

		private void CheckModVersions()
		{
			var regex = new Regex(@"^(?:https?:\/\/)?(?:[^@\/\n]+@)?(?:www\.)?([^:\/?\n]+)", RegexOptions.IgnoreCase);

			// Perform version checks on all the mods
			foreach (var mod in Deli.Mods)
				StartCoroutine(CheckModLatestVersion(mod, regex));
		}

		private IEnumerator CheckModLatestVersion(Mod mod, Regex regex)
		{
			// Exit if this mod doesn't have a source
			if (!mod.Info.SourceUrl.MatchSome(out var url) || string.IsNullOrEmpty(url))
			{
				mod.Log.LogInfo("Mod has no source");
				yield break;
			}

			var domain = regex.Match(url).Groups[0].Value;

			// Exit if we don't have a version checker for the domain
			if (!_kernel.Get<IVersionChecker, string>(domain).MatchSome(out var checker))
			{
				mod.Log.LogInfo($"No version checker registered for the domain {domain}");
				yield break;
			}

			// Check
			yield return checker.GetLatestVersion(mod);
			var result = checker.Result;

			if (result.MatchSome(out var version))
			{
				if (version == mod.Info.Version)
					mod.Log.LogInfo($"Mod is up to date! ({version})");
				else if (version > mod.Info.Version)
					mod.Log.LogWarning($"There is a newer version of this mod available. ({mod.Info.Version}) -> ({version})");
				else
					mod.Log.LogWarning($"This mod is more recent than the most recent version found at its source! ({version})");
			}
			else mod.Log.LogWarning($"Source URL for this mod is set but no version was found.");
		}
	}
}
