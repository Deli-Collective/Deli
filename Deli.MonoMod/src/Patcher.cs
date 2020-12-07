using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ADepIn;
using BepInEx.Logging;
using Mono.Cecil;

namespace Deli.MonoMod
{
	internal class Patcher : IPatcher
	{
		private readonly ManualLogSource _log;
		private readonly IEnumerable<byte[]> _mods;
		private readonly string _name;

		public Patcher(ManualLogSource log, IEnumerable<byte[]> mods, string name)
		{
			_log = log;
			_mods = mods;
			_name = name;
		}

		public void Patch(ref AssemblyDefinition assembly)
		{
			_log.LogInfo("MonoMod patching " + _name);

			using var modder = new DeliMonoModder(_log)
			{
				Module = assembly.MainModule
			};

			var mods = _mods.Select(x => new MemoryStream(x)).ToList();
			try
			{
				modder.MapDependencies();
				modder.PatchRefs();
				modder.AutoPatch();
			}
			finally
			{
				foreach (var mod in mods)
				{
					mod.Dispose();
				}
			}
		}
	}
}
