using System.IO;
using ADepIn;

namespace Deli
{
	public class DirectoryRawIO : IRawIO
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
	}
}
