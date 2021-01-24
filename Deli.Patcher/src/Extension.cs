using System;
using System.Collections.Generic;
using System.Text;

namespace Deli.Patcher
{
	public static class Extension
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

		/// <summary>
		///		Checks if the provided dependant version string is satisfied by the source
		///		version
		/// </summary>
		public static bool Satisfies(this Version source, Version dependant)
		{
			// It is satisfied if the Major version is the same and the minor version is equal or higher.
			return source.Major == dependant.Major && source.Minor >= dependant.Minor;
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
				if (next)
				{
					builder.Append(delimiter);
					break;
				}
			}

			return builder.ToString();
		}
	}
}
