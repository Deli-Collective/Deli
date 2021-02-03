using System;
using UnityEngine;

namespace Deli.Setup
{
	public static class ExtYieldInstruction
	{
		private static CustomYieldInstruction CallbackWith<TWrapper>(this TWrapper @this, Action callback) where TWrapper : IYieldWrapper
		{
			return new VoidCallback<TWrapper>(@this, callback);
		}

		private static ResultYieldInstruction<T> CallbackWith<TWrapper, T>(this TWrapper @this, Func<T> callback) where TWrapper : IYieldWrapper
		{
			return new ResultCallback<TWrapper, T>(@this, callback);
		}

		private static CustomYieldInstruction ContinueWith<TWrapper, TContWrapper>(this TWrapper @this, Func<TContWrapper> continuation)
			where TWrapper : IYieldWrapper where TContWrapper : struct, IYieldWrapper
		{
			return new VoidContinuation<TWrapper, TContWrapper>(@this, continuation);
		}

		private static ResultYieldInstruction<T> ContinueWith<TWrapper, T>(this TWrapper @this, Func<ResultYieldInstruction<T>> continuation) where TWrapper : IYieldWrapper
		{
			return new ResultContinuation<TWrapper, T>(@this, continuation);
		}

		public static CustomYieldInstruction CallbackWith(this AsyncOperation @this, Action callback)
		{
			return new AsyncOperationWrapper(@this).CallbackWith(callback);
		}

		public static ResultYieldInstruction<T> CallbackWith<T>(this AsyncOperation @this, Func<T> callback)
		{
			return new AsyncOperationWrapper(@this).CallbackWith(callback);
		}

		public static CustomYieldInstruction ContinueWith(this AsyncOperation @this, Func<AsyncOperation> continuation)
		{
			return new AsyncOperationWrapper(@this).ContinueWith(() => new AsyncOperationWrapper(continuation()));
		}

		public static CustomYieldInstruction ContinueWith(this AsyncOperation @this, Func<CustomYieldInstruction> continuation)
		{
			return new AsyncOperationWrapper(@this).ContinueWith(() => new CustomYieldWrapper(continuation()));
		}

		public static ResultYieldInstruction<T> ContinueWith<T>(this AsyncOperation @this, Func<ResultYieldInstruction<T>> continuation)
		{
			return new AsyncOperationWrapper(@this).ContinueWith(continuation);
		}

		public static CustomYieldInstruction CallbackWith(this CustomYieldInstruction @this, Action callback)
		{
			return new CustomYieldWrapper(@this).CallbackWith(callback);
		}

		public static ResultYieldInstruction<T> CallbackWith<T>(this CustomYieldInstruction @this, Func<T> callback)
		{
			return new CustomYieldWrapper(@this).CallbackWith(callback);
		}

		public static CustomYieldInstruction ContinueWith(this CustomYieldInstruction @this, Func<AsyncOperation> continuation)
		{
			return new CustomYieldWrapper(@this).ContinueWith(() => new AsyncOperationWrapper(continuation()));
		}

		public static CustomYieldInstruction ContinueWith(this CustomYieldInstruction @this, Func<CustomYieldInstruction> continuation)
		{
			return new CustomYieldWrapper(@this).ContinueWith(() => new CustomYieldWrapper(continuation()));
		}

		public static ResultYieldInstruction<T> ContinueWith<T>(this CustomYieldInstruction @this, Func<ResultYieldInstruction<T>> continuation)
		{
			return new CustomYieldWrapper(@this).ContinueWith(continuation);
		}

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
	}
}
