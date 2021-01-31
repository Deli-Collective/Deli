using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using Deli.VFS;
using UnityEngine;

namespace Deli.Setup
{
	/// <summary>
	///		A piece of code from a mod that runs during <see cref="RuntimeStage"/> and any subsequent stages.
	///		Also inherits from <see cref="MonoBehaviour"/>
	/// </summary>
	public abstract class DeliBehaviour : MonoBehaviour, IDeliCode
	{
		internal static Mod? GlobalSource;

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

		/// <summary>
		///		Invoked when the <see cref="SetupStage"/> is in progress.
		/// </summary>
		protected event StageRunner<SetupStage>? Setup;

		/// <summary>
		///		Invoked when the <see cref="RuntimeStage"/> is in progress.
		/// </summary>
		protected event StageRunner<RuntimeStage>? Runtime;

		protected DeliBehaviour()
		{
			Source = GlobalSource ?? throw new InvalidOperationException("A source was not ready for this behaviour. Was the behavior initialized outside of Deli?");
		}

		/// <inheritdoc cref="IDeliCode.RunStage"/>
		public virtual void RunStage(Stage stage)
		{
			switch (stage)
			{
				case SetupStage setup:
					Setup?.Invoke(setup);
					break;
				case RuntimeStage runtime:
					Runtime?.Invoke(runtime);
					break;
			}
		}
	}
}
