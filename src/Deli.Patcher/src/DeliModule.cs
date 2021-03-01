using BepInEx.Configuration;
using BepInEx.Logging;
using Deli.VFS;

namespace Deli.Patcher
{
	/// <summary>
	///		A plugin from a mod that runs during <see cref="PatcherStage"/> and any subsequent stages
	/// </summary>
	public abstract class DeliModule : IDeliPlugin
	{
		/// <summary>
		///		The mod this module originated from
		/// </summary>
		protected Mod Source { get; }

		/// <inheritdoc cref="Mod.Info"/>
		protected Mod.Manifest Info => Source.Info;

		/// <inheritdoc cref="Mod.Logger"/>
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
		///		Creates an instance of <see cref="DeliModule"/>
		/// </summary>
		/// <param name="source">The mod this module originated from</param>
		protected DeliModule(Mod source)
		{
			Source = source;
		}

		/// <inheritdoc cref="IDeliPlugin.Run"/>
		public virtual void Run(Stage stage) => Stages.Run(stage);

		/// <summary>
		///		Represents the specific possible stages a <see cref="DeliModule"/> can process
		/// </summary>
		protected class StageEvents : IDeliPlugin
		{
			private readonly OneTimeEvent<StageRunner<PatcherStage>> _patcher = new();

			/// <summary>
			///		Invoked when the <see cref="PatcherStage"/> is in progress.
			/// </summary>
			public event StageRunner<PatcherStage>? Patcher
			{
				add => _patcher.Add(value);
				remove => _patcher.Remove(value);
			}

			/// <summary>
			///		Invoked when future stages are in progress.
			/// </summary>
			public event StageRunner<Stage>? Other;

			/// <inheritdoc cref="IDeliPlugin.Run"/>
			public void Run(Stage stage)
			{
				switch (stage)
				{
					case PatcherStage patcher:
						_patcher.Consume()?.Invoke(patcher);
						break;
					default:
						Other?.Invoke(stage);
						break;
				}
			}
		}
	}
}
