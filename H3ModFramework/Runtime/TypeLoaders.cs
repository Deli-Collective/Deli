using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace H3ModFramework
{
    public static class TypeLoaders
    {
        public static Dictionary<Type, MethodInfo> RegisteredTypeLoaders = new Dictionary<Type, MethodInfo>();

        /// <summary>
        /// Scans the provided assembly for valid type loader methods and adds them to the dictionary.
        /// </summary>
        public static void ScanAssembly(Assembly assembly)
        {
            foreach (var method in assembly.GetTypes().SelectMany(t => t.GetMethods()).Where(m => m.IsStatic))
            {
                // Check if we have the type loader attribute on the method
                var attributes = method.GetCustomAttributes(typeof(TypeLoaderAttribute), false);
                if (attributes.Length <= 0) continue;

                if (RegisteredTypeLoaders.ContainsKey(method.ReturnType))
                {
                    H3ModFramework.PublicLogger.LogWarning($"Duplicate TypeLoader for type {method.ReturnType}. Ignoring duplicate implementation.");
                    continue;
                }

                // Verify the type loader is valid
                var parameters = method.GetParameters();
                if (parameters.Length != 1 || parameters[0].ParameterType != typeof(byte[]))
                {
                    H3ModFramework.PublicLogger.LogError($"Cannot register TypeLoader for method {method}, it is invalid.");
                    continue;
                }

                // If it's valid, add it to the dictionary
                RegisteredTypeLoaders[method.ReturnType] = method;
                H3ModFramework.PublicLogger.LogInfo("Registered TypeLoader for " + method.ReturnType);
            }
        }

        #region Type Loader Methods

        [TypeLoader]
        public static string TypeLoaderString(byte[] raw) => Encoding.UTF8.GetString(raw, 0, raw.Length);

        [TypeLoader]
        public static AssetBundle TypeLoaderAssetBundle(byte[] raw) => AssetBundle.LoadFromMemory(raw);

        [TypeLoader]
        public static Assembly TypeLoaderAssembly(byte[] raw) => Assembly.Load(raw);

        #endregion
    }

    /// <summary>
    /// Attribute to assign to methods that are Type Loaders
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class TypeLoaderAttribute : Attribute
    {
    }
}