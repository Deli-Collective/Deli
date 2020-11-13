namespace H3ModFramework
{
    public interface IAssetReader<out T>
    {
        T ReadAsset(byte[] raw);
    }
}