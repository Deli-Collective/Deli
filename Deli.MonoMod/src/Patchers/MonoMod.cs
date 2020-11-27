using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using Mono.Cecil;

namespace Deli.MonoMod
{
	internal class MonoModPatcher : IPatcher
	{
		private readonly ManualLogSource _log;
		private readonly IAssemblyResolver _resolver;
		private readonly IEnumerable<byte[]> _mods;

		public MonoModPatcher(ManualLogSource log, IAssemblyResolver resolver, IEnumerable<byte[]> mods)
		{
			_log = log;
			_resolver = resolver;
			_mods = mods;
		}

		public void Patch(ref AssemblyDefinition assembly)
		{
			var mods = _mods.Select(x => new MemoryStream(x)).ToList();

			try
			{
				using (var modder = new LoggedMonoModder(_log))
				{
					modder.Module = assembly.MainModule;
					modder.AssemblyResolver = _resolver;

					foreach (var mod in mods)
					{
						modder.ReadMod(mod);
					}

					modder.MapDependencies();
					modder.AutoPatch();
				}
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
