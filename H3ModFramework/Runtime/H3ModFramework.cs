using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace H3ModFramework
{
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class H3ModFramework : BaseUnityPlugin
    {
        public static H3ModFramework Instance;
        public static ManualLogSource PublicLogger;
        public static ModInfo[] InstalledMods;
        public static GameObject ManagerObject;

        public static event Action PostInitialization;

        private void Awake()
        {
            Instance = this;
            ManagerObject = new GameObject("H3ModFramework Manager");
            PublicLogger = GetLogger("H3MF");
            Initialize();
        }

        public static ManualLogSource GetLogger(string name)
        {
            var logger = new ManualLogSource(name);
            BepInEx.Logging.Logger.Sources.Add(logger);
            return logger;
        }

        /// <summary>
        /// Enumerates the mods in the mods folder
        /// </summary>
        /// <returns>An enumerable of the mods in the mods folder</returns>
        private static IEnumerable<ModInfo> DiscoverMods(string dir) => Directory.GetFiles(dir, "*." + Constants.ModExtension, SearchOption.AllDirectories).Select(ModInfo.FromFile).ToArray();

        private static void Initialize()
        {
            EnsureDirectoriesExist();
            
            // Scan this assembly for stuff
            TypeLoaders.ScanAssembly(Assembly.GetExecutingAssembly());
            ModuleLoaderAttribute.ScanAssembly(Assembly.GetExecutingAssembly());

            // Discover all the mods
            var modsDir = Directory.GetCurrentDirectory() + "/" + Constants.ModDirectory;
            InstalledMods = DiscoverMods(modsDir).ToArray();

            // Make sure all dependencies are satisfied
            if (!CheckDependencies(InstalledMods))
            {
                PublicLogger.LogError("One or more dependencies are not satisfied. Aborting initialization.");
                return;
            }

            // Load the mods
            try
            {
                // Sort the mods in the order they depend on each other
                var sorted = InstalledMods.TSort(x => InstalledMods.Where(m => x.Dependencies.Select(d => d.Split('@')[0]).Contains(m.Guid)), true);
                foreach (var mod in sorted) LoadMod(mod);
            }
            catch (Exception e)
            {
                PublicLogger.LogError("Could not initialize mod framework.\n" + e);
            }

            // Once the mods are all done loading we can fire the PostInitialization events
            PostInitialization?.Invoke();
        }

        private static bool CheckDependencies(ModInfo[] mods)
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
                    PublicLogger.LogError($"Mod {mod.Name} depends on {dep} but it is not installed!");
                    pass = false;
                }
                // Check if the installed version satisfies the dependency request
                else if (!dependency.Version.Satisfies(split[1]))
                {
                    PublicLogger.LogError($"Mod {mod.Name} depends on {dep} but version {dependency.VersionString} is installed!");
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
        
        private static void LoadMod(ModInfo mod)
        {
            // For each module inside the mod, load it
            foreach (var module in mod.Modules) ModuleLoaderAttribute.Cache[module.Loader].LoadModule(mod, module);
        }
    }
}