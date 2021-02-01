using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Deli.Setup
{
	public class VersionCheckerCollection : IEnumerable<KeyValuePair<string, VersionChecker>>
	{
		private readonly Dictionary<string, VersionChecker> _checkers = new();

		public bool TryGet(string domain, [MaybeNullWhen(false)] out VersionChecker checker)
		{
			return _checkers.TryGetValue(domain, out checker);
		}

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
