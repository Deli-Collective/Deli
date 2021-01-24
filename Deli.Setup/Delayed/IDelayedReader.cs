using Deli.VFS;

namespace Deli.Setup
{
    public interface IDelayedReader<T>
    {
		ResultYieldInstruction<T> Read(IFileHandle handle);
	}
}
