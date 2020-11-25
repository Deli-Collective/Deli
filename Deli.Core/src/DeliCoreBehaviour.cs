using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using ADepIn;
using Deli.Core.VersionCheckers;

namespace Deli.Core
{
	public class DeliCoreBehaviour : DeliBehaviour
	{
		private IDictionary<string, IVersionChecker> _versionCheckers;
		private readonly Regex _compiledRegex = new Regex(@"^(?:https?:\/\/)?(?:[^@\/\n]+@)?(?:www\.)?([^:\/?\n]+)", RegexOptions.IgnoreCase);

		private void Awake()
		{
			// Add our callback
			Deli.Services.Get<IList<Deli.ModLoadedEvent>>().Expect("Missing services list for ModLoadedEvent").Add(OnModLoaded);

			// Get our list of version checkers
			_versionCheckers = Deli.Services.Get<IDictionary<string, IVersionChecker>>().Expect("Missing version checker dict");
			_versionCheckers.Add("https://github.com", new GitHubVersionChecker());

			// This is required because Mono doesn't ship with any root certificates
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
		}

		private void OnModLoaded(Mod mod)
		{
			StartCoroutine(FetchModLatestVersion(mod));
		}

		private IEnumerator FetchModLatestVersion(Mod mod)
		{
			// Exit if this mod doesn't have a source
			if (!mod.Info.SourceUrl.MatchSome(out var url) || string.IsNullOrEmpty(url))
			{
				mod.Log.LogInfo("Mod has no source");
				yield break;
			}

			var domain = _compiledRegex.Match(url).Groups[0].Value;
			var checker = _versionCheckers.FirstOrDefault(x => x.Key == domain).Value;

			// Exit if we don't have a version checker for the domain
			if (checker == null)
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
