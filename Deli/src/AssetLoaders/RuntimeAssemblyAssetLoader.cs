using System;
using ADepIn;
using BepInEx.Logging;
using UnityEngine;

namespace Deli
{
	public class RuntimeAssemblyAssetLoader : AssemblyAssetLoader
	{
		private readonly GameObject _manager;

		public RuntimeAssemblyAssetLoader(GameObject manager, ManualLogSource log) : base(log)
		{
			_manager = manager;
		}

		protected override void TypeCallback(IServiceKernel kernel, Mod mod, string path, Type type)
		{
			if (type.IsAbstract || !type.IsAssignableFrom(typeof(DeliBehaviour))) return;

			_manager.AddComponent(type);
		}
	}
}
