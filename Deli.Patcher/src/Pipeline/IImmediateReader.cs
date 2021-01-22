using Deli.VFS;

namespace Deli.Patcher
{
	public interface IImmediateReader<out T>
	{
		T Read(IFileHandle handle);
	}
}
