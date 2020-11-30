using System.Collections;
using ADepIn;
using UnityEngine;

namespace Deli
{
	/// <summary>
	/// 	Helper methods of Deli modding framework during runtime. This is only accessible at runtime, not at patch-time.
	/// </summary>
	public static class DeliRuntime
	{
		private static Option<DeliPlugin> _instance;

		internal static DeliPlugin Instance
		{
			private get => _instance.Expect("The Deli runtime has not started yet. If you are a BepInEx plugin (not a DeliBehaviour), please ensure you depend on \"" + DeliConstants.Guid + "\" to give it time to initialize.");
			set => _instance = Option.Some(value);
		}

		/// <summary>
		/// 	Starts a coroutine via the Deli plugin component
		/// </summary>
		/// <param name="enumerator">The coroutine body to run</param>
		public static Coroutine StartCoroutine(IEnumerator enumerator)
		{
			return Instance.StartCoroutine(enumerator);
		}

		/// <summary>
		/// 	Stops a coroutine started by <seealso cref="StartCoroutine"/>
		/// </summary>
		/// <param name="coroutine">The coroutine handle to stop</param>
		public static void StopCoroutine(Coroutine coroutine)
		{
			Instance.StopCoroutine(coroutine);
		}
	}
}
