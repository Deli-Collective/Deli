using Atlas;

namespace Deli
{
    public class NormalizeRawIO : IRawIO
    {
        private readonly IRawIO _raw;
        private readonly char _from;
        private readonly char _to;

        public NormalizeRawIO(IRawIO raw, char from, char to)
        {
            _raw = raw;
            _from = from;
            _to = to;
        }

        public Option<byte[]> this[string path] => _raw[path.Replace(_from, _to)];
    }
}