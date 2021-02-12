using System.Collections.Generic;
using System.IO;

namespace Deli.VFS
{
	/// <summary>
	///		Extension methods pertaining to <see cref="IChildHandle"/>
	/// </summary>
	public static class ExtIChildHandle
	{
		/// <summary>
		///		Gets the root directory of the VFS that the handle resides in
		/// </summary>
		public static IDirectoryHandle GetRoot(this IChildHandle @this)
		{
			return @this.RecurseAtomic(c => c.Directory as IChildHandle).Directory;
		}

		/// <summary>
		///		Enumerates over all the directories that contain the handle, recursively.
		///		Enumeration begins with the handle's parent and ends with root, so you may need to reverse it.
		/// </summary>
		public static IEnumerable<IDirectoryHandle> GetAncestors(this IChildHandle @this)
		{
			return @this.Directory.RecurseEnumerable(c => (c as IChildHandle)?.Directory);
		}

		/// <summary>
		///		Returns the stem of the handle name
		/// </summary>
		public static string GetStem(this IChildHandle @this)
		{
			return Path.GetFileNameWithoutExtension(@this.Name);
		}

		/// <summary>
		///		Returns the extension of the handle name. If it does not have an extension, returns <see langword="null"/>.
		/// </summary>
		public static string? GetExtension(this IChildHandle @this)
		{
			var extension = Path.GetExtension(@this.Name);
			return extension?.Length > 0 ? extension.Substring(1) : null;
		}

		/// <summary>
		///		Returns a handle in the same directory with the given name. If not found, returns <see langword="null"/>.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="name">The name of the handle to retrieve</param>
		public static IChildHandle? WithName(this IChildHandle @this, string name)
		{
			return @this.Directory[name];
		}

		/// <summary>
		///		Returns a handle in the same directory with the same extension, but with the given stem. If not found, returns <see langword="null"/>.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="stem">The stem of the handle to retrieve</param>
		public static IChildHandle? WithStem(this IChildHandle @this, string stem)
		{
			return @this.WithName($"{stem}.{@this.GetExtension()}");
		}

		/// <summary>
		///		Returns a handle in the same directory with the same stem, but with the given extension. If not found, returns <see langword="null"/>.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="extension">The extension, which should NOT have a leading '.', of the handle to retrieve</param>
		public static IChildHandle? WithExtension(this IChildHandle @this, string extension)
		{
			return @this.WithName($"{@this.GetStem()}.{extension}");
		}
	}
}
