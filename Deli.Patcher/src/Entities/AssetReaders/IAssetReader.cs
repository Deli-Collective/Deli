namespace Deli
{
	public interface IAssetReader<out T>
	{
		T ReadAsset(byte[] raw);
	}
}
