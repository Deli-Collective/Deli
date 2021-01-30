using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Deli
{
	public class NestedServiceCollection<TKey1, TKey2, TService> : IEnumerable<KeyValuePair<TKey1, IEnumerable<KeyValuePair<TKey2, TService>>>>
	{
		private readonly Dictionary<TKey1, Dictionary<TKey2, TService>> _services = new();

		public bool TryGet(TKey1 key1, TKey2 key2, [MaybeNullWhen(false)] out TService service)
		{
			if (!_services.TryGetValue(key1, out var nested))
			{
				service = default;
				return false;
			}

			return nested.TryGetValue(key2, out service);
		}

		public TService this[TKey1 key1, TKey2 key2]
		{
			set
			{
				if (!_services.TryGetValue(key1, out var nested))
				{
					nested = new Dictionary<TKey2, TService>();
					_services.Add(key1, nested);
				}

				if (nested.ContainsKey(key2))
				{
					throw new ArgumentException($"A service under this secondary key ({key2}) has already been registered.", nameof(key2));
				}

				nested[key2] = value;
			}
		}

		public IEnumerator<KeyValuePair<TKey1, IEnumerable<KeyValuePair<TKey2, TService>>>> GetEnumerator()
		{
			return _services.Select(x => new KeyValuePair<TKey1, IEnumerable<KeyValuePair<TKey2, TService>>>(x.Key, x.Value)).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
