using System.Collections.Generic;
using Atlas;
using Atlas.Impl;
using Atlas.Fluent;

namespace Deli
{
    public class CachedResourceIO : IResourceIO
    {
        private readonly IResourceIO _raw;
        private readonly IServiceKernel _cache;

        public CachedResourceIO(IResourceIO raw)
        {
            _raw = raw;
            _cache = new StandardServiceKernel
            {
                MaxRecursion = Option.Some(1)
            };
        }

        public Option<T> Get<T>(string path)
        {
            if (!_cache.Get<IDictionary<string, Option<T>>>().MatchSome(out var typeCache))
            {
                typeCache = new Dictionary<string, Option<T>>();

                _cache.Bind<IDictionary<string, Option<T>>>()
                    .ToConstant(typeCache);
            }

            return typeCache.GetOrInsertWith(path, () => _raw.Get<T>(path));
        }
    }
}