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
			Source = Deli.Services.Get<Mod, DeliBehaviour>(this).Expect("Could not acquire mod for behaviour: " + GetType());
		}
	}
}
