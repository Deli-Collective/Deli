using BepInEx.Configuration;
using BepInEx.Logging;
using Deli.VFS;
using UnityEngine;

namespace Deli.Setup
{
	public abstract class DeliBehaviour : MonoBehaviour
	{
		protected Mod Source { get; }

		protected IDirectoryHandle Resources => Source.Resources;

		protected ConfigFile Config => Source.Config;

		protected ManualLogSource Logger => Source.Logger;
	}
}
