using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ADepIn;
using Ionic.Zip;

namespace Deli
{
	internal class ArchiveRawIO : IRawIO
	{
		private readonly ZipFile _archive;

		public ArchiveRawIO(ZipFile archive)
		{
			_archive = archive;
		}

		public Option<byte[]> this[string path]
		{
			get
			{
				if (!_archive.ContainsEntry(path)) return Option.None<byte[]>();

				var entry = _archive[path];
				using var reader = entry.OpenReader();

				var buffer = new byte[entry.UncompressedSize];
				using (var memory = new MemoryStream(buffer))
				{
					reader.CopyTo(memory);
				}

				return Option.Some(buffer);
			}
		}

		public IEnumerable<string> Find(string pattern)
		{
			var regex = new Regex(pattern, RegexOptions.IgnoreCase);
			return _archive.EntryFileNames.Where(n => regex.IsMatch(n));
		}
	}
}
