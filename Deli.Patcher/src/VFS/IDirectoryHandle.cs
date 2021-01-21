using System.Collections.Generic;

namespace Deli.VFS
{
	public interface IDirectoryHandle : IHandle, IEnumerable<IChildHandle>
	{
		IChildHandle? this[string name] { get; }
	}
}
