using BepInEx.Logging;
using Mono.Cecil;
using MonoMod;

namespace Deli.MonoMod
{
	internal class DeliMonoModder : MonoModder
	{
		private readonly ManualLogSource _log;

		public DeliMonoModder(ManualLogSource log, IAssemblyResolver resolver, ModuleDefinition module)
		{
			_log = log;

			AssemblyResolver = resolver;
			Module = module;
		}

		public override void Log(string value)
		{
			_log.LogDebug(value);
		}

        public override void Dispose()
        {
			Module = null;
			AssemblyResolver = null;

            base.Dispose();
        }

	}
}
