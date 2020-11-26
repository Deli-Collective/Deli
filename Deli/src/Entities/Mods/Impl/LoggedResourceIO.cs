using System.Collections.Generic;
using System.Linq;
using ADepIn;
using BepInEx.Logging;

namespace Deli
{
	public class LoggedModIO : IResourceIO
	{
		private readonly ManualLogSource _log;
		private readonly IResourceIO _resources;

		public LoggedModIO(ManualLogSource log, IResourceIO resources)
		{
			_log = log;
			_resources = resources;
		}

		public Option<T> Get<T>(string path)
		{
			var asset = _resources.Get<T>(path);
			_log.LogDebug($"Retrieving asset [{typeof(T)}: {path}]: {(asset.IsSome ? "OK" : "FAIL")}");
			return asset;
		}

		public IEnumerable<Option<T>> GetAll<T>(string pattern)
		{
			return Find(pattern).Select(Get<T>);
		}

		public IEnumerable<string> Find(string pattern)
		{
			return _resources.Find(pattern);
		}
	}
}
