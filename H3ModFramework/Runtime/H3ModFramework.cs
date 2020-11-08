using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;

namespace H3ModFramework
{
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class H3ModFramework : BaseUnityPlugin
    {
        public static H3ModFramework Instance;
        public static ManualLogSource PublicLogger;

        public static event Action PostInitialization;
        
        private void Awake()
        {
            Instance = this;
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
            // Scan this assembly for stuff
            TypeLoaders.ScanAssembly(Assembly.GetExecutingAssembly());
            ModuleLoaderAttribute.ScanAssembly(Assembly.GetExecutingAssembly());

            var modsDir = Directory.GetCurrentDirectory() + "/" + Constants.ModDirectory;
            // Sort the mods in the order they depend on each other
            var mods = DiscoverMods(modsDir).ToArray();
            try
            {
                var sorted = mods.TSort(x => mods.Where(m => x.Dependencies.Contains(m.Guid)), true);
                foreach (var mod in sorted) LoadMod(mod);
                
                // Once the mods are all done loading we can fire the PostInitialization events
                PostInitialization?.Invoke();
            }
            catch (Exception e)
            {
                PublicLogger.LogError("Could not initialize mod framework.\n" + e);
            }
        }

        private static void LoadMod(ModInfo mod)
        {
            // For each module inside the mod, load it
            foreach (var module in mod.Modules) ModuleLoaderAttribute.Cache[module.Loader].LoadModule(mod, module);
        }
    }
}