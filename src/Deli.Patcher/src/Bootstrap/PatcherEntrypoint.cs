#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using Mono.Cecil;
using static Deli.Bootstrap.Constants;

namespace Deli.Bootstrap
{
	public static class PatcherEntrypoint
	{
		// Store and access everything from this object, so we can scope the access of this static class.
		// Avoids memory leaking and enforces the implicit contract.
		private static Bootstrapper? _stateful;
		private static KeyValuePair<string, IEnumerable<KeyValuePair<Mod, Patcher.Patcher>>>? _activeFile;

		public static IEnumerable<string> TargetDLLs
		{
			get
			{
				if (_stateful is null)
				{
					throw new ImplicitContractException(nameof(Initialize));
				}

				foreach (var file in _stateful.Patchers)
				{
					_activeFile = file;
					yield return file.Key;
				}
			}
		}

		public static void Initialize()
		{
			_stateful = new Bootstrapper();
			_stateful.Logger.LogInfo($"Deli bootstrap has begun! Version {Metadata.Version} ({Git.Branch} @ {Git.Describe})");
		}

		public static void Patch(ref AssemblyDefinition assembly)
		{
			if (_stateful is null)
			{
				throw new ImplicitContractException(nameof(Initialize));
			}

			if (!_activeFile.HasValue)
			{
				throw new ImplicitContractException(nameof(TargetDLLs));
			}

			try
			{
				var activeFile = _activeFile.Value;
				var fileName = activeFile.Key;

				foreach (var pair in activeFile.Value)
				{
					var mod = pair.Key;
					var patcher = pair.Value;

					try
					{
						patcher(ref assembly);
					}
					catch (Exception)
					{
						_stateful.Logger.LogFatal($"{mod} threw an exception when patching {fileName}");
						throw;
					}
				}
			}
			finally
			{
				_activeFile = null;
			}
		}

		public static void Finish()
		{
			_activeFile = null;
		}

		public static HandoffBlob Handoff()
		{
			if (_stateful is null)
			{
				throw new InvalidOperationException("The bootstrap stack has not been initialized, or the handoff has already been performed.");
			}

			var blob = _stateful.Blob;
			_stateful = null;

			return blob;
		}

		private class ImplicitContractException : InvalidOperationException
		{
			public ImplicitContractException(string requiredMember) : base($"Implicit contract was broken: {requiredMember} must be called before this member is accessed.")
			{
			}
		}
	}
}
#pragma warning restore CS1591
