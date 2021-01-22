using Deli.VFS;

namespace Deli.Setup
{
    public interface ICoroutineReader<T>
    {
		ResultYieldInstruction<T> Read(IFileHandle handle);
	}
}
