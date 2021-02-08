using System;
using System.IO;

namespace Deli.VFS.Disk
{
	public sealed class FileHandle : IFileHandle, IDiskChildHandle
	{
		private DateTime _lastWriteUtc;

		private DateTime DiskLastWriteUtc => File.GetLastWriteTimeUtc(PathOnDisk);

		public string Path { get; }

		public string Name { get; }

		public DirectoryHandle Directory { get; }
		IDirectoryHandle IChildHandle.Directory => Directory;

		public string PathOnDisk { get; }

		public event Action? Updated;

		internal FileHandle(string name, string pathOnDisk, DirectoryHandle directory)
		{
			Path = directory.Path + name;
			Name = name;
			Directory = directory;
			PathOnDisk = pathOnDisk;

			_lastWriteUtc = DiskLastWriteUtc;
		}

		public void Refresh()
		{
			this.ThrowIfDead();

			var disk = DiskLastWriteUtc;
			if (_lastWriteUtc < disk)
			{
				Updated?.Invoke();
			}
			_lastWriteUtc = disk;
		}

		public Stream OpenRead()
		{
			this.ThrowIfDead();

			return new FileStream(PathOnDisk, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, true);
		}

		public override string ToString()
		{
			return PathOnDisk;
		}
	}
}
