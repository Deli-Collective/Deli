using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ADepIn;
using BepInEx.Logging;

namespace Deli
{
	public class AssemblyAssetLoader : IAssetLoader
	{
		public delegate void TypeLoadHandler(IServiceKernel kernel, Mod mod, string path, Type type);

		private readonly ManualLogSource _log;

		private readonly List<TypeLoadHandler> _handlers;

		public AssemblyAssetLoader(ManualLogSource log, IEnumerable<TypeLoadHandler> handlers)
		{
			_log = log;

			_handlers = new List<TypeLoadHandler>
			{
				LoadKernelModule,
				LoadDeliModule,
				LoadQuickBind
			};
			_handlers.AddRange(handlers);
		}

		private void LoadKernelModule(IServiceKernel kernel, Mod mod, string path, Type type)
		{
			if (kernel.LoadEntryType(type).IsNone) return;

			_log.LogDebug("Loaded kernel module: " + type);
		}

		private void LoadDeliModule(IServiceKernel kernel, Mod mod, string path, Type type)
		{
			if (type.IsAbstract || !typeof(DeliModule).IsAssignableFrom(type)) return;

			Deli.ModuleSources.Add(type, mod);

			ConstructorInfo ctor;
			if ((ctor = type.GetConstructor(new[] { typeof(IServiceKernel) })) != null)
			{
				ctor.Invoke(new[] { kernel });
			}
			else if ((ctor = type.GetConstructor(new Type[0])) != null)
			{
				ctor.Invoke(new object[0]);
			}
			else
			{
				_log.LogError("Invalid Deli module constructor signature: " + type);
				return;
			}

			_log.LogDebug("Loaded Deli module: " + type);
		}

		private void LoadQuickBind(IServiceKernel kernel, Mod mod, string path, Type type)
		{
			if (!QuickBindUtilizer.TryBind(kernel, type)) return;

			_log.LogDebug("Loaded quick binds: " + type);
		}

		public void LoadAsset(IServiceKernel kernel, Mod mod, string path)
		{
			// Load the assembly and scan it for new module loaders and resource type loaders
			var rawAssembly = mod.Resources.Get<byte[]>(path).Expect("Assembly not found at path: " + path);

			// If the assembly debugging symbols are also included load those too.
			var assembly = mod.Resources.Get<byte[]>(path + ".mdb").MatchSome(out var symbols) ? Assembly.Load(rawAssembly, symbols) : Assembly.Load(rawAssembly);

			var types = assembly.GetTypesSafe(_log).ToList();
			foreach (var handler in _handlers)
			{
				foreach (var type in types)
				{
					handler.Invoke(kernel, mod, path, type);
				}
			}
		}
	}
}
