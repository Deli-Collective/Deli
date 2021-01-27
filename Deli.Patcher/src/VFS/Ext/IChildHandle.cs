using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Deli.VFS
{
	public static class ExtIChildHandle
	{
		private static IEnumerable<T> RecurseEnumerable<T>(T original, Func<T, T?> mut) where T : class
		{
			var buffer = original;
			do
			{
				yield return buffer;
				buffer = mut(buffer);
			} while (buffer is not null);
		}

		private static T RecurseAtomic<T>(T original, Func<T, T?> mut) where T : class
		{
			var buffer = original;
			var swap = mut(buffer);
			while (swap is not null)
			{
				buffer = swap;
				swap = mut(buffer);
			}

			return buffer;
		}

		private static TRet RecurseParents<TRet>(IChildHandle child, Func<IDirectoryHandle, Func<IDirectoryHandle, IDirectoryHandle?>, TRet> func)
		{
			return func(child.Directory, directory => (directory as IChildHandle)?.Directory);
		}

		public static IDirectoryHandle GetRoot(this IChildHandle @this)
		{
			return RecurseParents(@this, RecurseAtomic);
		}

		public static IEnumerable<IDirectoryHandle> GetAncestors(this IChildHandle @this)
		{
			return RecurseParents(@this, RecurseEnumerable);
		}

		public static IEnumerable<IHandle> GetPathing(this IChildHandle @this)
		{
			foreach (var ancestor in @this.GetAncestors().Reverse())
			{
				yield return ancestor;
			}
			yield return @this;
		}

		public static IChildHandle? WithName(this IChildHandle @this, string name)
		{
			return @this.Directory[name];
		}

		public static IChildHandle? WithStem(this IChildHandle @this, string stem)
		{
			return @this.WithName(stem + Path.GetExtension(@this.Name));
		}

		public static IChildHandle? WithExtension(this IChildHandle @this, string extension)
		{
			return @this.WithName(Path.ChangeExtension(@this.Name, extension));
		}

		public static IChildHandle? GetSibling(this IChildHandle @this, string name)
		{
			return @this.Directory[name];
		}

		public static bool IsChildOf(this IChildHandle @this, IDirectoryHandle parent)
		{
			return @this.GetAncestors().Contains(parent);
		}
	}
}
