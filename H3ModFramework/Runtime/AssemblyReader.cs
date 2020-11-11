using System.Reflection;

namespace H3ModFramework
{
    public class AssemblyReader : IReader<Assembly>
    {
        public Assembly Read(byte[] raw)
        {
            return Assembly.Load(raw);
        }
    }
}