using System;
using System.Collections;
using System.Text.RegularExpressions;
using ADepIn;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace Deli
{
	[BepInPlugin(DeliConstants.Guid, DeliConstants.Name, DeliConstants.Version)]
	internal class DeliPlugin : BaseUnityPlugin, IDeliPlugin
	{
		public DeliPlugin()
		{
			DeliRuntime.Instance = this;
			Entrypoint.Postpatch(this);

			CheckModVersions();
		}

		public IAssetLoader Load(ManualLogSource log)
		{
			log.LogDebug("Injecting runtime loader...");

			var manager = new GameObject("Deli Manager");
			void LoadDeliBehaviour(IServiceKernel _0, Mod mod, string _1, Type type)
			{
				if (type.IsAbstract || !typeof(DeliBehaviour).IsAssignableFrom(type)) return;

				DeliRuntime.BehaviourSources.Add(type, mod);
				manager.AddComponent(type);

				log.LogDebug("Loaded DeliBehaviour: " + type);
			}

			return new AssemblyAssetLoader(log, new AssemblyAssetLoader.TypeLoadHandler[]
			{
				LoadDeliBehaviour
			});
		}

		private void CheckModVersions()
		{
			var regex = new Regex(@"^(?:https?:\/\/)?(?:[^@\/\n]+@)?(?:www\.)?([^:\/?\n]+)(?:\/)?(.*?)(?:\/)?$", RegexOptions.IgnoreCase);

			// Perform version checks on all the mods
			foreach (var check in Deli.Mods.WhereSelect(x => CheckModVersion(x, regex)))
			{
				StartCoroutine(check);
			}
		}

		// Synchronous pre-checks
		private Option<IEnumerator> CheckModVersion(Mod mod, Regex regex)
		{
			if (!mod.Info.SourceUrl.MatchSome(out var url))
			{
				mod.Logger.LogDebug("Source URL not present");

				return Option.None<IEnumerator>();
			}

			var match = regex.Match(url);
			if (!match.Success)
			{
				mod.Logger.LogWarning("Source URL is invalid");

				return Option.None<IEnumerator>();
			}

			var groups = match.Groups;
			var domain = groups[1].Value;
			var path = groups[2].Value;

			// Exit if we don't have a version checkable for the domain
			if (!Deli.GetVersionCheckable(domain).MatchSome(out var checkable))
			{
				mod.Logger.LogDebug($"No version checkable found: {domain}");

				return Option.None<IEnumerator>();
			}

			var checker = checkable.Check(mod, path);
			if (!checker.MatchSome(out var checkerInner))
			{
				mod.Logger.LogWarning($"Source URL path is misformatted: {path}");

				return Option.None<IEnumerator>();
			}

			mod.Logger.LogDebug("Checking version...");
			var coroutine = CheckModVersion(mod, regex, checkerInner);

			return Option.Some(coroutine);
		}

		// Asynchronous
		private IEnumerator CheckModVersion(Mod mod, Regex regex, IVersionChecker checker)
		{
			yield return checker.Await();

			if (!checker.Result.MatchSome(out var remoteVersion))
			{
				mod.Logger.LogInfo($"No versions found at source URL.");
				yield break;
			}

			var localVersion = mod.Info.Version;

			switch (localVersion.CompareTo(remoteVersion))
			{
				case -1:
					mod.Logger.LogWarning($"There is a newer version available: ({localVersion}) -> ({remoteVersion})");
					break;
				case 0:
					mod.Logger.LogInfo($"You are up to date: ({remoteVersion})");
					break;
				case 1:
					mod.Logger.LogWarning($"You are ahead of the latest version: ({localVersion}) <- ({remoteVersion})");
					break;

				default: throw new ArgumentOutOfRangeException();
			}
		}
	}
}
