using System;
using UnityEngine;

namespace Deli.Runtime.Yielding
{
	public delegate IAsyncResult BeginAsync<in T>(T self, AsyncCallback callback, object state);

	public delegate void EndAsync<in TState>(TState self, IAsyncResult result);
	public delegate TResult EndAsync<in TState, out TResult>(TState self, IAsyncResult result);

	public class AsyncYieldInstruction<TSelf> : CustomYieldInstruction
	{
		private readonly TSelf _state;
		private readonly EndAsync<TSelf> _end;
		private readonly IAsyncResult _async;

		public override bool keepWaiting => !_async.IsCompleted;

		public AsyncYieldInstruction(TSelf state, BeginAsync<TSelf> begin, EndAsync<TSelf> end)
		{
			_state = state;
			_end = end;

			_async = begin(_state, Callback, this);
		}

		private void Callback(IAsyncResult result)
		{
			_end(_state, result);
		}
	}

	public class AsyncYieldInstruction<TSelf, TResult> : ResultYieldInstruction<TResult>
	{
		private readonly TSelf _state;
		private readonly EndAsync<TSelf, TResult> _end;
		private readonly IAsyncResult _async;

		public override bool keepWaiting => !_async.IsCompleted;

		private TResult? _result;
		public override TResult Result => _result ?? throw new InvalidOperationException("Async operation has not yet been completed.");

		public AsyncYieldInstruction(TSelf state, BeginAsync<TSelf> begin, EndAsync<TSelf, TResult> end)
		{
			_state = state;
			_end = end;

			_async = begin(_state, Callback, this);
		}

		private void Callback(IAsyncResult result)
		{
			_result = _end(_state, result);
		}
	}
}
