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
        public override void LoadModule(ModInfo mod, ModInfo.ModuleInfo module)
        {
            var assembly = mod.GetResource<Assembly>(module.FilePath);
            ModuleLoaderAttribute.ScanAssembly(assembly);
        }
    }
    
    public class ModuleLoaderAttribute : Attribute
    {
        public static Dictionary<string, ModuleLoader> Cache = new Dictionary<string, ModuleLoader>();
        
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