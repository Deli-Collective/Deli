using System;

namespace Deli.VFS
{
	internal static class ExtIHandle
	{
		public static void ThrowIfDead(this IHandle @this)
		{
			if (!@this.IsAlive)
			{
				throw new InvalidOperationException("Handle is dead.");
			}
		}
	}
}
