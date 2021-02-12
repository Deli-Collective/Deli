using System;
using UnityEngine;

namespace Deli.Runtime.Yielding
{
	/// <summary>
	///		Extension methods pertaining to <see cref="YieldInstruction"/>
	/// </summary>
	public static class ExtYieldInstruction
	{
		#region Wrapper

		private static CustomYieldInstruction CallbackWith<TWrapper>(this TWrapper @this, Action callback) where TWrapper : IYieldWrapper
		{
			return new VoidCallback<TWrapper>(@this, callback);
		}

		private static ResultYieldInstruction<TResult> CallbackWith<TWrapper, TResult>(this TWrapper @this, Func<TResult> callback) where TWrapper : IYieldWrapper
		{
			return new ResultCallback<TWrapper, TResult>(@this, callback);
		}

		private static CustomYieldInstruction ContinueWith<TWrapper, TContWrapper>(this TWrapper @this, Func<TContWrapper> continuation)
			where TWrapper : IYieldWrapper where TContWrapper : struct, IYieldWrapper
		{
			return new VoidContinuation<TWrapper, TContWrapper>(@this, continuation);
		}

		private static ResultYieldInstruction<TResult> ContinueWith<TWrapper, TOperation, TResult>(this TWrapper @this, Func<TOperation> continuation, Func<TOperation, TResult> result)
			where TWrapper : IYieldWrapper where TOperation : AsyncOperation
		{
			return @this.CallbackWith(continuation).ContinueWith(x => x).CallbackWith(result);
		}

		private static ResultYieldInstruction<TResult> ContinueWith<TWrapper, TResult>(this TWrapper @this, Func<ResultYieldInstruction<TResult>> continuation) where TWrapper : IYieldWrapper
		{
			return new ResultContinuation<TWrapper, TResult>(@this, continuation);
		}

		#endregion

		#region AsyncOperation

		/// <summary>
		///		Creates a super yield instruction which awaits this void operation, then invokes a callback
		/// </summary>
		/// <param name="this"></param>
		/// <param name="callback">The callback to invoke after awaiting the operation</param>
		public static CustomYieldInstruction CallbackWith(this AsyncOperation @this, Action callback)
		{
			return new AsyncOperationWrapper(@this).CallbackWith(callback);
		}

		/// <summary>
		///		Creates a super yield instruction which awaits this async operation, then returns the result of a callback
		/// </summary>
		/// <param name="this"></param>
		/// <param name="callback">The result of the super instruction</param>
		/// <typeparam name="TResult">The type of the super instruction's result</typeparam>
		public static ResultYieldInstruction<TResult> CallbackWith<TResult>(this AsyncOperation @this, Func<TResult> callback)
		{
			return new AsyncOperationWrapper(@this).CallbackWith(callback);
		}

		/// <summary>
		/// 	Creates a super yield instruction which awaits this void operation, then awaits a void operation determined by a callback
		///
		/// 	<para>
		/// 		If you wish for a return value, use <seealso cref="ContinueWith{TOperation,TResult}(AsyncOperation,System.Func{TOperation},System.Func{TOperation,TResult})"/>.
		/// 	</para>
		/// </summary>
		/// <param name="this"></param>
		/// <param name="continuation">The void operation that the super instruction should await</param>
		public static CustomYieldInstruction ContinueWith(this AsyncOperation @this, Func<AsyncOperation> continuation)
		{
			return new AsyncOperationWrapper(@this).ContinueWith(() => new AsyncOperationWrapper(continuation()));
		}

		/// <summary>
		/// 	Creates a super yield instruction which awaits this void operation, then awaits a generic operation determined by a callback, and finally returns the result of
		///			a callback.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="continuation">The operation that the super instruction should await</param>
		/// <param name="result">The result of the super instruction</param>
		/// <typeparam name="TOperation">The type of the <see cref="AsyncOperation"/> to await</typeparam>
		/// <typeparam name="TResult">The type of the super instruction's result</typeparam>
		public static ResultYieldInstruction<TResult> ContinueWith<TOperation, TResult>(this AsyncOperation @this, Func<TOperation> continuation, Func<TOperation, TResult> result)
			where TOperation : AsyncOperation
		{
			return new AsyncOperationWrapper(@this).ContinueWith(continuation, result);
		}

