using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using Deli.Runtime;
using Deli.VFS;
using UnityEngine;

namespace Deli.Setup
{
	/// <summary>
	///		A plugin from a mod that runs during <see cref="RuntimeStage"/> and any subsequent stages.
	///		Also inherits from <see cref="MonoBehaviour"/>
	/// </summary>
	public abstract class DeliBehaviour : MonoBehaviour, IDeliPlugin
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
		///		Invoked when stages are in progress.
		/// </summary>
		protected StageEvents Events { get; } = new();

		/// <summary>
		///		Creates an instance of <see cref="DeliBehaviour"/>
		/// </summary>
		protected DeliBehaviour()
		{
			Source = GlobalSource ?? throw new InvalidOperationException("A source was not ready for this behaviour. Was the behavior initialized outside of Deli?");
		}

		/// <inheritdoc cref="IDeliPlugin.Run"/>
		public virtual void Run(Stage stage) => Events.Run(stage);

		/// <summary>
		///		Represents the specific possible stages a <see cref="DeliBehaviour"/> can process
		/// </summary>
		protected class StageEvents : IDeliPlugin
		{
			/// <summary>
			///		Invoked when the <see cref="SetupStage"/> is in progress.
			/// </summary>
			protected event StageRunner<SetupStage>? Setup;

			/// <summary>
			///		Invoked when the <see cref="RuntimeStage"/> is in progress.
			/// </summary>
			protected event StageRunner<RuntimeStage>? Runtime;

			/// <inheritdoc cref="IDeliPlugin.Run"/>
			public void Run(Stage stage)
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
}
