using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Deli.VFS.Globbing
{
	/// <summary>
	///		A method which enumerates over handles with specific qualities
	/// </summary>
	/// <param name="directory">The directory whose children should be enumerated</param>
	public delegate IEnumerable<IHandle> Globber(IDirectoryHandle directory);

	internal class CompositeGlobber
	{
		private readonly ICollection<Globber> _globs;

		public CompositeGlobber(ICollection<Globber> globs)
		{
			_globs = globs;
		}

		public IEnumerable<IHandle> Globber(IDirectoryHandle directory)
		{
			using var enumerator = _globs.GetEnumerator();

			IEnumerable<IDirectoryHandle> directories = new[] {directory};

			Globber? glob = null;
			if (enumerator.MoveNext())
			{
				while (true)
				{
					glob = enumerator.Current!;
					var next = enumerator.MoveNext();
					if (!next)
					{
						break;
					}

					var globC = glob;
					directories = directories.SelectMany(d => globC(d)).WhereCast<IHandle, IDirectoryHandle>();
				}
			}

			return glob is null ? directories.Cast<IHandle>() : directories.SelectMany(d => glob(d));
		}
	}

	internal class NameGlobber
	{
		private static void ApplyGlobs(string value, int start, int length, StringBuilder result, List<GlobberFactory.NameReplacementEntry> nameReplacementEntries)
		{
			if (length == 0) return;

			foreach (var glob in nameReplacementEntries)
			{
				var match = glob.Filter.Match(value, start, length);
				if (!match.Success) continue;

				if (!glob.MatchAllowed(out var replacement))
				{
					throw new ArgumentException($"Name replacement glob not allowed: '{match.Value}'", nameof(value));
				}

				var groups = match.Groups;
				var groupCount = groups.Count - 1;

				// Parse before the match
				ApplyGlobs(value, start, match.Index - start, result, nameReplacementEntries);

				// Replace match
				if (groupCount == 0)
				{
					result.Append(replacement);
				}
				else
				{
					var parameters = new object[groupCount];
					for (var i = 0; i < parameters.Length; ++i)
					{
						parameters[i] = match.Groups[i + 1].Value;
					}

					result.AppendFormat(replacement, parameters);
				}

				// Parse after the match
				var end = match.Index + match.Length;
				ApplyGlobs(value, end, value.Length - end, result, nameReplacementEntries);

				return;
			}

			var raw = value.Substring(start, length);
			var escaped = Regex.Escape(raw);
			result.Append(escaped);
		}

		private readonly Regex _regex;

		public NameGlobber(string name, List<GlobberFactory.NameReplacementEntry> nameReplacements)
		{
			var builder = new StringBuilder();
			ApplyGlobs(name, 0, name.Length, builder, nameReplacements);

			_regex = new Regex(builder.ToString());
		}

		public IEnumerable<IHandle> Globber(IDirectoryHandle directory)
		{
			return directory.Where(c => _regex.IsMatch(c.Name)).Cast<IHandle>();
		}
	}

	internal static class StatelessGlobbers
	{
		public static IEnumerable<IHandle> Root(IDirectoryHandle directory)
		{
			yield return directory.GetRoot();
		}

		public static IEnumerable<IHandle> Globstar(IDirectoryHandle directory)
		{
			yield return directory;
			foreach (var child in directory.GetRecursive()) yield return child;
		}

		public static IEnumerable<IHandle> Parent(IDirectoryHandle directory)
		{
			if (directory is not IChildDirectoryHandle child)
			{
				throw new InvalidOperationException("A parent glob was used on the root directory.");
			}

			yield return child.Directory;
		}

		public static IEnumerable<IHandle> Current(IDirectoryHandle directory)
		{
			yield return directory;
		}
	}
}
