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

namespace H3ModFramework
{
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class H3ModFramework : BaseUnityPlugin
    {
        private static readonly StandardServiceKernel _kernel;

        public static IServiceResolver Services => _kernel;

        public static new ManualLogSource Logger => Services.Get<ManualLogSource>().Unwrap();

        static H3ModFramework()
        {
            _kernel = new StandardServiceKernel();

            // Basic impls
            _kernel.Bind<IAssetReader<Assembly>>()
                .ToConstant(new AssemblyAssetReader());

            // Named dictionaries
            _kernel.Bind<IDictionary<string, IModuleLoader>>()
                .ToConstant(new Dictionary<string, IModuleLoader>
            {
                ["Assembly"] = new AssemblyModuleLoader()
            });
            _kernel.Bind<IDictionary<string, ModInfo>>()
                .ToConstant(new Dictionary<string, ModInfo>());

            // Enumerables
            _kernel.Bind<IEnumerable<IModuleLoader>>()
                .ToRecursiveMethod(x => x.Get<IDictionary<string, IModuleLoader>>().Map(v => (IEnumerable<IModuleLoader>) v.Values))
                .InTransientScope();
            _kernel.Bind<IEnumerable<ModInfo>>()
                .ToRecursiveMethod(x => x.Get<IDictionary<string, ModInfo>>().Map(v => (IEnumerable<ModInfo>) v.Values))
                .InTransientScope();

            // Contextual to dictionaries
            _kernel.Bind<IModuleLoader, string>()
                .ToWholeMethod((services, context) => services.Get<IDictionary<string, IModuleLoader>>()
                    .Map(x => x.OptionGetValue(context))
                    .Flatten())
                .InTransientScope();
            _kernel.Bind<ModInfo, string>()
                .ToWholeMethod((services, context) => services.Get<IDictionary<string, ModInfo>>()
                    .Map(x => x.OptionGetValue(context))
                    .Flatten())
                .InTransientScope();

            // Custom impls
            _kernel.Bind<ManualLogSource, string>()
                .ToContextualNopMethod(x => BepInEx.Logging.Logger.CreateLogSource(x))
                .InSingletonScope();
        }

        private void Awake()
        {
            _kernel.Bind<H3ModFramework>()
                .ToConstant(this);
            _kernel.Bind<ManualLogSource>()
                .ToConstant(base.Logger);
            {
                var manager = new GameObject("H3ModFramework Manager");
                DontDestroyOnLoad(manager);
                
                _kernel.Bind<GameObject>()
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

                loader.LoadModule(_kernel, mod, module);
            }

            // Add the ModInfo to the kernel.
            Services.Get<IDictionary<string, ModInfo>>().Unwrap().Add(mod.Name, mod);
        }
    }
}