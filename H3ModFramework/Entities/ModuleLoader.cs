using System;
using System.Collections.Generic;
using System.Reflection;

namespace H3ModFramework
{
    public abstract class ModuleLoader
    {
        public abstract void LoadModule(ModInfo mod, ModInfo.ModuleInfo module);
    }

    [ModuleLoader(Name = "Assembly")]
    public class AssemblyModuleLoader : ModuleLoader
    {
        public AssemblyModuleLoader()
        {
            H3ModFramework.PostInitialization += PostInitialization;
        }

        private static readonly List<H3VRMod> _loadedModClasses = new List<H3VRMod>();
        
        public override void LoadModule(ModInfo mod, ModInfo.ModuleInfo module)
        {
            var assembly = mod.GetResource<Assembly>(module.FilePath);
            ModuleLoaderAttribute.ScanAssembly(assembly);
            TypeLoaders.ScanAssembly(assembly);
            
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(H3VRMod))) continue;
                var modClass = (H3VRMod) Activator.CreateInstance(type, mod, H3ModFramework.GetLogger(mod.Name));
                _loadedModClasses.Add(modClass);
            }
        }

        private static void PostInitialization()
        {
            foreach (var modClass in _loadedModClasses) modClass.Start();
        }
    }
    
    public class ModuleLoaderAttribute : Attribute
    {
        public static readonly Dictionary<string, ModuleLoader> Cache = new Dictionary<string, ModuleLoader>();
        
        public string Name; 

        public static void ScanAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                var attributes = type.GetCustomAttributes(typeof(ModuleLoaderAttribute), false);
                if (attributes.Length == 0) continue;
                var modLoader = (ModuleLoaderAttribute) attributes[0];

                Cache[modLoader.Name] = (ModuleLoader) Activator.CreateInstance(type);
            }
        }
    }
}