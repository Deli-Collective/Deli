using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace H3ModFramework
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
        /// Checks if the provided dependant version string is satisfied by the source version
        /// </summary>
        public static bool Satisfies(this Version source, string dependant)
        {
            // It is satisfied if the Major version is the same and the minor version is equal or higher.
            var dep = new Version(dependant);
            return source.Major == dep.Major && source.Minor >= dep.Minor;
        }

        public static Type[] GetTypesSafe(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null).ToArray();
            }
        }
    }
}