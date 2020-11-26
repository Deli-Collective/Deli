using System.Collections.Generic;
using Mono.Cecil;

namespace Deli
{
	public static class Entrypoint
	{
		private static readonly Dictionary<string, List<IPatcher>> _patchers;

		public static IEnumerable<string> TargetDLLs => _patchers.Keys;

		static Entrypoint()
		{
			_patchers = Deli.Patch();
		}

		public static void Patch(ref AssemblyDefinition assembly)
		{
			AssemblyPatcher
			_patchers[assembly.Name.Name]
		}
	}
}
