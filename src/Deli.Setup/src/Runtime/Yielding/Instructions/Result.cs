using System;
using UnityEngine;

namespace Deli.Runtime.Yielding
{
	/// <summary>
	///		A yield instruction that contains a result on completion.
	/// </summary>
	/// <typeparam name="TResult">The type of the result.</typeparam>
	public abstract class ResultYieldInstruction<TResult> : CustomYieldInstruction
	{
		/// <summary>
		///		The result of operation.
		/// </summary>
		/// <exception cref="InvalidOperationException">The operation has not been completed.</exception>
		public abstract TResult Result { get; }

		/// <summary>
		///		Creates a super yield instruction which executes this instruction, then invokes a callback, and finally returns the result of this instruction
		/// </summary>
		/// <param name="callback">The callback to invoke after this instruction</param>
		public ResultYieldInstruction<TResult> CallbackWith(Action<TResult> callback)
		{
			return new VoidCallback(this, callback);
		}

		/// <summary>
		///		Creates a super yield instruction which executes this instruction, then returns the result of a callback
		/// </summary>
		/// <param name="callback">The result of the super instruction</param>
		/// <typeparam name="TNext">The type of the super instruction's result</typeparam>
		public ResultYieldInstruction<TNext> CallbackWith<TNext>(Func<TResult, TNext> callback)
		{
			return new ResultCallback<TNext>(this, callback);
		}

		/// <summary>
		///		Creates a super yield instruction which executes this instruction, then awaits a void operation determined by a callback, and finally returns the result of this
		///		instruction.
		///
		///		<para>
		///			If you wish for the return value to be data from the <see cref="AsyncOperation"/>, use <seealso cref="ContinueWith{TOperation,TNext}"/>.
		///		</para>
		/// </summary>
		/// <param name="continuation">The void operation that the super instruction should await</param>
		public ResultYieldInstruction<TResult> ContinueWith(Func<TResult, AsyncOperation> continuation)
		{
			return new VoidContinuation<AsyncOperationWrapper>(this, r => new AsyncOperationWrapper(continuation(r)));
		}

		/// <summary>
		///		Creates a super yield instruction which executes this instruction, then awaits a generic operation determined by a callback, and finally returns the result of a
		///		callback.
		/// </summary>
		/// <param name="continuation">The operation that the super instruction should await</param>
		/// <param name="result">The result of the super instruction</param>
		/// <typeparam name="TOperation">The type of the <see cref="AsyncOperation"/> to await</typeparam>
		/// <typeparam name="TNext">The type of the super instruction's result</typeparam>
		public ResultYieldInstruction<TNext> ContinueWith<TOperation, TNext>(Func<TResult, TOperation> continuation, Func<TOperation, TNext> result)
			where TOperation : AsyncOperation
		{
			return CallbackWith(continuation).ContinueWith(x => x).CallbackWith(result);
		}

		/// <summary>
		///		Creates a super yield instruction which executes this instruction, then executes an instruction determined by a callback, and finally returns the result of this
		///		instruction.
		/// </summary>
		/// <param name="continuation">The instruction that the super instruction should execute</param>
		public ResultYieldInstruction<TResult> ContinueWith(Func<TResult, CustomYieldInstruction> continuation)
		{
			return new VoidContinuation<CustomYieldWrapper>(this, r => new CustomYieldWrapper(continuation(r)));
		}

		/// <summary>
		///		Creates a super yield instruction which executes this instruction, then executes an instruction determined by a callback, and finally returns the result of the
		///		callback's instruction
		/// </summary>
		/// <param name="continuation">The instruction that the super instruction should execute</param>
		/// <typeparam name="TNext">The type of the super instruction's result</typeparam>
		public ResultYieldInstruction<TNext> ContinueWith<TNext>(Func<TResult, ResultYieldInstruction<TNext>> continuation)
		{
			return new ResultContinuation<TNext>(this, continuation);
		}

