using System;
using System.Collections.Generic;
using Semver;

namespace Deli.Runtime
{
	internal class VersionCache
	{
		public Dictionary<string, Timestamped<SemVersion?>> Cached { get; }

		private TimeSpan _cacheDuration;
		public TimeSpan CacheDuration
		{
			get => _cacheDuration;
			set
			{
				if (value <= TimeSpan.Zero)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "Duration must be positive.");
				}

				_cacheDuration = value;
			}
		}

		public VersionCache(Dictionary<string, Timestamped<SemVersion?>> cached)
		{
			Cached = cached;
			CacheDuration = TimeSpan.FromHours(1);
		}

		public Timestamped<SemVersion?>? this[string path]
		{
			get
			{
				if (Cached.TryGetValue(path, out var cache))
				{
					if (DateTime.UtcNow - cache.TimeUtc <= CacheDuration)
					{
						return cache;
					}

					Cached.Remove(path);
				}

				return null;
			}
			set
			{
				if (value is null)
				{
					Cached.Remove(path);
				}
				else
				{
					Cached[path] = value.Value;
				}
			}
		}
	}
}
