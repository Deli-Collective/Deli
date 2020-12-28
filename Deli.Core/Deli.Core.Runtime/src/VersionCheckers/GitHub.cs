using System;
using System.Collections;
using System.Text.RegularExpressions;
using ADepIn;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.Networking;
using Valve.Newtonsoft.Json.Linq;

namespace Deli.Core
{
	public class GitHubVersionCheckable : IVersionCheckable
	{
		private static readonly Regex _pathMatcher = new Regex(@"^([^/\.:]+)/([^/\.:]+)$");
		private static readonly Regex _versionMatcher = new Regex(@"(\d\.\d(?:\.\d(?:\.\d)?)?)");

		public Option<IVersionChecker> Check(Mod mod, string path)
		{
			var match = _pathMatcher.Match(path);
			if (!match.Success)
			{
				// Not [owner]/[repo]
				return Option.None<IVersionChecker>();
			}

			var groups = match.Groups;
			var owner = groups[1].Value;
			var repo = groups[2].Value;

			// Make and wait for the request
			IVersionChecker checker = new VersionChecker(mod.Logger, owner, repo);
			return Option.Some(checker);
		}

		private class VersionChecker : CachedVersionChecker
		{
			private const string Api = "https://api.github.com/";

			private static Option<Coroutine> _rateLimitInitializer;
			private static Option<RateLimitInfo> _rateLimit;

			private readonly ManualLogSource _log;
			private readonly string _owner;
			private readonly string _repo;

			private Option<UnityWebRequest> _request;

			protected override string Url  => Api + $"repos/{_owner}/{_repo}/releases/latest";

			public VersionChecker(ManualLogSource log, string owner, string repo)
			{
				_log = log;
				_owner = owner;
				_repo = repo;
			}

			private IEnumerator FetchRateLimit()
			{
				_log.LogDebug("Fetching initial rate limit info...");

				var request = UnityWebRequest.Get(Api + "rate_limit");
				yield return request.Send();

				if (request.isError)
				{
					throw new Exception("Failed to retrieve rate limit information: " + request.error);
				}

				var raw = request.downloadHandler.text;
				var obj = JObject.Parse(raw);

				_rateLimitInitializer = Option.None<Coroutine>();
				_rateLimit = Option.Some(new RateLimitInfo(obj));

				_log.LogDebug("Initial rate limit info cached");
			}

			private Option<Version> ProcessResponse(UnityWebRequest request)
			{
				if (request.responseCode == 404)
				{
					// No releases
					return Option.None<Version>();
				}

				if (request.isError)
				{
					throw new Exception("Failed to retrieve version: " + request.error);
				}

				_rateLimit.Unwrap().Update(request);

				// Parse the response as JSON
				var raw = request.downloadHandler.text;
				var obj = JObject.Parse(raw);

				// Get the version from the JSON
				var tag = obj["tag_name"].Value<string>();
				var matches = _versionMatcher.Match(tag);
				if (!matches.Success)
				{
					_log.LogWarning("Invalid remote version: " + tag);

					return Option.None<Version>();
				}

				var version = new Version(matches.Groups[0].Value);
				return Option.Some(version);
			}

			protected override IEnumerator AwaitInternal()
			{
				_request.ExpectNone("Already awaiting");

				// Create request
				var request = UnityWebRequest.Get(Url);
				_request = Option.Some(request);

				// Get rate limit info
				if (!_rateLimit.MatchSome(out var rateLimit))
				{
					_log.LogDebug("Awaiting GitHub rate limit information...");

					if (_rateLimitInitializer.IsNone)
					{
						var coroutine = DeliRuntime.StartCoroutine(FetchRateLimit());
						_rateLimitInitializer = Option.Some(coroutine);

						yield return coroutine;
					}
					else
					{
						// Only one coroutine can await another, so just wait for this one to finish and free itself
						yield return new WaitUntil(() => _rateLimit.IsSome);
					}

					rateLimit = _rateLimit.Unwrap();
				}

				// Wait until a request can be made before sending it
				yield return DeliRuntime.StartCoroutine(rateLimit.Consume());
				yield return request.Send();

				Result = ProcessResponse(request);
			}

			// https://docs.github.com/en/free-pro-team@latest/rest/overview/resources-in-the-rest-api#rate-limiting
			private class RateLimitInfo
			{
				private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

				private int _limit;
				private int _remaining;
				private Option<DateTime> _reset;

				private long ResetEpoch
				{
					set => _reset = Option.Some(_epoch.AddSeconds(value));
				}

				public RateLimitInfo(JObject response)
				{
					var data = response["resources"]["core"];

					_limit = data["limit"].Value<int>();
					_remaining = data["remaining"].Value<int>();
					ResetEpoch = data["reset"].Value<long>();
				}

				public void Update(UnityWebRequest request)
				{
					const string prefix = "X-RateLimit-";

					_limit = int.Parse(request.GetResponseHeader(prefix + "Limit"));
					_remaining = int.Parse(request.GetResponseHeader(prefix + "Remaining"));
					ResetEpoch = long.Parse(request.GetResponseHeader(prefix + "Reset"));
				}

				public IEnumerator Consume()
				{
					if (_reset.MapOr(false, v => v <= DateTime.UtcNow))
					{
						_reset = Option.None<DateTime>();
						_remaining = _limit;
					}

					while (_remaining <= 0)
					{
						DateTime reset;
						while (!_reset.MatchSome(out reset))
						{
							yield return null;
						}

						var offset = reset - DateTime.UtcNow;
						var duration = (float) offset.TotalSeconds;

						yield return new WaitForSecondsRealtime(duration);
					}

					--_remaining;
				}
			}
		}
	}
}
