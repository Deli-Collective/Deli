using System;
using Deli.Runtime.Yielding;
using Semver;
using Metadata = Deli.Bootstrap.Constants.Metadata;

namespace Deli.Runtime
{
	/// <summary>
	///		Checks the version of a mod from a URL's path.
	/// </summary>
	public delegate ResultYieldInstruction<SemVersion?> VersionChecker(string path);

	internal static class VersionCheckers
	{
		public static VersionCheckerCollection DefaultCollection()
		{
			return new()
			{
				["github.com"] = GitHub.Checker
			};
		}

		private static class GitHub
		{
			private static readonly JsonRestClient Client;

			static GitHub()
			{
				var headers = XRateLimit.HeaderInfo.Prefixed("X-RateLimit-", "Limit", "Remaining", "Reset");
				Client = new JsonRestClient("https://api.github.com/")
				{
					RateLimit = new XRateLimit(headers),
					RequestHeaders =
					{
						["Accept"] = "application/vnd.github.v3+json",
						["User-Agent"] = Metadata.Name + "/" + Metadata.Version
					}
				};
			}

			public static ResultYieldInstruction<SemVersion?> Checker(string path)
			{
				var split = path.Split('/');
				if (split.Length != 2)
				{
					throw new ArgumentException("Path must be to a repository ({owner}/{repo})", nameof(path));
				}

				return Client.Get($"repos/{split[0]}/{split[1]}/releases/latest").CallbackWith(payload =>
				{
					var version = payload?["tag_name"]?.ToObject<string>();
					if (version is null)
					{
						return null;
					}

					if (version[0] == 'v')
					{
						version = version.Substring(1);
					}

					return SemVersion.Parse(version);
				});
			}
		}
	}
}
