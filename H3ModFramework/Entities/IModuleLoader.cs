using System;
using System.Collections.Generic;
using System.Reflection;

namespace H3ModFramework
{
    /// <summary>
    /// Interface for module loaders
    /// </summary>
    public interface IModuleLoader
    {
        void LoadModule(ModInfo mod, ModInfo.ModuleInfo module);
    }

    /// <summary>
    /// Attribute for module loaders. Accepts the loader name
    /// </summary>
    public class ModuleLoaderAttribute : Attribute
    {
        public static readonly Dictionary<string, IModuleLoader> Cache = new Dictionary<string, IModuleLoader>();

        public string Name;

        /// <summary>
        /// Scans the given assembly for additional module loaders
        /// </summary>
        /// <param name="assembly">Assembly to scan</param>
        public static void ScanAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypesSafe())
            {
                var attributes = type.GetCustomAttributes(typeof(ModuleLoaderAttribute), false);
                if (attributes.Length == 0) continue;
                var modLoader = (ModuleLoaderAttribute) attributes[0];

                Cache[modLoader.Name] = (IModuleLoader) Activator.CreateInstance(type);
            }
        }
    }
}