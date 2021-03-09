using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using Deli.VFS;
using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;

namespace Deli.Patcher
{
	internal sealed class DeliMonoModder : MonoModder
	{
		private static readonly string[] SearchDirectories =
		{
			Paths.BepInExAssemblyDirectory,
			Paths.PatcherPluginPath,
			Paths.PluginPath,
			Paths.ManagedPath
		};

		private static readonly DefaultAssemblyResolver Resolver;

		static DeliMonoModder()
		{
			Resolver = new();

			foreach (var d in SearchDirectories) Resolver.AddSearchDirectory(d);
		}

		private readonly ManualLogSource _logger;

		public DeliMonoModder(ManualLogSource logger, ModuleDefinition module)
		{
			_logger = logger;
			Module = module;
			AssemblyResolver = Resolver;
		}

		public override void Log(string text)
		{
			_logger.LogInfo(text);
		}

		public override void LogVerbose(string text)
		{
			_logger.LogDebug(text);
		}

		public override void Dispose()
		{
			Module = null;
			base.Dispose();
		}
	}

	internal class MonoModAssetLoader
	{
		private readonly Mod _mod;
		private readonly Dictionary<string, List<IFileHandle>> _targetMods = new();

		private ManualLogSource Logger => _mod.Logger;

		public MonoModAssetLoader(Mod mod)
		{
			_mod = mod;
		}

		private Patcher Patch(List<IFileHandle> files)
		{
			void Closure(ref AssemblyDefinition assembly)
			{
				var modBuffer = new Stream?[files.Count];

				try
				{
					using var modder = new DeliMonoModder(Logger, assembly.MainModule);

					for (var i = 0; i < modBuffer.Length; ++i)
					{
						var file = files[i];
						var mod = file.OpenRead();

						modBuffer[i] = mod;
						modder.ReadMod(mod);
					}

					modder.MapDependencies();
					modder.PatchRefs();
					modder.AutoPatch();
				}
				finally
				{
					foreach (var mod in modBuffer)
					{
						mod?.Dispose();
					}
				}
			}

			return Closure;
		}

		public void AssetLoader(PatcherStage stage, Mod mod, IHandle handle)
		{
			if (handle is not IFileHandle file)
			{
				throw new ArgumentException("The MonoMod loader must be provided a file.", nameof(handle));
			}

			const string mmDll = ".mm.dll";
			var name = file.Name;
			if (!name.EndsWith(mmDll))
			{
				throw new ArgumentException("The file must end with '" + mmDll + "'.", nameof(handle));
			}

			var target = name.Substring(0, name.Length - mmDll.Length) + ".dll";
			if (!_targetMods.TryGetValue(target, out var mods))
			{
				_mod.Logger.LogDebug($"Prepping MonoMod patcher for '{target}'");

				mods = new();
				var patcher = Patch(mods);

				_targetMods.Add(target, mods);
				stage.Patchers.SetOrAdd(target, _mod, patcher);
			}

			mods.Add(file);
		}
	}

	internal class MonoModHookGenAssetLoader
	{
		private readonly Mod _mod;
		private readonly Dictionary<string, MemoryStream> _outputs = new();

		public IEnumerable<KeyValuePair<string, MemoryStream>> Outputs => _outputs;

		public MonoModHookGenAssetLoader(Mod mod)
		{
			_mod = mod;
		}

		private Patcher Patch(Stream output)
		{
			void Closure(ref AssemblyDefinition assembly)
			{
				var module = assembly.MainModule;

				using var modder = new DeliMonoModder(_mod.Logger, module);
				modder.MapDependencies();

				var generator = new HookGenerator(modder, "MMHOOK_" + module.Name)
				{
					HookPrivate = true
				};
				generator.Generate();

				generator.OutputModule.Write(output);
			}

			return Closure;
		}

		public void AssetLoader(PatcherStage stage, Mod mod, IHandle handle)
		{
			if (handle is not IFileHandle file)
			{
				throw new ArgumentException("The MonoMod.HookGen loader must be provided a file.", nameof(handle));
			}

			var reader = stage.ImmediateReaders.Get<IEnumerable<string>>();

			foreach (var target in reader(file))
			{
				if (_outputs.ContainsKey(target)) continue;

				_mod.Logger.LogDebug($"Prepping HookGen for '{target}'");

				var output = new MemoryStream();
				var patcher = Patch(output);

				_outputs.Add(target, output);
				stage.Patchers.SetOrAdd(target, _mod, patcher);
			}
		}
	}
}
