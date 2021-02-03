using UnityEngine;

namespace Deli.Setup
{
	internal interface IYieldWrapper
	{
		bool KeepWaiting { get; }
	}

	internal readonly struct AsyncOperationWrapper : IYieldWrapper
	{
		private readonly AsyncOperation _op;

		public bool KeepWaiting => !_op.isDone;

		public AsyncOperationWrapper(AsyncOperation op)
		{
			_op = op;
		}
	}

	internal readonly struct CustomYieldWrapper : IYieldWrapper
	{
		private readonly CustomYieldInstruction _inst;

		public bool KeepWaiting => _inst.keepWaiting;

		public CustomYieldWrapper(CustomYieldInstruction inst)
		{
			_inst = inst;
		}
	}
}
