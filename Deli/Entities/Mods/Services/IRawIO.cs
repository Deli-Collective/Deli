using Atlas;

namespace Deli
{
    public interface IRawIO
    {
        Option<byte[]> this[string path] { get; }
    }
}