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
			return Directory.GetFiles(_root.FullName, "*.*", SearchOption.AllDirectories).Select(x => x.Replace(_root.FullName + "\\", "")).Where(p => regex.IsMatch(p));
		}
	}
}
