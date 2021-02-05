using BepInEx.Configuration;
using BepInEx.Logging;
using Deli.VFS;

namespace Deli.Patcher
{
	/// <summary>
	///		A piece of code from a mod that runs during <see cref="PatcherStage"/> and any subsequent stages.
	/// </summary>
	public abstract class DeliModule : IDeliCode
	{
		/// <summary>
		///		The mod this module originated from.
		/// </summary>
		protected Mod Source { get; }

		/// <inheritdoc cref="Mod.Logger"/>
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
		///		Creates an instance of <see cref="DeliModule"/>.
		/// </summary>
		/// <param name="source">The mod this module originated from.</param>
		protected DeliModule(Mod source)
		{
			Source = source;
		}

		/// <inheritdoc cref="IDeliCode.Run"/>
		public virtual void Run(Stage stage) => Events.Run(stage);

		protected class StageEvents : IDeliCode
		{
			/// <summary>
			///		Invoked when the <see cref="PatcherStage"/> is in progress.
			/// </summary>
			public event StageRunner<PatcherStage>? Patcher;

			/// <summary>
			///		Invoked when future stages are in progress.
			/// </summary>
			public event StageRunner<Stage>? Other;

			public void Run(Stage stage)
			{
				switch (stage)
				{
					case PatcherStage patcher:
						Patcher?.Invoke(patcher);
						break;
					default:
						Other?.Invoke(stage);
						break;
				}
			}
		}
	}
}
