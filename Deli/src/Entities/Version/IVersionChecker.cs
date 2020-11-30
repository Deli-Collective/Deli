using System;
using System.Collections;
using ADepIn;

namespace Deli
{
	/// <summary>
	///		Represents a version checker for a specific mod and URL
	/// </summary>
	public interface IVersionChecker
	{
		/// <summary>
		/// 	The version of the remote mod, if available
		/// </summary>
		Option<Version> Result { get; }

		/// <summary>
		/// 	Ran before and should finish at or after <seealso cref="Result"/> is set
		///		<p>This is a coroutine. For more information, see <see href="https://docs.unity3d.com/Manual/Coroutines.html">the Unity Documentation</see>.</p>
		/// </summary>
		IEnumerator Await();
	}
}
