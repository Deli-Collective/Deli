using System;
using UnityEngine;

namespace Deli.Setup
{
	public static class ExtCustomYieldInstruction
	{
		public static CustomYieldInstruction CallbackWith(this CustomYieldInstruction @this, Action callback)
		{
			return new VoidCallback(@this, callback);
		}

		public static ResultYieldInstruction<T> CallbackWith<T>(this CustomYieldInstruction @this, Func<T> callback)
		{
			return new ResultCallback<T>(@this, callback);
		}

		public static CustomYieldInstruction ContinueWith(this CustomYieldInstruction @this, Func<CustomYieldInstruction> continuation)
		{
			return new VoidContinuation(@this, continuation);
		}

		public static ResultYieldInstruction<T> ContinueWith<T>(this CustomYieldInstruction @this, Func<ResultYieldInstruction<T>> continuation)
		{
			return new ResultContinuation<T>(@this, continuation);
		}

		private class VoidCallback : CustomYieldInstruction
		{
			private readonly CustomYieldInstruction _inst;

			private Action? _callback;

			public override bool keepWaiting
			{
				get
				{
					if (!_inst.keepWaiting)
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

			public VoidCallback(CustomYieldInstruction inst, Action callback)
			{
				_inst = inst;
				_callback = callback;
			}
		}

		private class ResultCallback<TResult> : ResultYieldInstruction<TResult>
		{
			private readonly CustomYieldInstruction _inst;
			private readonly Func<TResult> _callback;

			public override bool keepWaiting => _inst.keepWaiting;

			private TResult? _result;
			public override TResult Result => _result ??= _callback();

			public ResultCallback(CustomYieldInstruction inst, Func<TResult> callback)
			{
				_inst = inst;
				_callback = callback;
			}
		}

		private class VoidContinuation : CustomYieldInstruction
		{
			private CustomYieldInstruction _current;
			private Func<CustomYieldInstruction>? _contFactory;

			public override bool keepWaiting
			{
				get
				{
					if (!_current.keepWaiting && _contFactory is not null)
					{
						_current = _contFactory();
						_contFactory = null;
					}

					return _current.keepWaiting;
				}
			}

			public VoidContinuation(CustomYieldInstruction inst, Func<CustomYieldInstruction> contFactory)
			{
				_current = inst;
				_contFactory = contFactory;
			}
		}

		private class ResultContinuation<TResult> : ResultYieldInstruction<TResult>
		{
			private readonly CustomYieldInstruction _inst;
			private readonly Func<ResultYieldInstruction<TResult>> _contFactory;

			private ResultYieldInstruction<TResult>? _cont;

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

			public ResultContinuation(CustomYieldInstruction inst, Func<ResultYieldInstruction<TResult>> contFactory)
			{
				_inst = inst;
				_contFactory = contFactory;
			}
		}
	}
}
