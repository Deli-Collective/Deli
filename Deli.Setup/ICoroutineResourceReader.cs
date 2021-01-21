using Deli.VFS;

namespace Deli
{
    public interface ICoroutineResourceReader<T>
    {
		ResultYieldInstruction<T> Read(IFileHandle handle);
	}
}
