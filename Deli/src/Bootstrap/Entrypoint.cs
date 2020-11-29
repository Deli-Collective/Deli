using System;
using System.Collections.Generic;
using ADepIn;
using Mono.Cecil;

namespace Deli
{
	public static class Entrypoint
	{
		private static Option<IEnumerable<IPatcher>> _patchersNow;

		public static IEnumerable<string> TargetDLLs
		{
			get
			{
				foreach (var dll in Deli.Patchers)
				{
					_patchersNow.Replace(dll.Value);
					yield return dll.Key;
				}
			}
		}

		static Entrypoint()
		{
			Deli.PatchStage();

			_patchersNow = Option.None<IEnumerable<IPatcher>>();
		}

		// Rerouted so people don't see this in the Deli class.
		public static void Postpatch(IDeliRuntime module)
		{
			Deli.RuntimeStage(module);
		}

		public static void Patch(ref AssemblyDefinition assembly)
		{
			var patchers = _patchersNow.Take().Expect("A call was made outside the contract of a BepInEx patcher.");
			foreach (var patcher in patchers)
			{
				patcher.Patch(ref assembly);
			}
		}
	}
}
