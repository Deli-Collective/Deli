using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Deli.Patcher;

namespace Deli.VFS
{
	public class Globber
	{
		private static readonly Regex _globInterpreter;
		private static readonly Dictionary<int, GlobType> _globTypes;

		static Globber()
		{
			_globTypes = new();

			var groupOffset = 0;
			void Create(string pattern, string replacement)
			{
				var glob = new GlobType(pattern, replacement, groupOffset);
				groupOffset += 1 + glob.GroupCount; // +1 for the full matcher (to index the glob that was matched).

				_globTypes.Add(groupOffset, glob);
			}

			Create(@"(?<!\\)\/?\*\*", @".*[^\/]");
			Create(@"(?<!\\)\*", @"[^\/]*");
			Create(@"(?<!\\)\?", @"[^\/]");
			Create(@"\[(.+)\]", @"[{0}]");
			Create(@"\[(.+)-(.+)\]", @"[{0}-{1}]");

			var coalescedPattern = _globTypes.Values.Select(p => "(" + p.Pattern + ")").JoinStr("|");
			_globInterpreter = new Regex(coalescedPattern, RegexOptions.Compiled);
		}

		private static GlobType GetGlobType(GroupCollection groups)
		{
			for (var i = 0; i < groups.Count; ++i)
			{
				if (groups[i].Success)
				{
					return _globTypes[i];
				}
			}

			throw new NotSupportedException("This method should only be called if the filter was a match.");
		}

		private static Regex Interpret(string path)
		{
			var builder = new StringBuilder();
			var lastIndex = 0;

			void AppendSinceLast(int index, int count)
			{
				var oldLastIndex = lastIndex;
				lastIndex = index + count;

				var length = lastIndex - oldLastIndex;
				// Some unmatched characters since last match
				if (length > 0)
				{
					var sinceLast = path.Substring(lastIndex, length);
					builder.Append(Regex.Escape(sinceLast));
				}
			}

			void AppendMatch(Match match)
			{
				var groups = match.Groups;
				var glob = GetGlobType(groups);
				var subst = new object[glob.GroupCount];
				for (var i = 0; i < subst.Length; ++i)
				{
					subst[i] = groups[glob.GroupOffset + i].Value;
				}

				builder.Append(string.Format(glob.Replacement, subst));
			}

			builder.Append('^');
			foreach (Match match in _globInterpreter.Matches(path))
			{
				AppendSinceLast(match.Index, match.Length);
				AppendMatch(match);
			}
			AppendSinceLast(path.Length - 1, 0);
			builder.Append("(?:\\/)?"); // Also match directories
			builder.Append('$');

			return new Regex(builder.ToString());
		}

		public Regex? Pattern { get; }

		public bool DirectoriesOnly { get; }

		public Globber(string path)
		{
			// Returns directory it is given
			if (path.Length == 0)
			{
				Pattern = null;
				DirectoriesOnly = true;
				return;
			}

			// Mods don't need absolute paths, they can just glob from root directory.
			// If a mod has a user input paths, they could input a root path and access files that shouldn't be.
			// Not much of a point in allowing it.
			if (path[0] == '/')
			{
				throw new ArgumentOutOfRangeException(nameof(path), path, "Absolute paths are not supported by design.");
			}

			DirectoriesOnly = path[path.Length - 1] == '/';

			var suffix = DirectoriesOnly ? 1 : 0;
			var trimmed = path.Substring(0, path.Length - suffix);
			Pattern = Interpret(trimmed);
		}

		public IEnumerable<IHandle> Glob(IDirectoryHandle root)
		{
			if (Pattern is null)
			{
				yield return root;
				yield break;
			}

			var children = root.GetChildren();
			if (DirectoriesOnly)
			{
				children = children.Where(c => c is IDirectoryHandle);
			}

			var startAt = root.Path.Length;
			foreach (var child in children)
			{
				if (Pattern.IsMatch(child.Path, startAt))
				{
					yield return child;
				}
			}
		}

		private readonly struct GlobType
		{
			public readonly string Pattern;
			public readonly string Replacement;
			public readonly int GroupOffset;
			public readonly int GroupCount;

			public GlobType(string pattern, string replacement, int groupOffset)
			{
				Pattern = pattern;
				Replacement = replacement;
				GroupOffset = groupOffset;
				GroupCount = new Regex(pattern).GetGroupNumbers().Length;
			}
		}
	}
}
