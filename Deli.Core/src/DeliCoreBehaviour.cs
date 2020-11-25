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
		private void Awake()
		{
			// Get our list of version checkers
			var versionCheckers = Deli.Services.Get<IDictionary<string, IVersionChecker>>().Expect("Missing version checker dict");
			versionCheckers.Add("https://github.com", new GitHubVersionChecker());
		}
	}
}
