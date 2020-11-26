namespace Deli
{
	public class ByteArrayAssetReader : IAssetReader<byte[]>
	{
		public byte[] ReadAsset(byte[] raw)
		{
			return raw;
		}
	}
}
