namespace H3ModFramework
{
    public interface IReader<out T>
    {
        T Read(byte[] raw);
    }
}