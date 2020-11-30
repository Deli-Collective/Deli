namespace Deli
{
	/// <summary>
	/// 	Represents a deserializer for mod assets
	/// </summary>
	/// <typeparam name="T">The type to deserialize to</typeparam>
	public interface IAssetReader<out T>
	{
		T ReadAsset(byte[] raw);
	}
}
