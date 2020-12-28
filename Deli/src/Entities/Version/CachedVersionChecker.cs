using System;
using System.Collections;
using System.Collections.Generic;
using ADepIn;

namespace Deli
{
	public abstract class CachedVersionChecker : IVersionChecker
	{
		private static Dictionary<string, Option<Version>> _cachedVersions = new Dictionary<string, Option<Version>>();

		/// <summary>
		///		Result
		/// </summary>
		private Option<Option<Version>> _result;
		public Option<Version> Result
		{
			get => _result.Expect("The result has not yet been assigned.");
			protected set => _result = Option.Some(value);
		}

		/// <summary>
		///		Override method to actually await the result
		/// </summary>
		public IEnumerator Await()
		{
			if (_cachedVersions.TryGetValue(Url, out var result))
				Result = result;
			else yield return AwaitInternal();
		}

		protected abstract string Url { get; }

		protected abstract IEnumerator AwaitInternal();
	}
}