		/// <summary>
		///		Creates a super yield instruction which executes this instruction, then executes an async pattern, and finally returns the result of this instruction
		/// </summary>
		/// <param name="begin">The method to begin the async operation</param>
		/// <param name="end">The method to end the void async operation</param>
		public ResultYieldInstruction<TResult> ContinueWith(BeginAsync<TResult> begin, EndAsync<TResult> end)
		{
			return ContinueWith(result => new AsyncYieldInstruction<TResult>(result, begin, end));
		}

		/// <summary>
		///		Creates a super yield instruction which executes this instruction, then executes an async pattern, and finally returns the result of the async pattern
		/// </summary>
		/// <param name="begin">The method to begin the async operation</param>
		/// <param name="end">The method to end the non-void async operation</param>
		/// <typeparam name="TNext">The type of the super instruction's result</typeparam>
		public ResultYieldInstruction<TNext> ContinueWith<TNext>(BeginAsync<TResult> begin, EndAsync<TResult, TNext> end)
		{
			return ContinueWith(result => new AsyncYieldInstruction<TResult, TNext>(result, begin, end));
		}

		private class VoidCallback : ResultYieldInstruction<TResult>
		{
			private readonly ResultYieldInstruction<TResult> _inst;

			private Action<TResult>? _callback;

			public override bool keepWaiting
			{
				get
				{
					if (!_inst.keepWaiting)
					{
						if (_callback is not null)
						{
							_callback(_inst.Result);
							_callback = null;
						}

						return false;
					}

					return true;
				}
			}

			public override TResult Result => _inst.Result;

			public VoidCallback(ResultYieldInstruction<TResult> inst, Action<TResult> callback)
			{
				_inst = inst;
				_callback = callback;
			}
		}

		private class ResultCallback<TNext> : ResultYieldInstruction<TNext>
		{
			private readonly ResultYieldInstruction<TResult> _inst;
			private readonly Func<TResult, TNext> _callback;

			public override bool keepWaiting => _inst.keepWaiting;

			private bool _evaluated;
			private TNext? _result;
			public override TNext Result
			{
				get
				{
					if (!_evaluated)
					{
						_result = _callback(_inst.Result);
						_evaluated = true;
					}

					return _result!;
				}
			}

			public ResultCallback(ResultYieldInstruction<TResult> inst, Func<TResult, TNext> callback)
			{
				_inst = inst;
				_callback = callback;
			}
		}

		private class VoidContinuation<TCont> : ResultYieldInstruction<TResult> where TCont : struct, IYieldWrapper
		{
			private readonly ResultYieldInstruction<TResult> _inst;
			private readonly Func<TResult, TCont> _contFactory;

			private TCont? _cont;

			public override bool keepWaiting
			{
				get
				{
					if (!_cont.HasValue)
					{
						if (_inst.keepWaiting)
						{
							return true;
						}

						_cont = _contFactory(_inst.Result);
					}

					return _cont.Value.KeepWaiting;
				}
			}

			public override TResult Result => _inst.Result;

			public VoidContinuation(ResultYieldInstruction<TResult> inst, Func<TResult, TCont> contFactory)
			{
				_inst = inst;
				_contFactory = contFactory;
			}
		}

		private class ResultContinuation<TNext> : ResultYieldInstruction<TNext>
		{
			private readonly ResultYieldInstruction<TResult> _inst;
			private readonly Func<TResult, ResultYieldInstruction<TNext>> _contFactory;

			private ResultYieldInstruction<TNext>? _cont;

			public override bool keepWaiting
			{
				get
				{
					if (_cont is null)
					{
						if (_inst.keepWaiting)
						{
							return true;
						}

						_cont = _contFactory(_inst.Result);
					}

					return _cont.keepWaiting;
				}
			}

			public override TNext Result
			{
				get
				{
					if (_cont is null)
					{
						throw new InvalidOperationException("This instruction is not finished.");
					}

					return _cont.Result;
				}
			}

			public ResultContinuation(ResultYieldInstruction<TResult> inst, Func<TResult, ResultYieldInstruction<TNext>> contFactory)
			{
				_inst = inst;
				_contFactory = contFactory;
			}
		}
	}
}
