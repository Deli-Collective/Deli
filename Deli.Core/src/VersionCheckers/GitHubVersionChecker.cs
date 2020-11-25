using System;
using System.Collections;
using System.Text.RegularExpressions;
using ADepIn;
using UnityEngine;
using UnityEngine.Networking;
using Valve.Newtonsoft.Json.Linq;

namespace Deli.Core.VersionCheckers
{
	public class GitHubVersionChecker : IVersionChecker
	{
		public Option<Version> Result { get; private set; }

		public IEnumerator GetLatestVersion(Mod mod)
		{
			// Get the API url for the mod
			// Format of the GitHub releases API
			// https://api.github.com/repos/[user]/[repo]/releases/latest
			var split = mod.Info.SourceUrl.Expect("Mod is missing SourceURL").Split('/');
			var url = $"https://api.github.com/repos/{split[3]}/{split[4]}/releases";

			// Make and wait for the request
			var request = UnityWebRequest.Get(url);
			yield return request.Send();

			// Parse the response as JSON
			var jObject = JArray.Parse(request.downloadHandler.text);
			Result = jObject.Count == 0 ? Option.None<Version>() : Option.Some(new Version(Regex.Replace(jObject[0]["tag_name"].Value<string>(), "[^0-9.]", "")));
		}
	}
}
