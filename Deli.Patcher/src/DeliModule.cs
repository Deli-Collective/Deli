using System.Collections.Generic;
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
		///		Creates an instance of <see cref="DeliModule"/>.
		/// </summary>
		/// <param name="source">The mod this module originated from.</param>
		protected DeliModule(Mod source)
		{
			Source = source;
		}

		/// <summary>
		///		Invoked when the <see cref="PatcherStage"/> is in progress.
		/// </summary>
		protected abstract void RunStage(PatcherStage stage);

		/// <inheritdoc cref="IDeliCode.RunStage"/>
		public virtual void RunStage(Stage stage)
		{
			if (stage is PatcherStage patcher)
			{
				RunStage(patcher);
			}
		}
	}
}
