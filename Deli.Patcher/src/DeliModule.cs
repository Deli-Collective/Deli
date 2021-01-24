using BepInEx.Configuration;
using BepInEx.Logging;
using Deli.VFS;

namespace Deli.Patcher
{
	public abstract class DeliModule
	{
		protected Mod Source { get; } = null!;

		protected IDirectoryHandle Resources => Source.Resources;

		protected ConfigFile Config => Source.Config;

		protected ManualLogSource Logger => Source.Logger;
	}
}
