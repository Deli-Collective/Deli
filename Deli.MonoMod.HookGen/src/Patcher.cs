using System.IO;
using BepInEx.Logging;
using Mono.Cecil;
using MonoMod.RuntimeDetour.HookGen;

namespace Deli.MonoMod.HookGen
{
	internal class Patcher : IPatcher
	{
		private readonly ManualLogSource _log;
		private readonly Stream _output;
		private readonly string _name;

		public Patcher(ManualLogSource log, Stream output, string name)
		{
			_log = log;
			_output = output;
			_name = name;
		}

		public void Patch(ref AssemblyDefinition assembly)
		{
			_log.LogInfo("Generating hooks for " + _name);

			using var modder = new DeliMonoModder(_log)
			{
				Module = assembly.MainModule
			};
			modder.MapDependencies();

			var generator = new HookGenerator(modder, "MMHOOK_" + assembly.MainModule.Name)
			{
				HookPrivate = true
			};
			generator.Generate();

			generator.OutputModule.Write(_output);
		}
	}
}
