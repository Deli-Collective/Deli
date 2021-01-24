using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Deli.VFS
{
	public static class ExtIChildHandle
	{
		private static IEnumerable<T> Recurse<T>(T original, Func<T, T?> mut) where T : class
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

		public static IDirectoryHandle GetRoot(this IChildHandle @this)
		{
			return RecurseAtomic(@this.Directory, v => (v as IChildHandle)?.Directory);
		}

		public static IEnumerable<IDirectoryHandle> GetAncestors(this IChildHandle @this)
		{
			return Recurse(@this.Directory, d => (d as IChildHandle)?.Directory);
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

		public static string GetPath(this IChildHandle @this)
		{
			using var enumerator = @this.GetPathing().GetEnumerator();

			// Skip root and verify it was more than root
			if (!enumerator.MoveNext() && !enumerator.MoveNext())
			{
				throw new InvalidOperationException("The number of nodes in the pathing of the handle is <2. Something is very wrong with this handle or its ancestors.");
			}

			var builder = new StringBuilder();
			do
			{
				builder.Append('/');
				if (enumerator.Current is INamedHandle named)
				{
					builder.Append(named.Name);
				}
			} while (enumerator.MoveNext());

			return builder.ToString();
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
