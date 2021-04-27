using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Deli.Patcher.Exceptions;
using Deli.VFS;

namespace Deli.Immediate
{
	/// <summary>
	///		An element of the loading sequence which uses immediate loaders. This class is not intended to be inherited outside the framework, so please don't.
	/// </summary>
	/// <typeparam name="TStage">A recursive generic to facilitate strongly typed stage parameters</typeparam>
	public abstract class ImmediateStage<TStage> : Stage<ImmediateAssetLoader<TStage>> where TStage : ImmediateStage<TStage>
	{
#pragma warning disable CS1591

		protected abstract TStage GenericThis { get; }

		protected ImmediateStage(Blob data) : base(data)
		{
		}

		private void LoadMod(Mod mod, Dictionary<string, Mod> lookup)
		{
			var table = mod.Info.Assets;
			if (table is null) return;

			var assets = GetAssets(table);
			if (assets is null) return;

			Logger.LogInfo(Locale.LoadingAssets(mod));
			foreach (var asset in assets)
			{
				var loader = GetLoader(mod, lookup, asset, out var loaderMod);

				foreach (var handle in Glob(mod, asset))
				{
					try
					{
						loader(GenericThis, mod, handle);
					}
					catch
					{
						Logger.LogFatal(Locale.LoaderException(asset.Value, loaderMod, mod, handle));
						throw;
					}
				}
			}
		}

		protected abstract Dictionary<string, AssetLoaderID>? GetAssets(Mod.AssetTable table);

		protected void AssemblyLoader(Stage stage, Mod mod, IHandle handle)
		{
			var assembly = ImmediateReaders.Get<Assembly>()(AssemblyPreloader(handle));
			AssemblyLoader(stage, mod, assembly);
		}

		protected IEnumerable<Mod> Run(IEnumerable<Mod> mods)
		{
			PreRun();

			var lookup = mods.ToDictionary(mod => mod.Info.Guid);
			foreach (var mod in lookup.Values)
			{
				if (AreDependenciesAlive(mod, lookup))
				{
					try
					{
						RunModules(mod);
						LoadMod(mod, lookup);
					}
					catch (Exception e)
					{
						// If a module failed here, log the error and disable the mod.
						Logger.LogError(e);
						mod.State.IsDisabled = true;
						mod.State.ExceptionsInternal.Add(e);
					}

					// Apparently .NET doesn't let you put yield returns inside a try/catch block. Boo.
					if (!mod.State.IsDisabled) yield return mod;
				}
				else
				{
					// If the mod's dependencies are not all alive, disable it and give a warning
					mod.State.IsDisabled = true;
					Logger.LogWarning($"Mod {mod} has been disabled because one of it's dependencies is no longer alive.");
				}
			}

			PostRun();
		}

#pragma warning restore CS1591
	}
}
