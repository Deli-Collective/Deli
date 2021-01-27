using System.Collections.Generic;

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

		public static IEnumerable<IChildHandle> GetLeafs(this IDirectoryHandle @this)
		{
			foreach (var child in @this)
			{
				if (child is IDirectoryHandle subdirectory)
				{
					foreach (var subchild in subdirectory.GetLeafs())
					{
						yield return subchild;
					}
				}
				else
				{
					yield return child;
				}
			}
		}

		public static IEnumerable<IChildHandle> GetChildren(this IDirectoryHandle @this)
		{
			foreach (var child in @this)
			{
				yield return child;

				if (child is IDirectoryHandle subdirectory)
				{
					foreach (var subchild in subdirectory.GetChildren())
					{
						yield return subchild;
					}
				}
			}
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
