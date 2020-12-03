using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ADepIn;

namespace Deli
{
	internal class DirectoryRawIO : IRawIO
	{
		private readonly DirectoryInfo _root;

		public DirectoryRawIO(DirectoryInfo root)
		{
			_root = root;
		}

		public Option<byte[]> this[string path]
		{
			get
			{
				var relativePath = Path.Combine(_root.FullName, path);
				var file = new FileInfo(relativePath);
				if (!file.Exists) return Option.None<byte[]>();

				// I *could* use File.ReadAllBytes here, but then I would be getting and passing the path instead of a handle (possible source of error).
				using (var reader = file.OpenRead())
				{
					using (var memory = new MemoryStream())
					{
						reader.CopyTo(memory);
						return Option.Some(memory.ToArray());
					}
				}
			}
		}

		public IEnumerable<string> Find(string pattern)
		{
			var regex = new Regex(pattern, RegexOptions.IgnoreCase);
			var dirs = _root.GetDirectories("*", SearchOption.AllDirectories).Select(x => x.FullName + "/");
			var files = _root.GetFiles("*", SearchOption.AllDirectories).Select(x => x.FullName);
			var rootLength = _root.FullName.Length + 1;

			return dirs.Concat(files)
				.Select(x => x.Substring(rootLength).Replace('\\', '/'))
				.Where(x => regex.IsMatch(x));
		}
	}
}
