using System;
using System.IO;

namespace Deli.VFS.Disk
{
	public sealed class FileHandle : IFileHandle, IDiskChildHandle
	{
		public string Name { get; }

		public DirectoryHandle Directory { get; }
		IDirectoryHandle IChildHandle.Directory => Directory;

		public string PathOnDisk { get; }

		public event Action? Updated;

		internal FileHandle(string name, string pathOnDisk, DirectoryHandle directory)
		{
			Name = name;
			Directory = directory;
			PathOnDisk = pathOnDisk;
		}

		internal void Refresh()
		{
			// TODO: check if this file has changed, and invoke updated if so
		}

		public Stream OpenRead()
		{
			return File.OpenRead(PathOnDisk);
		}

		public override string ToString()
		{
			return PathOnDisk;
		}
	}
}
