using System;
using System.Collections.Generic;
using System.Reflection;
using Atlas;
using Atlas.Fluent.Impl;
using BepInEx.Logging;
using UnityEngine;

namespace Deli
{
    /// <summary>
    /// Module Loader for assemblies.
    /// </summary>
    public class AssemblyModuleLoader : IAssetLoader
    {
        private static Option<ConstructorInfo> GetParameterlessCtor<TAttribute>(Type type, IServiceResolver services)
        {
            return type.GetParameterlessCtor().Map(v =>
            {
                if (services.Get<ManualLogSource>().MatchSome(out var log))
                {
                    log.LogError($"Type {type} is annotated with {typeof(TAttribute)}, but does not contain a public, parameterless constructor.");
                }

                return v;
            });
        }

        public void LoadAsset(IServiceKernel kernel, Mod mod, string path)
        {
            // Load the assembly and scan it for new module loaders and resource type loaders
            var assembly = mod.Resources.Get<Assembly>(path).Expect("Assembly not found at path: " + path);

            // Try to discover any mod plugins in the assembly
            foreach (var type in assembly.GetTypesSafe())
            {
                if (kernel.LoadEntryType(type).IsSome)
                {
                    continue;
                }

                if (type.GetCustomAttribute<QuickUnnamedBindAttribute>().MatchSome(out var unnamedAttr) &&
                    GetParameterlessCtor<QuickUnnamedBindAttribute>(type, kernel).MatchSome(out var ctor))
                {
                    var services = unnamedAttr.AsServices;

                    if (services.Length == 0)
                    {
                        services = type.GetInterfaces();
                    }

                    var inst = ctor.Invoke(new object[0]);
                    foreach (var service in services)
                    {
                        var binderGenericArguments = new[] { service, typeof(Unit) };
                        var bindingType = typeof(ConstantServiceBinding<,>).MakeGenericType(binderGenericArguments);
                        var binderBindMethod = typeof(IServiceBinder).GetMethod(nameof(IServiceBinder.Bind)).MakeGenericMethod(binderGenericArguments);

                        var bindingInst = Activator.CreateInstance(bindingType, inst);
                        binderBindMethod.Invoke(kernel, new[] { bindingInst });
                    }

                    continue;
                }

                if (type.GetCustomAttribute<QuickNamedBindAttribute>().MatchSome(out var namedAttr) &&
                    GetParameterlessCtor<QuickNamedBindAttribute>(type, kernel).MatchSome(out ctor))
                {
                    var services = namedAttr.AsServices;

                    if (services.Length == 0)
                    {
                        services = type.GetInterfaces();
                    }

                    var inst = ctor.Invoke(new object[0]);
                    var dictAddParameters = new[] { namedAttr.Name, inst };
                    foreach (var service in services)
                    {
                        var genericDictArguments = new[] { typeof(string), service };
                        var dict = typeof(IDictionary<,>).MakeGenericType(genericDictArguments);
                        var dictAddMethod = dict.GetMethod(nameof(IDictionary<object, object>.Add));

                        var genericKernelArguments = new[] { dict, typeof(Unit) };
                        var resolverGetMethod = typeof(IServiceResolver).GetMethod(nameof(IServiceResolver.Get)).MakeGenericMethod(genericKernelArguments);

                        var dictOptInst = resolverGetMethod.Invoke(kernel, new object[] { default(Unit) });

                        var option = typeof(Option<>).MakeGenericType(dict);
                        var optionMatchSomeMethod = option.GetMethod(nameof(Option<object>.MatchSome));

                        var matched = new object[] { null };
                        object dictInst;
                        if ((bool) optionMatchSomeMethod.Invoke(dictOptInst, matched))
                        {
                            dictInst = matched[0];
                        }
                        else
                        {
                            var bindingType = typeof(ConstantServiceBinding<,>).MakeGenericType(genericKernelArguments);
                            var bindMethod = typeof(IServiceBinder).GetMethod(nameof(IServiceBinder.Bind)).MakeGenericMethod(genericKernelArguments);

                            var genericDictImpl = typeof(Dictionary<,>).MakeGenericType(genericDictArguments);
                            dictInst = Activator.CreateInstance(genericDictImpl);

                            var binding = Activator.CreateInstance(bindingType, dictInst);
                            bindMethod.Invoke(kernel, new[] { binding });
                        }

                        dictAddMethod.Invoke(dictInst, dictAddParameters);
                    }

                    continue;
                }

                if (type.IsSubclassOf(typeof(DeliMod)))
                {
                    var manager = kernel.Get<GameObject>().Expect("Could not find manager object.");

                    manager.SetActive(false);
                    try
                    {
                        var modClass = (DeliMod) manager.AddComponent(type);
                        modClass.BaseMod = mod;
                    }
                    finally
                    {
                        manager.SetActive(true);
                    }
                }
            }
        }
    }
}