using System;
using System.IO;

namespace Deli.VFS
{
	public interface IFileHandle : IChildHandle
	{
		event Action Updated;

		Stream OpenRead();
	}
}
