using System.Collections.Generic;
using System.Linq;
using Deli.Patcher;
using Deli.VFS.Globbing;

namespace Deli.VFS
{
	public static class ExtIDirectoryHandle
	{
		public static IFileHandle? GetFile(this IDirectoryHandle @this, string name)
		{
			return @this[name] as IFileHandle;
		}

		public static IChildDirectoryHandle? GetDirectory(this IDirectoryHandle @this, string name)
		{
			return @this[name] as IChildDirectoryHandle;
		}

		public static IEnumerable<IChildHandle> GetRecursive(this IDirectoryHandle @this)
		{
			foreach (var child in @this)
			{
				if (child is IDirectoryHandle subdirectory)
				{
					foreach (var subchild in subdirectory.GetRecursive())
					{
						yield return subchild;
					}
				}

				yield return child;
			}
		}

		public static IEnumerable<IChildDirectoryHandle> GetDirectories(this IDirectoryHandle @this)
		{
			return @this.WhereCast<IChildHandle, IChildDirectoryHandle>();
		}

		public static IEnumerable<IChildDirectoryHandle> GetDirectoriesRecursive(this IDirectoryHandle @this)
		{
			foreach (var directory in @this.GetDirectories())
			{
				yield return directory;

				foreach (var subdirectory in directory.GetDirectoriesRecursive())
				{
					yield return subdirectory;
				}
			}
		}

		public static IEnumerable<IFileHandle> GetFiles(this IDirectoryHandle @this)
		{
			return @this.WhereCast<IChildHandle, IFileHandle>();
		}

		public static IEnumerable<IFileHandle> GetFilesRecursive(this IDirectoryHandle @this)
		{
			foreach (var child in @this)
			{
				if (child is IDirectoryHandle subdirectory)
				{
					foreach (var subfile in subdirectory.GetFilesRecursive())
					{
						yield return subfile;
					}
				}

				if (child is IFileHandle file)
				{
					yield return file;
				}
			}
		}

		public static IEnumerable<IHandle> Glob(this IDirectoryHandle @this, string path)
		{
			return Globber.Glob(@this, path);
		}

		public static IDirectoryHandle GetRoot(this IDirectoryHandle @this)
		{
			return @this is IChildHandle child ? child.GetRoot() : @this;
		}

		public static bool IsParentOf(this IDirectoryHandle @this, IChildHandle child)
		{
			return child.IsChildOf(@this);
		}
	}
}
