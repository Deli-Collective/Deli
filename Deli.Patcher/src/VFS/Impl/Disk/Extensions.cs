using System;
using System.IO;

namespace Deli.VFS.Disk
{
	public static class Extensions
	{
		internal static void ThrowIfDead(this IDiskHandle @this)
		{
			if (!@this.IsAlive())
			{
				throw new InvalidOperationException("Handle is dead.");
			}
		}

		public static bool IsAlive(this IDiskHandle @this)
		{
			return @this switch
			{
				IDirectoryHandle => Directory.Exists(@this.PathOnDisk),
				IFileHandle => File.Exists(@this.PathOnDisk),
				_ => throw new ArgumentException("Handle was neither a directory nor a file.", nameof(@this))
			};
		}
	}
}
