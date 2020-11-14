using System.Reflection;

namespace H3ModFramework
{
    public class AssemblyAssetReader : IAssetReader<Assembly>
    {
        public Assembly ReadAsset(byte[] raw)
        {
            return Assembly.Load(raw);
        }
    }
}