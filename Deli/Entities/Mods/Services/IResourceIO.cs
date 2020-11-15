using Atlas;

namespace Deli
{
    public interface IResourceIO
    {
        Option<T> Get<T>(string path);
    }
}