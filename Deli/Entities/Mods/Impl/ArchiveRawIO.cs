using System.IO;
using Atlas;
using Ionic.Zip;

namespace Deli
{
    public class ArchiveRawIO : IRawIO
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
                if (!_archive.ContainsEntry(path))
                {
                    return Option.None<byte[]>();
                }

                var entry = _archive[path];
                using (var reader = entry.OpenReader())
                {
                    var buffer = new byte[entry.UncompressedSize];

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