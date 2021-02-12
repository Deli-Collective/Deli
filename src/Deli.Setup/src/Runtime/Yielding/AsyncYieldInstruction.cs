using System;
using UnityEngine;

namespace Deli.Runtime.Yielding
{
	/// <summary>
	///		A method which begins an async operation
	/// </summary>
	/// <param name="self">The object which implements the pattern</param>
	/// <param name="callback">The callback to pass to the begin method of the pattern</param>
	/// <param name="state">The state to pass to the begin method of the pattern</param>
	/// <typeparam name="TSelf">The type of the object which implements the pattern</typeparam>
	public delegate IAsyncResult BeginAsync<in TSelf>(TSelf self, AsyncCallback callback, object state);

	/// <summary>
	///		A method which ends a void async operation
	/// </summary>
	/// <param name="self">The object which implements the pattern</param>
	/// <param name="result">The result to pass to the end method of the pattern</param>
	/// <typeparam name="TSelf">The type of the object which implements the pattern</typeparam>
	public delegate void EndAsync<in TSelf>(TSelf self, IAsyncResult result);

	/// <summary>
	///		A method which ends a non-void async operation
	/// </summary>
	/// <param name="self">The object which implements the pattern</param>
	/// <param name="result">The result to pass to the end method of the pattern</param>
	/// <typeparam name="TSelf">The type of the object which implements the pattern</typeparam>
	/// <typeparam name="TResult">The type of the return value of the pattern</typeparam>
	public delegate TResult EndAsync<in TSelf, out TResult>(TSelf self, IAsyncResult result);

	/// <summary>
	///		A yield instruction which awaits a void .NET async operation (begin/end pattern)
	/// </summary>
	/// <typeparam name="TSelf">The type of the object which performs the async operation</typeparam>
	public class AsyncYieldInstruction<TSelf> : CustomYieldInstruction
	{
		private readonly TSelf _self;
		private readonly EndAsync<TSelf> _end;
		private readonly IAsyncResult _async;

		/// <inheritdoc cref="CustomYieldInstruction.keepWaiting"/>
		public override bool keepWaiting => !_async.IsCompleted;

		/// <summary>
		///		Creates an instance of <see cref="AsyncYieldInstruction{TSelf}"/>
		/// </summary>
		/// <param name="self">The object which performs the async operation</param>
		/// <param name="begin">The method to begin the async operation</param>
		/// <param name="end">The method to end the void async operation</param>
		public AsyncYieldInstruction(TSelf self, BeginAsync<TSelf> begin, EndAsync<TSelf> end)
		{
			_self = self;
			_end = end;

			_async = begin(_self, Callback, this);
		}

		private void Callback(IAsyncResult result)
		{
			_end(_self, result);
		}
	}

	/// <summary>
	///		A yield instruction which awaits a non-void .NET async operation (begin/end pattern)
	/// </summary>
	/// <typeparam name="TSelf">The type of the object which performs the async operation</typeparam>
	/// <typeparam name="TResult">The type of the return value of the async operation</typeparam>
	public class AsyncYieldInstruction<TSelf, TResult> : ResultYieldInstruction<TResult>
	{
		private readonly TSelf _self;
		private readonly EndAsync<TSelf, TResult> _end;
		private readonly IAsyncResult _async;

		/// <inheritdoc cref="CustomYieldInstruction.keepWaiting"/>
		public override bool keepWaiting => !_async.IsCompleted;

		private TResult? _result;
		/// <inheritdoc cref="ResultYieldInstruction{TResult}.Result"/>
		public override TResult Result => _result ?? throw new InvalidOperationException("Async operation has not yet been completed.");

		/// <summary>
		///		Creates an instance of <see cref="AsyncYieldInstruction{TSelf,TResult}"/>
		/// </summary>
		/// <param name="self">The object which performs the async operation</param>
		/// <param name="begin">The method to begin the async operation</param>
		/// <param name="end">The method to end the non-void async operation</param>
		public AsyncYieldInstruction(TSelf self, BeginAsync<TSelf> begin, EndAsync<TSelf, TResult> end)
		{
			_self = self;
			_end = end;

			_async = begin(_self, Callback, this);
		}

		private void Callback(IAsyncResult result)
		{
			_result = _end(_self, result);
		}
	}
}
