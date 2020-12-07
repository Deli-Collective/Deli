using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Mono.Cecil;
using MonoMod;

namespace Deli.MonoMod
{
	public sealed class DeliMonoModder : MonoModder
	{
		private static readonly string[] _resolutionDirs =
		{
			Paths.BepInExAssemblyDirectory,
			Paths.ManagedPath,
			Paths.PatcherPluginPath,
			Paths.PluginPath
		};

		public static DefaultAssemblyResolver Resolver { get; }

		static DeliMonoModder()
		{
			Resolver = new DefaultAssemblyResolver();
			foreach (var dir in _resolutionDirs)
			{
				Resolver.AddSearchDirectory(dir);
			}

			Resolver.ResolveFailure += (_, name) => TypeLoader.Resolver.Resolve(name);
		}

		private readonly ManualLogSource _log;

		public DeliMonoModder(ManualLogSource log)
		{
			_log = log;

			AssemblyResolver = Resolver;
		}

		public override void Log(string value)
		{
			_log.LogDebug(value);
		}

		public override void Dispose()
		{
			// Prevent module from being disposed before its written.
			Module = null;

			base.Dispose();
		}
	}
}