		/// <summary>
		/// 	Creates a super yield instruction which awaits this async operation, then executes an instruction determined by a callback
		/// </summary>
		/// <param name="this"></param>
		/// <param name="continuation">The instruction that the super instruction should execute</param>
		public static CustomYieldInstruction ContinueWith(this AsyncOperation @this, Func<CustomYieldInstruction> continuation)
		{
			return new AsyncOperationWrapper(@this).ContinueWith(() => new CustomYieldWrapper(continuation()));
		}

		/// <summary>
		/// 	Creates a super yield instruction which awaits this async operation, then executes an instruction determined by a callback, and finally returns the result of the
		/// 	callback's instruction
		/// </summary>
		/// <param name="this"></param>
		/// <param name="continuation">The instruction that the super instruction should execute</param>
		/// <typeparam name="TResult">The type of the super instruction's result</typeparam>
		public static ResultYieldInstruction<TResult> ContinueWith<TResult>(this AsyncOperation @this, Func<ResultYieldInstruction<TResult>> continuation)
		{
			return new AsyncOperationWrapper(@this).ContinueWith(continuation);
		}

		#endregion

		#region CustomYieldInstruction

		/// <summary>
		///		Creates a super yield instruction which executes this instruction, then invokes a callback
		/// </summary>
		/// <param name="this"></param>
		/// <param name="callback">The callback to invoke after executing the instruction</param>
		public static CustomYieldInstruction CallbackWith(this CustomYieldInstruction @this, Action callback)
		{
			return new CustomYieldWrapper(@this).CallbackWith(callback);
		}

		/// <summary>
		///		Creates a super yield instruction which executes this instruction, then returns the result of a callback
		/// </summary>
		/// <param name="this"></param>
		/// <param name="callback">The result of the super instruction</param>
		/// <typeparam name="TResult">The type of the super instruction's result</typeparam>
		public static ResultYieldInstruction<TResult> CallbackWith<TResult>(this CustomYieldInstruction @this, Func<TResult> callback)
		{
			return new CustomYieldWrapper(@this).CallbackWith(callback);
		}

		/// <summary>
		///		Creates a super yield instruction which executes this instruction, then awaits a void operation determined by a callback
		///
		///		<para>
		/// 		If you wish for a return value, use <seealso cref="ContinueWith{TOperation,TResult}(AsyncOperation,System.Func{TOperation},System.Func{TOperation,TResult})"/>.
		/// 	</para>
		/// </summary>
		/// <param name="this"></param>
		/// <param name="continuation">The void operation that the super instruction should await</param>
		public static CustomYieldInstruction ContinueWith(this CustomYieldInstruction @this, Func<AsyncOperation> continuation)
		{
			return new CustomYieldWrapper(@this).ContinueWith(() => new AsyncOperationWrapper(continuation()));
		}

		/// <summary>
		/// 	Creates a super yield instruction which executes this instruction, then awaits a generic operation determined by a callback, and finally returns the result of
		///			a callback.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="continuation">The operation that the super instruction should await</param>
		/// <param name="result">The result of the super instruction</param>
		/// <typeparam name="TOperation">The type of the <see cref="AsyncOperation"/> to await</typeparam>
		/// <typeparam name="TResult">The type of the super instruction's result</typeparam>
		public static ResultYieldInstruction<TResult> ContinueWith<TOperation, TResult>(this CustomYieldInstruction @this, Func<TOperation> continuation, Func<TOperation, TResult> result)
			where TOperation : AsyncOperation
		{
			return new CustomYieldWrapper(@this).ContinueWith(continuation, result);
		}

