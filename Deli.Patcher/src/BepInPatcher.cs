using System;
using System.Collections.Generic;
using BepInEx;
using Mono.Cecil;

namespace Deli
{
    public static class BepInPatcher
    {
		private static Dictionary<string, IEnumerable<IPatcher>> _allPatchers;
		private static IEnumerable<IPatcher>? _activePatchers;

		public static IEnumerable<string> TargetDLLs
		{
			get
			{
				foreach (var localPatchers in _allPatchers)
				{
					_activePatchers = localPatchers.Value;
					yield return localPatchers.Key;
				}

				_activePatchers = null;
			}
		}

		static BepInPatcher()
		{
			_allPatchers = null; // TODO: source patchers
		}

		public static void Patch(ref AssemblyDefinition assembly)
		{
			foreach (var patcher in _activePatchers)
			{
				patcher.Patch(ref assembly);
			}
		}
	}
}
