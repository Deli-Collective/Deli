using UnityEngine;

namespace Deli.Runtime.Yielding
{
	/// <summary>
	///		A <see cref="CustomYieldInstruction"/> which completes immediately
	/// </summary>
	public sealed class DummyYieldInstruction : CustomYieldInstruction
	{
		/// <inheritdoc cref="CustomYieldInstruction.keepWaiting"/>
		public override bool keepWaiting { get; } = false;
	}

	/// <summary>
	///		A <see cref="ResultYieldInstruction{TResult}"/> which completes immediately
	/// </summary>
	/// <typeparam name="T">The type of the result</typeparam>
	public sealed class DummyYieldInstruction<T> : ResultYieldInstruction<T>
	{
		/// <inheritdoc cref="CustomYieldInstruction.keepWaiting"/>
		public override bool keepWaiting { get; } = false;

		/// <inheritdoc cref="ResultYieldInstruction{TResult}.Result"/>
		public override T Result { get; }

		/// <summary>
		///		Creates an instance of <see cref="DummyYieldInstruction{T}"/>
		/// </summary>
		/// <param name="result">The result that this instruction should house</param>
		public DummyYieldInstruction(T result)
		{
			Result = result;
		}
	}
}
