using System.Collections.Generic;

namespace Deli.VFS.Globbing
{
	/// <summary>
	///		Extension methods pertaining to <see cref="IDirectoryHandle"/>
	/// </summary>
	public static class ExtIDirectoryHandle
	{
		private static readonly GlobberFactory DefaultGlobberFactory = new();

		/// <summary>
		///		Enumerates over all the handles that match the path.
		///		For more finite control over the globs allowed, use and configure a <see cref="GlobberFactory"/>.
		///		If you are calling multiple times with the same path, use <see cref="GlobberFactory.Create"/> to create a reusable, efficient glob method.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="path">The path which may contain globs and path separators</param>
		public static IEnumerable<IHandle> Glob(this IDirectoryHandle @this, string path)
		{
			return DefaultGlobberFactory.Glob(@this, path);
		}
	}
}
