using System.Reflection;

namespace Deli
{
	public class AssemblyAssetReader : IAssetReader<Assembly>
	{
		public Assembly ReadAsset(byte[] raw)
		{
			return Assembly.Load(raw);
		}
	}
}
