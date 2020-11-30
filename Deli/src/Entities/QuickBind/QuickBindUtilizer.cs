using System;
using System.Collections.Generic;
using System.Linq;
using ADepIn;
using ADepIn.Fluent.Impl;

namespace Deli
{
	internal static class QuickBindUtilizer
	{
		private delegate void CheckCallback<TAttribute>(IServiceKernel kernel, TAttribute attribute, Type[] services, object instance) where TAttribute : QuickBindAttribute;

		private static bool CheckFor<TAttribute>(IServiceKernel kernel, Type type, CheckCallback<TAttribute> callback) where TAttribute : QuickBindAttribute
		{
			var attributes = type.GetCustomAttributes<TAttribute>().ToList();
			if (attributes.Count == 0) return false;

			var ctor = type.GetParameterlessCtor().Expect($"Type {type} is annotated with {typeof(TAttribute)}, but does not contain a public, parameterless constructor.");
			var instance = ctor.Invoke(new object[0]);

			foreach (var attribute in attributes)
			{
				var services = attribute.AsServices;
				if (services.Length == 0) services = type.GetInterfaces();

				callback(kernel, attribute, services, instance);
			}

			return true;
		}

		/// <summary>
		///		Checks if the given type has an unnamed quick bind attribute
		/// </summary>
		/// <param name="kernel">Services</param>
		/// <param name="type">Type to check</param>
		/// <returns>True if the type had an unnamed quick bind attribute</returns>
		private static bool CheckForUnnamedQuickBind(IServiceKernel kernel, Type type)
		{
			return CheckFor<QuickUnnamedBindAttribute>(kernel, type, (closureKernel, _, services, instance) =>
			{
				foreach (var service in services)
				{
					var binderGenericArguments = new[] {service, typeof(Unit)};
					var bindingType = typeof(ConstantServiceBinding<,>).MakeGenericType(binderGenericArguments);
					var binderBindMethod = typeof(IServiceBinder).GetMethod(nameof(IServiceBinder.Bind)).MakeGenericMethod(binderGenericArguments);

					var bindingInst = Activator.CreateInstance(bindingType, instance);
					binderBindMethod.Invoke(closureKernel, new[] {bindingInst});
				}
			});
		}

		/// <summary>
		///		Checks if the given type has a named quick bind attribute
		/// </summary>
		/// <param name="kernel">Services</param>
		/// <param name="type">Type to check</param>
		/// <returns>True if the type has a named quick bind attribute</returns>
		private static bool CheckForNamedQuickBind(IServiceKernel kernel, Type type)
		{
			return CheckFor<QuickNamedBindAttribute>(kernel, type, (closureKernel, attribute, services, instance) =>
			{
				var dictAddParameters = new[] {attribute.Name, instance};
				foreach (var service in services)
				{
					var genericDictArguments = new[] {typeof(string), service};
					var dict = typeof(IDictionary<,>).MakeGenericType(genericDictArguments);
					var dictAddMethod = dict.GetMethod(nameof(IDictionary<object, object>.Add));

					var genericKernelArguments = new[] {dict, typeof(Unit)};
					var resolverGetMethod = typeof(IServiceResolver).GetMethod(nameof(IServiceResolver.Get)).MakeGenericMethod(genericKernelArguments);

					var dictOptInst = resolverGetMethod.Invoke(closureKernel, new object[] {default(Unit)});

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
						bindMethod.Invoke(closureKernel, new[] {binding});
					}

					dictAddMethod.Invoke(dictInst, dictAddParameters);
				}
			});
		}

		public static bool TryBind(IServiceKernel kernel, Type type)
		{
			// Important to use bitwise, not short-circuit
			return CheckForUnnamedQuickBind(kernel, type) | CheckForNamedQuickBind(kernel, type);
		}
	}
}
