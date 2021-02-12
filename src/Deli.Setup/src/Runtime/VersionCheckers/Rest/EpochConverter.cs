using System;

namespace Deli.Runtime
{
	internal static class EpochConverter
	{
		private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static DateTime FromUtc(ulong time)
		{
			return Epoch.AddSeconds(time);
		}

		public static ulong ToUtc(DateTime time)
		{
			return (ulong) (time - Epoch).TotalSeconds;
		}
	}
}
