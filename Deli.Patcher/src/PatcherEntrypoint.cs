using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Mono.Cecil;

namespace Deli.Patcher
{
    public static class PatcherEntrypoint
    {
		private static Dictionary<string, List<IPatcher>> _allPatchers;
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

		static PatcherEntrypoint()
		{
			var stage = new DeliPatcherStage();
			// TODO: do something

			_allPatchers = stage.FilePatchers; // TODO: source patchers
		}

		public static void Patch(ref AssemblyDefinition assembly)
		{
			if (_activePatchers is null)
			{
				throw new InvalidOperationException("Implicit contract broken: target DLLs was not enumerated before the patch method.");
			}

			foreach (var patcher in _activePatchers)
			{
				patcher.Patch(ref assembly);
			}
		}

		public static void Handoff(Action<ManualLogSource, ImmediateReaderCollection> callback)
		{
			DeliPatcherStage.Handoff(callback);
		}
	}
}
