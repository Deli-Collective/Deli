using UnityEngine;

namespace Deli
{
	public abstract class ResultYieldInstruction<T> : CustomYieldInstruction
	{
		public abstract T Result { get; }
	}
}
