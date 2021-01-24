using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using Deli.VFS;
using UnityEngine;

namespace Deli.Setup
{
	/// <summary>
	///		A piece of code from a mod that runs during <see cref="SetupStage"/> and any subsequent stages.
	///		Also inherits from <see cref="MonoBehaviour"/>
	/// </summary>
	public abstract class DeliBehaviour : MonoBehaviour, IDeliCode
	{
		// TODO: set this before running AddComponent on the manager.
		internal static Mod? GlobalSource { private get; set; }

		/// <summary>
		///		The mod this behaviour originated from.
		/// </summary>
		protected Mod Source { get; }

		/// <inheritdoc cref="Mod.Resources"/>
		protected IDirectoryHandle Resources => Source.Resources;

		/// <inheritdoc cref="Mod.Config"/>
		protected ConfigFile Config => Source.Config;

		/// <inheritdoc cref="Mod.Logger"/>
		protected ManualLogSource Logger => Source.Logger;

		protected DeliBehaviour()
		{
			Source = GlobalSource ?? throw new InvalidOperationException("A source was not ready for this behaviour. Was the behavior initialized outside of Deli?");
			// Reset to null so the last mod doesn't leak into future initializations.
			GlobalSource = null;
		}

		protected abstract void RunStage(SetupStage stage);

		public virtual void RunStage(Stage stage)
		{
			if (stage is SetupStage setup)
			{
				RunStage(setup);
			}
		}
	}
}
