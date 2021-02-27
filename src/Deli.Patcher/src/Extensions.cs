using System;
using System.Collections.Generic;
using System.Text;
using Semver;

namespace Deli
{
	internal static class Extensions
	{
		public static IEnumerable<T> TSort<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> dependencies, bool throwOnCycle = false)
		{
			var sorted = new List<T>();
			var visited = new HashSet<T>();

			foreach (var item in source)
				Visit(item, visited, sorted, dependencies, throwOnCycle);

			return sorted;
		}

		// https://stackoverflow.com/a/11027096/8809017
		private static void Visit<T>(T item, HashSet<T> visited, List<T> sorted, Func<T, IEnumerable<T>> dependencies, bool throwOnCycle)
		{
			if (!visited.Contains(item))
			{
				visited.Add(item);

				foreach (var dep in dependencies(item))
					Visit(dep, visited, sorted, dependencies, throwOnCycle);

				sorted.Add(item);
			}
			else
			{
				if (throwOnCycle && !sorted.Contains(item))
					throw new Exception("Cyclic dependency found");
			}
		}

		// .NET Framework 3.5's string.Join(...) is array only...
		/// <summary>
		///		Joins a string enumerable with a delimiter.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="delimiter">The string to insert between elements.</param>
		public static string JoinStr(this IEnumerable<string> @this, string delimiter)
		{
			using var enumerator = @this.GetEnumerator();

			bool next = enumerator.MoveNext();
			if (!next)
			{
				return string.Empty;
			}

			var builder = new StringBuilder();
			while (true)
			{
				builder.Append(enumerator.Current);

				next = enumerator.MoveNext();
				if (!next)
				{
					break;
				}

				builder.Append(delimiter);
			}

			return builder.ToString();
		}

		public static IEnumerable<TCast> ImplicitCast<T, TCast>(this IEnumerable<T> @this) where T : TCast
		{
			// Don't use LINQ cast, this will break enumerators that use this method.
			foreach (var item in @this) yield return item;
		}

		public static IEnumerable<TCast> WhereCast<T, TCast>(this IEnumerable<T> @this) where TCast : T
		{
			foreach (var item in @this)
			{
				if (item is TCast casted)
				{
					yield return casted;
				}
			}
		}

		public static bool Satisfies(this SemVersion @this, SemVersion requirement)
		{
			if (requirement.Major == 0)
			{
				return @this.Major == 0 && @this.Minor == requirement.Minor;
			}

			return @this.Major == requirement.Major && @this.Minor > requirement.Minor;
		}

		public static IEnumerable<T> RecurseEnumerable<T>(this T original, Func<T, T?> mut) where T : class
		{
			var buffer = original;
			do
			{
				yield return buffer;
				buffer = mut(buffer);
			} while (buffer is not null);
		}

		public static T RecurseAtomic<T>(this T original, Func<T, T?> mut) where T : class
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
	}
}
