using System.Text;

namespace Deli.Core
{
    [QuickUnnamedBind]
    public class StringAssetReader : IAssetReader<string>
    {
        public string ReadAsset(byte[] raw)
        {
            return Encoding.UTF8.GetString(raw);
        }
    }
}