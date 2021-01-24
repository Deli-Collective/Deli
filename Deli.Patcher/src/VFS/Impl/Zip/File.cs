using System;
using System.IO;
using Ionic.Zip;

namespace Deli.VFS.Zip
{
	public sealed class FileHandle : IFileHandle
	{
		private readonly ZipEntry _entry;

		public string Name { get; }

		public DirectoryHandle Directory { get; }
		IDirectoryHandle IChildHandle.Directory => Directory;

		public event Action? Updated;

		internal FileHandle(ZipEntry entry, string name, DirectoryHandle directory)
		{
			_entry = entry;

			Name = name;
			Directory = directory;
		}

		public Stream OpenRead()
		{
			return _entry.OpenReader();
		}
	}
}
