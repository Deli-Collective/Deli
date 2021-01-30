using System.Collections.Generic;

namespace Deli.VFS.Globbing
{
	public interface IGlobber
	{
		IEnumerable<IHandle> Matches(IDirectoryHandle directory);
	}
}
