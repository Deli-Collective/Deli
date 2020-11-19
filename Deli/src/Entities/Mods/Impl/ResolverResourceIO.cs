using ADepIn;

namespace Deli
{
    public class ResolverResourceIO : IResourceIO
    {
        private readonly IRawIO _raw;
        private readonly IServiceResolver _readers;

        public ResolverResourceIO(IRawIO raw, IServiceResolver readers)
        {
            _raw = raw;
            _readers = readers;
        }

        public Option<T> Get<T>(string path)
        {
            return _raw[path].Map(data =>
            {
                var reader = _readers.Get<IAssetReader<T>>().Expect($"No asset reader found for {typeof(T)}");

                return reader.ReadAsset(data);
            });
        }
    }
}