using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Deli
{
	/// <summary>
	///		A collection of objects, each keyed by a primary and secondary key
	/// </summary>
	public class NestedServiceCollection<TKey1, TKey2, TService> : IEnumerable<KeyValuePair<TKey1, IEnumerable<KeyValuePair<TKey2, TService>>>>
	{
		private readonly Dictionary<TKey1, Dictionary<TKey2, TService>> _services = new();

		/// <summary>
		///		Gets a service in the collection. If not found, returns <see langword="false"/>.
		/// </summary>
		/// <param name="key1">The primary key of the service</param>
		/// <param name="key2">The secondary key of the service</param>
		/// <param name="service">The service, if it was found</param>
		public bool TryGet(TKey1 key1, TKey2 key2, [MaybeNullWhen(false)] out TService service)
		{
			if (!_services.TryGetValue(key1, out var nested))
			{
				service = default;
				return false;
			}

			return nested.TryGetValue(key2, out service);
		}

		/// <summary>
		///		Gets or sets a service in this collection
		/// </summary>
		/// <param name="key1">The primary key of the service</param>
		/// <param name="key2">The secondary key of the service</param>
		/// <exception cref="KeyNotFoundException">The primary/secondary key did not exist when getting a service</exception>
		public virtual TService this[TKey1 key1, TKey2 key2]
		{
			get
			{
				if (!_services.TryGetValue(key1, out var nested))
				{
					throw new KeyNotFoundException($"Services under the primary key ({key1}) were not found.");
				}

				if (!nested.TryGetValue(key2, out var item))
				{
					throw new KeyNotFoundException($"A service under the secondary key ({key2}) was not found.");
				}

				return item;
			}
			set
			{
				if (!_services.TryGetValue(key1, out var nested))
				{
					nested = new Dictionary<TKey2, TService>();
					_services.Add(key1, nested);
				}

				nested[key2] = value;
			}
		}

		/// <summary>
		///		Enumerates over all of the services, with their corresponding secondary key, with those services' corresponding primary key, in this collection
		/// </summary>
		/// <returns></returns>
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
