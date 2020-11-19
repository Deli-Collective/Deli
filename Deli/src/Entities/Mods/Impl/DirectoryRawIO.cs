using System.IO;
using Atlas;

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
                    var buffer = new byte[file.Length];

                    using (var memory = new MemoryStream(buffer))
                    {
                        reader.CopyTo(memory);
                    }

                    return Option.Some(buffer);
                }
            }
        }
    }
}