using System;
using UnityEngine;

namespace Deli.Runtime.Yielding
{
	/// <summary>
	///		A method which extracts a result from an async operation
	/// </summary>
	/// <param name="operation">The operation to extract the result from</param>
	/// <typeparam name="TOperation">The type of the operation</typeparam>
	/// <typeparam name="TResult">The type of the result of <typeparamref name="TOperation"/></typeparam>
	public delegate TResult EndAsyncOperation<in TOperation, out TResult>(TOperation operation) where TOperation : AsyncOperation;

	/// <summary>
	///		A yield instruction which awaits an <see cref="AsyncOperation"/>
	/// </summary>
	public class AsyncOperationYieldInstruction : CustomYieldInstruction
	{
		private readonly AsyncOperation _operation;

		/// <inheritdoc cref="CustomYieldInstruction.keepWaiting"/>
		public override bool keepWaiting => !_operation.isDone;

		/// <summary>
		///		Creates an instance of <see cref="AsyncYieldInstruction{TSelf}"/>
		/// </summary>
		/// <param name="operation">The operation to await</param>
		public AsyncOperationYieldInstruction(AsyncOperation operation)
		{
			_operation = operation;
		}
	}

	/// <summary>
	///		A yield instruction which awaits a generic <see cref="AsyncOperation"/>
	/// </summary>
	/// <typeparam name="TOperation">The type of the <see cref="AsyncOperation"/></typeparam>
	/// <typeparam name="TResult">The type of the result of <typeparamref name="TOperation"/></typeparam>
	public class AsyncOperationYieldInstruction<TOperation, TResult> : ResultYieldInstruction<TResult> where TOperation : AsyncOperation
	{
		private readonly TOperation _operation;
		private readonly EndAsyncOperation<TOperation, TResult> _end;

		/// <inheritdoc cref="CustomYieldInstruction.keepWaiting"/>
		public override bool keepWaiting => !_operation.isDone;

		private bool _evaluated;
		private TResult? _result;
		/// <inheritdoc cref="ResultYieldInstruction{TResult}.Result"/>
		public override TResult Result
		{
			get
			{
				if (!_evaluated)
				{
					if (!_operation.isDone)
					{
						throw new InvalidOperationException("Async operation is not done.");
					}

					_result = _end(_operation);
					_evaluated = true;
				}

				return _result!;
			}
		}

		/// <summary>
		///		Creates an instance of <see cref="AsyncOperationYieldInstruction{TOperation,TResult}"/>
		/// </summary>
		/// <param name="operation">The operation to await</param>
		/// <param name="end">The method to get the result of the operation</param>
		public AsyncOperationYieldInstruction(TOperation operation, EndAsyncOperation<TOperation, TResult> end)
		{
			_operation = operation;
			_end = end;
		}
	}
}
