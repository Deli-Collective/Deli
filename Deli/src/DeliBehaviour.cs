using System;
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

		// Obsoleted for naming and accessor purposes.
		[Obsolete("Please change all references to " + nameof(Source) + ". This is merely a proxy to it.")]
		public Mod BaseMod => Source;

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
		protected ManualLogSource Log => Source.Log;

		protected DeliBehaviour()
		{
			Source = Deli.Services.Get<Mod, DeliBehaviour>(this).Expect("Could not acquire mod for behaviour: " + GetType());
		}
	}

	// Obsoleted for naming purposes.
	[Obsolete("Please change all references to" + nameof(DeliBehaviour) + ". This is merely a proxy to it.")]
	public abstract class DeliMod : DeliBehaviour
	{
	}
}
