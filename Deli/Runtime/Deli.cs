using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Atlas;
using Atlas.Fluent;
using Atlas.Impl;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace Deli
{
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class Deli : BaseUnityPlugin
    {
        private static readonly StandardServiceKernel Kernel;

        public static IServiceResolver Services => Kernel;

        public new static ManualLogSource Logger => Services.Get<ManualLogSource>().Unwrap();

        static Deli()
        {
            Kernel = new StandardServiceKernel();

            // Basic impls
            Kernel.Bind<IAssetReader<Assembly>>()
                .ToConstant(new AssemblyAssetReader());

            // Named dictionaries
            Kernel.Bind<IDictionary<string, IModuleLoader>>()
                .ToConstant(new Dictionary<string, IModuleLoader>
            {
                ["Assembly"] = new AssemblyModuleLoader()
            });
            Kernel.Bind<IDictionary<string, ModInfo>>()
                .ToConstant(new Dictionary<string, ModInfo>());

            // Enumerables
            Kernel.Bind<IEnumerable<IModuleLoader>>()
                .ToRecursiveMethod(x => x.Get<IDictionary<string, IModuleLoader>>().Map(v => (IEnumerable<IModuleLoader>) v.Values))
                .InTransientScope();
            Kernel.Bind<IEnumerable<ModInfo>>()
                .ToRecursiveMethod(x => x.Get<IDictionary<string, ModInfo>>().Map(v => (IEnumerable<ModInfo>) v.Values))
                .InTransientScope();

            // Contextual to dictionaries
            Kernel.Bind<IModuleLoader, string>()
                .ToWholeMethod((services, context) => services.Get<IDictionary<string, IModuleLoader>>()
                    .Map(x => x.OptionGetValue(context))
                    .Flatten())
                .InTransientScope();
            Kernel.Bind<ModInfo, string>()
                .ToWholeMethod((services, context) => services.Get<IDictionary<string, ModInfo>>()
                    .Map(x => x.OptionGetValue(context))
                    .Flatten())
                .InTransientScope();

            // Custom impls
            Kernel.Bind<ManualLogSource, string>()
                .ToContextualNopMethod(x => BepInEx.Logging.Logger.CreateLogSource(x))
                .InSingletonScope();
        }

        private void Awake()
        {
            Kernel.Bind<Deli>()
                .ToConstant(this);
            Kernel.Bind<ManualLogSource>()
                .ToConstant(base.Logger);
            {
                var manager = new GameObject("Deli Manager");
                DontDestroyOnLoad(manager);
                
                Kernel.Bind<GameObject>()
                    .ToConstant(manager);
            }
            
            Initialize();
        }

        /// <summary>
        ///     Enumerates the mods in the mods folder
        /// </summary>
        /// <returns>An enumerable of the mods in the mods folder</returns>
        private static IEnumerable<ModInfo> DiscoverMods(string dir)
        {
            var archives = Directory.GetFiles(dir, "*." + Constants.ModExtension, SearchOption.AllDirectories).Select(ModInfo.FromArchive);
            var directories = Directory.GetFiles(dir, Constants.ManifestFileName, SearchOption.AllDirectories).Select(ModInfo.FromManifest);
            return archives.Concat(directories);
        }

        private void Initialize()
        {
            EnsureDirectoriesExist();

            // Discover all the mods
            var modsDir = Directory.GetCurrentDirectory() + "/" + Constants.ModDirectory;
            var mods = DiscoverMods(modsDir).ToArray();
            Logger.LogInfo($"Discovered {mods.Length} mods");

            // Make sure all dependencies are satisfied
            if (!CheckDependencies(mods))
            {
                Logger.LogError("One or more dependencies are not satisfied. Aborting initialization.");
                return;
            }

            // Load the mods
            try
            {
                // Sort the mods in the order they depend on each other
                var sorted = mods.TSort(x => mods.Where(m => x.Dependencies.Select(d => d.Split('@')[0]).Contains(m.Guid)), true);
                foreach (var mod in sorted) LoadMod(mod);
            }
            catch (Exception e)
            {
                Logger.LogError("Could not initialize mod framework.\n" + e);
            }
        }

        private bool CheckDependencies(ModInfo[] mods)
        {
            var pass = true;

            foreach (var mod in mods)
            foreach (var dep in mod.Dependencies)
            {
                // Split the dependency by @ and extract the target version
                var split = dep.Split('@');

                // Try finding the installed dependency
                var dependency = mods.FirstOrDefault(m => m.Guid == split[0]);
                if (dependency == null)
                {
                    Logger.LogError($"Mod {mod.Name} depends on {dep} but it is not installed!");
                    pass = false;
                }
                // Check if the installed version satisfies the dependency request
                else if (!dependency.Version.Satisfies(split[1]))
                {
                    Logger.LogError($"Mod {mod.Name} depends on {dep} but version {dependency.VersionString} is installed!");
                    pass = false;
                }
            }

            return pass;
        }

        private static void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(Constants.ModDirectory);
            Directory.CreateDirectory(Constants.ConfigDirectory);
        }

        private void LoadMod(ModInfo mod)
        {
            // For each module inside the mod, load it
            foreach (var module in mod.Modules)
            {
                if (!Services.Get<IModuleLoader, string>(module.Loader).MatchSome(out var loader))
                {
                    Logger.LogError($"Module not found for {mod}: {module.Loader}");
                    continue;
                }

                loader.LoadModule(Kernel, mod, module);
            }

            // Add the ModInfo to the kernel.
            Services.Get<IDictionary<string, ModInfo>>().Unwrap().Add(mod.Name, mod);
        }
    }
}