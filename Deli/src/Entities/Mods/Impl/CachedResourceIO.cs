using System.Collections.Generic;
using System.Linq;
using ADepIn;
using ADepIn.Fluent;
using ADepIn.Impl;

namespace Deli
{
	internal class CachedResourceIO : IResourceIO
	{
		private readonly IServiceKernel _cache;
		private readonly IResourceIO _raw;

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

				_cache.Bind<IDictionary<string, Option<T>>>().ToConstant(typeCache);
			}

			return typeCache.GetOrInsertWith(path, () => _raw.Get<T>(path));
		}

		public IEnumerable<Option<T>> GetAll<T>(string pattern)
		{
			return Find(pattern).Select(Get<T>);
		}

		public IEnumerable<string> Find(string pattern)
		{
			return _raw.Find(pattern);
		}
	}
}
