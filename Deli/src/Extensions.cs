using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ADepIn;
using ADepIn.Fluent;
using BepInEx.Logging;

namespace Deli
{
	// https://stackoverflow.com/a/11027096/8809017
	public static class Extensions
	{
		public static IEnumerable<T> TSort<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> dependencies, bool throwOnCycle = false)
		{
			var sorted = new List<T>();
			var visited = new HashSet<T>();

			foreach (var item in source)
				Visit(item, visited, sorted, dependencies, throwOnCycle);

			return sorted;
		}

		// https://stackoverflow.com/a/11027096/8809017
		private static void Visit<T>(T item, HashSet<T> visited, List<T> sorted, Func<T, IEnumerable<T>> dependencies, bool throwOnCycle)
		{
			if (!visited.Contains(item))
			{
				visited.Add(item);

				foreach (var dep in dependencies(item))
					Visit(dep, visited, sorted, dependencies, throwOnCycle);

				sorted.Add(item);
			}
			else
			{
				if (throwOnCycle && !sorted.Contains(item))
					throw new Exception("Cyclic dependency found");
			}
		}

		/// <summary>
		///		Checks if the provided dependant version string is satisfied by the source
		///		version
		/// </summary>
		public static bool Satisfies(this Version source, Version dependant)
		{
			// It is satisfied if the Major version is the same and the minor version is equal or higher.
			return source.Major == dependant.Major && source.Minor >= dependant.Minor;
		}

		/// <summary>
		///		Extension for Assembly.GetTypes() that won't throw an exception.
		///		This is needed to avoid a slew of ReflectionTypeLoadExceptions
		/// </summary>
		/// <param name="assembly">Assembly to get types of</param>
		/// <returns>Array of not-null types in the assembly</returns>
		public static Type[] GetTypesSafe(this Assembly assembly, ManualLogSource log)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				var exceptionsFormatted = e.LoaderExceptions.Where(x => x != null).Select(x => x.Message).ToArray();
				log.LogError($"Encountered one or more type load exceptions while getting types from {assembly}:\n{string.Join("\n", exceptionsFormatted)}");

				return e.Types.Where(t => t != null).ToArray();
			}
		}

		// This is added in .NET Framework 4, but isn't present in .NET Framework 3.5. For now, we can make our own.
		/// <summary>
		///		Copies data from this stream to the other.
		/// </summary>
		/// <param name="source">The stream to copy from.</param>
		/// <param name="dest">The stream to copy to.</param>
		public static void CopyTo(this Stream source, Stream dest)
		{
			const int bufferLength = 64 * 1024;
			var buffer = new byte[bufferLength];

			var read = source.Read(buffer, 0, bufferLength);
			while (read == bufferLength)
			{
				dest.Write(buffer, 0, bufferLength);
				read = source.Read(buffer, 0, bufferLength);
			}

			dest.Write(buffer, 0, read);
		}

		public static Option<TAttribute> GetCustomAttribute<TAttribute>(this Type @this) where TAttribute : Attribute
		{
			var attrs = @this.GetCustomAttributes(typeof(TAttribute), false);

			return attrs.Length > 0 ? Option.Some((TAttribute) attrs[0]) : Option.None<TAttribute>();
		}

		public static Option<ConstructorInfo> GetParameterlessCtor(this Type @this)
		{
			return @this.GetConstructor(new Type[0]) is ConstructorInfo ctor ? Option.Some(ctor) : Option.None<ConstructorInfo>();
		}

		public static void BindJson<T>(this IServiceKernel @this)
		{
			@this.Bind<IAssetReader<Option<T>>>().ToRecursiveNopMethod(x => new JsonAssetReader<T>(x)).InSingletonNopScope();
		}
	}
}
