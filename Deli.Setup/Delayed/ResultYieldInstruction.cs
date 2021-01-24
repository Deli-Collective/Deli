using System;
using UnityEngine;

namespace Deli.Setup
{
	/// <summary>
	///		A yield instruction that contains a result on completion.
	/// </summary>
	/// <typeparam name="T">The type of the result.</typeparam>
	public abstract class ResultYieldInstruction<T> : CustomYieldInstruction
	{
		/// <summary>
		///		The result of operation.
		/// </summary>
		/// <exception cref="InvalidOperationException">The operation has not been completed.</exception>
		public abstract T Result { get; }
	}
}
