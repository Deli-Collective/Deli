using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Deli.Runtime
{
	/// <summary>
	///		A collection of <see cref="VersionChecker"/>s, keyed by domain
	/// </summary>
	public class VersionCheckerCollection : IEnumerable<KeyValuePair<string, VersionChecker>>
	{
		private readonly Dictionary<string, VersionChecker> _checkers = new();

		/// <summary>
		///		Gets a checker, if it exists, from this collection. Otherwise, returns <see langword="false"/>.
		/// </summary>
		/// <param name="domain">The domain the checker corresponds to</param>
		/// <param name="checker">The checker, if it was found</param>
		/// <returns></returns>
		public bool TryGet(string domain, [MaybeNullWhen(false)] out VersionChecker checker)
		{
			return _checkers.TryGetValue(domain, out checker);
		}

		/// <summary>
		///		Sets a domain to use the given checker
		/// </summary>
		/// <param name="domain">The domain the checker is responsible for</param>
		/// <exception cref="ArgumentException">A checker is already registered for the domain</exception>
		public VersionChecker this[string domain]
		{
			set
			{
				if (_checkers.ContainsKey(domain))
				{
					throw new ArgumentException($"A version checker has already been registered for this domain ({domain}).", nameof(domain));
				}

				_checkers[domain] = value;
			}
		}

		/// <summary>
		///		Enumerates over all the checkers and their corresponding domains
		/// </summary>
		public IEnumerator<KeyValuePair<string, VersionChecker>> GetEnumerator()
		{
			return _checkers.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
