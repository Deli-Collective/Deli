using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Deli.VFS;
using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;

namespace Deli.Patcher
{
	internal sealed class DeliAssemblyResolver : DefaultAssemblyResolver
	{
		private static IEnumerable<string> DepthFirstSearch(IEnumerable<string> directories)
		{
			foreach (var directory in directories)
			{
				yield return directory;

				var subdirectories = Directory.GetDirectories(directory);
				foreach (var subdirectory in DepthFirstSearch(subdirectories))
				{
					yield return subdirectory;
				}
			}
		}

		protected override AssemblyDefinition SearchDirectory(AssemblyNameReference name, IEnumerable<string> directories, ReaderParameters parameters)
		{
			return base.SearchDirectory(name, DepthFirstSearch(directories), parameters);
		}
	}

	internal sealed class DeliMonoModder : MonoModder
	{
		private static readonly DefaultAssemblyResolver Resolver;

		static DeliMonoModder()
		{
			Resolver = new();

			foreach (var directory in SearchDirectories())
			{
				Resolver.AddSearchDirectory(directory);
			}

			Resolver.ResolveFailure += ResolverOnResolveFailure;
		}

		private static IEnumerable<string> SearchDirectories()
		{
			yield return Paths.BepInExAssemblyDirectory;
			yield return Paths.PatcherPluginPath;
			yield return Paths.PluginPath;
			yield return Paths.ManagedPath;
			yield return Path.Combine(Paths.BepInExRootPath, "monomod");
		}

		// Copied from BepInEx.MonoMod.Loader
		private static AssemblyDefinition? ResolverOnResolveFailure(object sender, AssemblyNameReference reference)
		{
			foreach (var directory in SearchDirectories())
			{
				var potentialDirectories = new List<string> { directory };

				potentialDirectories.AddRange(Directory.GetDirectories(directory, "*", SearchOption.AllDirectories));

				var potentialFiles = potentialDirectories.Select(x => Path.Combine(x, $"{reference.Name}.dll"))
					.Concat(potentialDirectories.Select(
						x => Path.Combine(x, $"{reference.Name}.exe")));

				foreach (string path in potentialFiles)
				{
					if (!File.Exists(path))
						continue;

					var assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters(ReadingMode.Deferred));

					if (assembly.Name.Name == reference.Name)
						return assembly;

					assembly.Dispose();
				}
			}

			return null;
		}

		private readonly ManualLogSource _logger;
		private readonly bool _hookGenDebug;

		public DeliMonoModder(ManualLogSource logger, ModuleDefinition module, bool hookGenDebug)
		{
			_logger = logger;
			_hookGenDebug = hookGenDebug;
			Module = module;
			AssemblyResolver = Resolver;
		}

		public override void Log(string text)
		{
			_logger.LogInfo(text);
		}

		public override void LogVerbose(string text)
		{
			if (_hookGenDebug)
			{
				_logger.LogDebug(text);
			}
		}

		public override void Dispose()
		{
			Module = null;
			AssemblyResolver = null;

			base.Dispose();
		}
	}

	internal class AssetLoaders
	{
		private readonly Mod _mod;
		private readonly ConfigEntry<bool> _hookGenDebug;
		private readonly Dictionary<string, Patcher> _patchers = new();

		public IEnumerable<KeyValuePair<string, MemoryStream>> Hooks => _patchers
			.Select(x => new KeyValuePair<string, MemoryStream?>(x.Key, x.Value.HookDestination))
			.Where(x => x.Value is not null)!;

		public AssetLoaders(Mod mod, ConfigEntry<bool> hookGenDebug)
		{
			_mod = mod;
			_hookGenDebug = hookGenDebug;
		}

		private Patcher this[PatcherStage stage, string assembly]
		{
			get
			{
				if (!_patchers.TryGetValue(assembly, out var patcher))
				{
					patcher = new Patcher(_mod.Logger, _hookGenDebug);
					_patchers.Add(assembly, patcher);

					stage.Patchers[assembly, _mod] = patcher.Patch;
				}

				return patcher;
			}
		}

		public void MonoModAssetLoader(PatcherStage stage, Mod mod, IHandle handle)
		{
			if (handle is not IFileHandle file)
			{
				throw new ArgumentException("The MonoMod loader must be provided a file.", nameof(handle));
			}

			const string mmDll = ".mm.dll";
			var name = file.Name;
			if (!name.EndsWith(mmDll))
			{
				throw new ArgumentException("The file did not match the MonoMod format. It must start with the name of the " +
				                            "assembly to patch, and must end with '" + mmDll + "'.", nameof(handle));
			}

			var target = name.Substring(0, name.Length - mmDll.Length) + ".dll";
			this[stage, target].Mods.Add(file);
		}

		public void MonoModHookGenAssetLoader(PatcherStage stage, Mod mod, IHandle handle)
		{
			if (handle is not IFileHandle file)
			{
				throw new ArgumentException("The MonoMod.HookGen loader must be provided a file.", nameof(handle));
			}

			var reader = stage.ImmediateReaders.Get<IEnumerable<string>>();
			foreach (var target in reader(file))
			{
				this[stage, target].HookDestination ??= new MemoryStream();
			}
		}

		private class Patcher
		{
			private readonly ManualLogSource _logger;
			private readonly ConfigEntry<bool> _hookGenDebug;

			public List<IFileHandle> Mods { get; } = new();
			public MemoryStream? HookDestination { get; set; }

			public Patcher(ManualLogSource logger, ConfigEntry<bool> hookGenDebug)
			{
				_logger = logger;
				_hookGenDebug = hookGenDebug;
			}

			public void Patch(ref AssemblyDefinition assembly)
			{
				var modBuffer = new Stream?[Mods.Count];

				try
				{
					var module = assembly.MainModule;
					using var modder = new DeliMonoModder(_logger, module, _hookGenDebug.Value);

					for (var i = 0; i < modBuffer.Length; ++i)
					{
						var file = Mods[i];
						using var mod = file.OpenRead();

						var buffer = new MemoryStream();
						mod.CopyTo(buffer);

						modBuffer[i] = buffer;
						modder.ReadMod(buffer);
					}

					modder.MapDependencies();
					if (modBuffer.Length > 0)
					{
						modder.PatchRefs();
						modder.AutoPatch();
					}

					var hookDestination = HookDestination;
					if (hookDestination is not null)
					{
						var generator = new HookGenerator(modder, "MMHOOK_" + module.Name)
						{
							HookPrivate = true
						};

						generator.Generate();
						generator.OutputModule.Write(hookDestination);
					}
				}
				finally
				{
					foreach (var mod in modBuffer)
					{
						mod?.Dispose();
					}
				}
			}
		}
	}
}
