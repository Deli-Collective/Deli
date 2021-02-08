using System.Collections.Generic;

namespace Deli.VFS.Globbing
{
	public static class GlobFactory
	{
		private static Globber? FromName(string name)
		{
			return name switch
			{
				"." => null,
				".." => StatelessGlobbers.Parent,
				"**" => StatelessGlobbers.Globstar,
				var other => new NameGlobber(other).Globber
			};
		}

		public static Globber? Create(string path)
		{
			if (path.Length == 0)
			{
				return null;
			}

			var split = path.Split('/');
			var length = split.Length;

			if (length == 1)
			{
				return FromName(split[0]);
			}

			var globbers = new List<Globber>();

			var first = split[0] switch
			{
				"" => StatelessGlobbers.Root,
				var name => FromName(name)
			};
			if (first is not null)
			{
				globbers.Add(first);
			}

			for (var i = 1; i < length - 1; ++i)
			{
				var current = FromName(split[i]);
				if (current is not null)
				{
					globbers.Add(current);
				}
			}

			var last = split[length - 1] switch
			{
				"" => StatelessGlobbers.Current,
				var other => FromName(other)
			};
			if (last is not null)
			{
				globbers.Add(last);
			}

			return globbers.Count == 1 ? globbers[0] : new CompositeGlobber(globbers).Globber;
		}

		public static IEnumerable<IHandle> Glob(IDirectoryHandle directory, string path)
		{
			var glob = Create(path);
			if (glob is null)
			{
				yield return directory;
				yield break;
			}

			foreach (var match in glob(directory))
			{
				yield return match;
			}
		}
	}
}
