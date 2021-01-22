using UnityEngine;

namespace Deli.Setup
{
	public abstract class ResultYieldInstruction<T> : CustomYieldInstruction
	{
		public abstract T Result { get; }
	}
}
