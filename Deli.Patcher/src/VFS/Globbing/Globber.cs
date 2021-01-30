using System.Collections.Generic;

namespace Deli.VFS.Globbing
{
	public static class Globber
	{
		private static IGlobber? FromName(string name)
		{
			return name switch
			{
				"." => null,
				".." => new ParentGlobber(),
				"**" => new GlobstarGlobber(),
				var other => new NameGlobber(other)
			};
		}

		public static IGlobber? Create(string path)
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

			var globbers = new List<IGlobber>();

			var first = split[0] switch
			{
				"" => new RootGlobber(),
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
				"" => new CurrentGlobber(),
				var other => FromName(other)
			};
			if (last is not null)
			{
				globbers.Add(last);
			}

			return globbers.Count == 1 ? globbers[0] : new CompositeGlobber(globbers);
		}

		public static IEnumerable<IHandle> Glob(IDirectoryHandle directory, string path)
		{
			var glob = Create(path);
			if (glob is null)
			{
				yield return directory;
				yield break;
			}

			foreach (var match in glob.Matches(directory))
			{
				yield return match;
			}
		}
	}
}
