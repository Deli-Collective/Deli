using ADepIn;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace Deli
{
	/// <summary>
	///		The base class for Deli code assets, i.e. plugins
	/// </summary>
	public abstract class DeliBehaviour : MonoBehaviour
	{
		/// <summary>
		///		The mod that contained this behaviour, refered to as the "source mod"
		/// </summary>
		protected Mod Source { get; }

		/// <summary>
		/// 	Information about the source mod
		/// </summary>
		protected Mod.Manifest Info => Source.Info;

		/// <summary>
		/// 	The assets for the source mod
		/// </summary>
		protected IResourceIO Resources => Source.Resources;

		/// <summary>
		/// 	The configuration for the source mod
		/// </summary>
		protected ConfigFile Config => Source.Config;

		/// <summary>
		/// 	The log available to the source mod
		/// </summary>
		protected ManualLogSource Logger => Source.Logger;

		protected DeliBehaviour()
		{
			var self = GetType();

			// Yeah, you could use an indexer, but a KeyNotFoundException isn't very helpful
			var sourceOpt = DeliRuntime.BehaviourSources.OptionGetValue(self);
			Source = sourceOpt.Expect($"There was no behaviour source for {GetType()}. Was this instantiated by something other than Deli.Runtime?");
			
			// Disallow multiple instances
			DeliRuntime.BehaviourSources.Remove(self);
		}
	}
}
