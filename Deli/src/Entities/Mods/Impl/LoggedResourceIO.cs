using System;
using System.Collections.Generic;
using System.Linq;
using ADepIn;
using BepInEx.Logging;

namespace Deli
{
	internal class LoggedModIO : IResourceIO
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
			Option<T> asset;
			try
			{
				asset = _resources.Get<T>(path);
			}
			catch (Exception e)
			{
				throw new Exception($"Failed to retrieve asset at {path}", e);
			}

			_log.LogDebug($"Retrieved asset at {path}: {asset}");

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
