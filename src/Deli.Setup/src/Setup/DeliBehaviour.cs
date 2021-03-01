using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using Deli.Runtime;
using Deli.VFS;
using UnityEngine;

namespace Deli.Setup
{
	/// <summary>
	///		A plugin from a mod that runs during <see cref="SetupStage"/> and any subsequent stages.
	///		Also inherits from <see cref="MonoBehaviour"/>
	/// </summary>
	public abstract class DeliBehaviour : MonoBehaviour, IDeliPlugin
	{
		internal static Mod? GlobalSource;

		/// <summary>
		///		The mod this behaviour originated from.
		/// </summary>
		protected Mod Source { get; }

		/// <inheritdoc cref="Mod.Info"/>
		protected Mod.Manifest Info => Source.Info;

		/// <inheritdoc cref="Mod.Resources"/>
		protected IDirectoryHandle Resources => Source.Resources;

		/// <inheritdoc cref="Mod.Config"/>
		protected ConfigFile Config => Source.Config;

		/// <inheritdoc cref="Mod.Logger"/>
		protected ManualLogSource Logger => Source.Logger;

		/// <summary>
		///		Invoked when stages are in progress.
		/// </summary>
		protected StageEvents Stages { get; } = new();

		/// <summary>
		///		Creates an instance of <see cref="DeliBehaviour"/>
		/// </summary>
		protected DeliBehaviour()
		{
			Source = GlobalSource ?? throw new InvalidOperationException("A source was not ready for this behaviour. Was the behavior initialized outside of Deli?");
		}

		/// <inheritdoc cref="IDeliPlugin.Run"/>
		public virtual void Run(Stage stage) => Stages.Run(stage);

		/// <summary>
		///		Represents the specific possible stages a <see cref="DeliBehaviour"/> can process
		/// </summary>
		protected class StageEvents : IDeliPlugin
		{
			private readonly OneTimeEvent<StageRunner<SetupStage>> _setup = new();
			private readonly OneTimeEvent<StageRunner<RuntimeStage>> _runtime = new();

			/// <summary>
			///		Invoked when the <see cref="SetupStage"/> is in progress.
			/// </summary>
			public event StageRunner<SetupStage>? Setup
			{
				add => _setup.Add(value);
				remove => _setup.Remove(value);
			}

			/// <summary>
			///		Invoked when the <see cref="RuntimeStage"/> is in progress.
			/// </summary>
			public event StageRunner<RuntimeStage>? Runtime
			{
				add => _runtime.Add(value);
				remove => _runtime.Remove(value);
			}

			/// <inheritdoc cref="IDeliPlugin.Run"/>
			public void Run(Stage stage)
			{
				switch (stage)
				{
					case SetupStage setup:
						_setup.Consume()?.Invoke(setup);
						break;
					case RuntimeStage runtime:
						_runtime.Consume()?.Invoke(runtime);
						break;
				}
			}
		}
	}
}
