using System;
using Deli.VFS;

namespace Deli
{
	public interface IResourceReader<out T>
	{
		T Read(IFileHandle handle);
	}
}