		/// <summary>
		/// 	Creates a super yield instruction which executes this instruction, then executes an instruction determined by a callback
		/// </summary>
		/// <param name="this"></param>
		/// <param name="continuation">The instruction that the super instruction should execute</param>
		public static CustomYieldInstruction ContinueWith(this CustomYieldInstruction @this, Func<CustomYieldInstruction> continuation)
		{
			return new CustomYieldWrapper(@this).ContinueWith(() => new CustomYieldWrapper(continuation()));
		}

		/// <summary>
		/// 	Creates a super yield instruction which executes this instruction, then executes an instruction determined by a callback, and finally returns the result of the
		/// 	callback's instruction
		/// </summary>
		/// <param name="this"></param>
		/// <param name="continuation">The instruction that the super instruction should execute</param>
		/// <typeparam name="TResult">The type of the super instruction's result</typeparam>
		public static ResultYieldInstruction<TResult> ContinueWith<TResult>(this CustomYieldInstruction @this, Func<ResultYieldInstruction<TResult>> continuation)
		{
			return new CustomYieldWrapper(@this).ContinueWith(continuation);
		}

		#endregion

		#region Implementations

		private class VoidCallback<TWrapper> : CustomYieldInstruction where TWrapper : IYieldWrapper
		{
			private readonly TWrapper _wrapper;

			private Action? _callback;

			public override bool keepWaiting
			{
				get
				{
					if (!_wrapper.KeepWaiting)
					{
						if (_callback is not null)
						{
							_callback();
							_callback = null;
						}

						return false;
					}

					return true;
				}
			}

			public VoidCallback(TWrapper wrapper, Action callback)
			{
				_wrapper = wrapper;
				_callback = callback;
			}
		}

		private class ResultCallback<TWrapper, TResult> : ResultYieldInstruction<TResult> where TWrapper : IYieldWrapper
		{
			private readonly TWrapper _wrapper;
			private readonly Func<TResult> _callback;

			private bool _evaluated;
			private TResult? _result;

			public override bool keepWaiting => _wrapper.KeepWaiting;

			public override TResult Result
			{
				get
				{
					if (!_evaluated)
					{
						_result = _callback();
						_evaluated = true;
					}

					return _result!;
				}
			}

			public ResultCallback(TWrapper wrapper, Func<TResult> callback)
			{
				_wrapper = wrapper;
				_callback = callback;
			}
		}

		private class VoidContinuation<TWrapper, TContWrapper> : CustomYieldInstruction where TWrapper : IYieldWrapper where TContWrapper : struct, IYieldWrapper
		{
			private readonly TWrapper _wrapper;
			private readonly Func<TContWrapper> _contFactory;

			private TContWrapper? _cont;

			public override bool keepWaiting
			{
				get
				{
					if (!_cont.HasValue)
					{
						if (_wrapper.KeepWaiting)
						{
							return true;
						}

						_cont = _contFactory();
					}

					return _cont.Value.KeepWaiting;
				}
			}

			public VoidContinuation(TWrapper wrapper, Func<TContWrapper> contFactory)
			{
				_wrapper = wrapper;
				_contFactory = contFactory;
			}
		}

		private class ResultContinuation<TWrapper, TResult> : ResultYieldInstruction<TResult> where TWrapper : IYieldWrapper
		{
			private readonly TWrapper _wrapper;
			private readonly Func<ResultYieldInstruction<TResult>> _contFactory;

			private ResultYieldInstruction<TResult>? _cont;

			public override bool keepWaiting
			{
				get
				{
					if (_cont is null)
					{
						if (_wrapper.KeepWaiting)
						{
							return true;
						}

						_cont = _contFactory();
					}

					return _cont.keepWaiting;
				}
			}

			public override TResult Result
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

			public ResultContinuation(TWrapper wrapper, Func<ResultYieldInstruction<TResult>> contFactory)
			{
				_wrapper = wrapper;
				_contFactory = contFactory;
			}
		}

		#endregion
	}
}
