using System;
using System.Collections.Generic;
using System.Reflection;
using ADepIn;
using ADepIn.Fluent.Impl;
using BepInEx.Logging;
using UnityEngine;

namespace Deli
{
	/// <summary>
	///		Module Loader for assemblies.
	/// </summary>
	public class AssemblyAssetLoader : IAssetLoader
	{
		public void LoadAsset(IServiceKernel kernel, Mod mod, string path)
		{
			// Load the assembly and scan it for new module loaders and resource type loaders
			var rawAssembly = mod.Resources.Get<byte[]>(path).Expect("Assembly not found at path: " + path);

			// If the assembly debugging symbols are also included load those too.
			var assembly = mod.Resources.Get<byte[]>(path + ".mdb").MatchSome(out var symbols) ? Assembly.Load(rawAssembly, symbols) : Assembly.Load(rawAssembly);

			// Get log in case a type load exception occurs.
			var log = kernel.Get<ManualLogSource>().Expect("Failed to acquire logger.");

			// Try to discover any mod plugins in the assembly
			// TODO: This isn't great
			var types = assembly.GetTypesSafe(log);
			foreach (var type in types)
				kernel.LoadEntryType(type);
			foreach (var type in types)
				CheckForUnnamedQuickBind(kernel, type);
			foreach (var type in types)
				CheckForNamedQuickBind(kernel, type);
			foreach (var type in types)
				CheckIsDeliBehaviour(kernel, mod, type);
		}

		/// <summary>
		///		Gets a parameterless constructor from the given type
		/// </summary>
		/// <param name="type">The type to get a constructor for</param>
		/// <param name="services">Services for logging</param>
		/// <typeparam name="TAttribute">Type for logging</typeparam>
		/// <returns>Some constructor info or None if not found</returns>
		private static Option<ConstructorInfo> GetParameterlessCtor<TAttribute>(Type type, IServiceResolver services)
		{
			if (type.GetParameterlessCtor().MatchSome(out var ctor)) return Option.Some(ctor);

			if (services.Get<ManualLogSource>().MatchSome(out var log))
				log.LogError($"Type {type} is annotated with {typeof(TAttribute)}, but does not contain a public, parameterless constructor.");

			return Option.None<ConstructorInfo>();
		}

		/// <summary>
		///		Checks if the given type has an unnamed quick bind attribute
		/// </summary>
		/// <param name="kernel">Services</param>
		/// <param name="type">Type to check</param>
		/// <returns>True if the type had an unnamed quick bind attribute</returns>
		private static bool CheckForUnnamedQuickBind(IServiceResolver kernel, Type type)
		{
			if (!type.GetCustomAttribute<QuickUnnamedBindAttribute>().MatchSome(out var unnamedAttr) || !GetParameterlessCtor<QuickUnnamedBindAttribute>(type, kernel).MatchSome(out var ctor)) return false;
			var services = unnamedAttr.AsServices;

			if (services.Length == 0) services = type.GetInterfaces();

			var inst = ctor.Invoke(new object[0]);
			foreach (var service in services)
			{
				var binderGenericArguments = new[] {service, typeof(Unit)};
				var bindingType = typeof(ConstantServiceBinding<,>).MakeGenericType(binderGenericArguments);
				var binderBindMethod = typeof(IServiceBinder).GetMethod(nameof(IServiceBinder.Bind)).MakeGenericMethod(binderGenericArguments);

				var bindingInst = Activator.CreateInstance(bindingType, inst);
				binderBindMethod.Invoke(kernel, new[] {bindingInst});
			}

			return true;
		}

		/// <summary>
		///		Checks if the given type has a named quick bind attribute
		/// </summary>
		/// <param name="kernel">Services</param>
		/// <param name="type">Type to check</param>
		/// <returns>True if the type has a named quick bind attribute</returns>
		private static bool CheckForNamedQuickBind(IServiceResolver kernel, Type type)
		{
			if (!type.GetCustomAttribute<QuickNamedBindAttribute>().MatchSome(out var namedAttr) || !GetParameterlessCtor<QuickNamedBindAttribute>(type, kernel).MatchSome(out var ctor)) return false;
			var services = namedAttr.AsServices;

			if (services.Length == 0) services = type.GetInterfaces();

			var inst = ctor.Invoke(new object[0]);
			var dictAddParameters = new[] {namedAttr.Name, inst};
			foreach (var service in services)
			{
				var genericDictArguments = new[] {typeof(string), service};
				var dict = typeof(IDictionary<,>).MakeGenericType(genericDictArguments);
				var dictAddMethod = dict.GetMethod(nameof(IDictionary<object, object>.Add));

				var genericKernelArguments = new[] {dict, typeof(Unit)};
				var resolverGetMethod = typeof(IServiceResolver).GetMethod(nameof(IServiceResolver.Get)).MakeGenericMethod(genericKernelArguments);

				var dictOptInst = resolverGetMethod.Invoke(kernel, new object[] {default(Unit)});

				var option = typeof(Option<>).MakeGenericType(dict);
				var optionMatchSomeMethod = option.GetMethod(nameof(Option<object>.MatchSome));

				var matched = new object[] {null};
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
					bindMethod.Invoke(kernel, new[] {binding});
				}

				dictAddMethod.Invoke(dictInst, dictAddParameters);
			}

			return true;
		}

		/// <summary>
		///		Checks if the given type is a valid code mod
		/// </summary>
		/// <param name="services">Services</param>
		/// <param name="mod">Base mod from where this type originated</param>
		/// <param name="type">Type to check</param>
		/// <returns>True if the type is a valid code mod</returns>
		private bool CheckIsDeliBehaviour(IServiceResolver services, Mod mod, Type type)
		{
			if (!type.IsSubclassOf(typeof(DeliBehaviour))) return false;

			var manager = services.Get<GameObject>().Expect("Could not find manager object.");
			services.Get<IDictionary<Type, Mod>>().Expect("Could not find mod type dictionary.").Add(type, mod);

			manager.AddComponent(type);

			return true;
		}
	}
}
