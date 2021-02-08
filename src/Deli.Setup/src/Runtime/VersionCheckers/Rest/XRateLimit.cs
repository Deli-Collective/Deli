using System;
using System.Collections.Generic;
using Deli.Runtime.Yielding;
using UnityEngine;
using UnityEngine.Networking;

namespace Deli.Runtime
{
	internal class XRateLimit
	{
		private readonly Queue<XRateLimitYieldInstruction> _yields = new();

		public ResetInfo? Reset { get; private set; }
		public uint Remaining { get; private set; }

		public HeaderInfo Headers { get; }

		public XRateLimit(HeaderInfo headers)
		{
			// Allow only one to get through
			Remaining = 1;
			Headers = headers;
		}

		private bool Consume()
		{
			if (Remaining == 0)
			{
				return false;
			}

			--Remaining;
			return true;
		}

		private bool EmptyQueue()
		{
			while (_yields.Count > 0)
			{
				if (!Consume())
				{
					return false;
				}

				_yields.Dequeue().Resolve();
			}

			return true;
		}

		private void TryReset()
		{
			if (Reset is null) return;

			var reset = Reset.Value;
			if (DateTime.UtcNow <= reset.ResetUtc) return;

			Remaining = reset.Limit;
			Reset = null;
		}

		private bool Cycle()
		{
			TryReset();

			return EmptyQueue();
		}

		private XRateLimitYieldInstruction Enqueue()
		{
			var yield = new XRateLimitYieldInstruction(this);
			_yields.Enqueue(yield);

			return yield;
		}

		public void Update(UnityWebRequest response)
		{
			var remaining = uint.Parse(response.GetResponseHeader(Headers.Remaining));
			var limit = uint.Parse(response.GetResponseHeader(Headers.Limit));
			var timeUtc = EpochConverter.FromUtc(ulong.Parse(response.GetResponseHeader(Headers.Reset)));

			// Listen to the server and take the most lenient limit; other applications might be biting into our rate limits.
			Remaining = Math.Min(Remaining, remaining);
			if (Reset is not null)
			{
				var reset = Reset.Value;
				limit = Math.Min(limit, reset.Limit);
				timeUtc = timeUtc > reset.ResetUtc ? timeUtc : reset.ResetUtc;
			}
			Reset = new ResetInfo(limit, timeUtc);
		}

		public CustomYieldInstruction Use()
		{
			return Cycle() && Consume() ? new DummyYieldInstruction() : Enqueue();
		}

		public class HeaderInfo
		{
			public static HeaderInfo Prefixed(string prefix, string limit, string remaining, string reset)
			{
				return new(prefix + limit, prefix + remaining, prefix + reset);
			}

			public string Limit { get; }
			public string Remaining { get; }
			public string Reset { get; }

			public HeaderInfo(string limit, string remaining, string reset)
			{
				Limit = limit;
				Remaining = remaining;
				Reset = reset;
			}
		}

		public readonly struct ResetInfo
		{
			public uint Limit { get; }
			public DateTime ResetUtc { get; }

			public ResetInfo(uint limit, DateTime resetUtc)
			{
				Limit = limit;
				ResetUtc = resetUtc;
			}
		}

		private class XRateLimitYieldInstruction : CustomYieldInstruction
		{
			private readonly XRateLimit _rateLimit;

			private bool _keepWaiting;
			public override bool keepWaiting
			{
				get
				{
					if (!_keepWaiting)
					{
						_rateLimit.Cycle();
					}

					return _keepWaiting;
				}
			}

			public XRateLimitYieldInstruction(XRateLimit rateLimit)
			{
				_rateLimit = rateLimit;
			}

			public void Resolve() => _keepWaiting = true;
		}
	}
}
