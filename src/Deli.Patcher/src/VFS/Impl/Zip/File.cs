using System;
using System.IO;
using Ionic.Zip;

namespace Deli.VFS.Zip
{
	/// <summary>
	///		A file that exists within a zip file
	/// </summary>
	public sealed class FileHandle : IFileHandle
	{
		private readonly ZipEntry _entry;

		/// <inheritdoc cref="IHandle.IsAlive"/>
		public bool IsAlive { get; } = true;

		/// <inheritdoc cref="IHandle.Path"/>
		public string Path { get; }

		/// <inheritdoc cref="IChildHandle.Directory"/>
		public DirectoryHandle Directory { get; }
		IDirectoryHandle IChildHandle.Directory => Directory;

		/// <inheritdoc cref="IChildHandle.Name"/>
		public string Name { get; }

		/// <inheritdoc cref="IHandle.Updated"/>
		public event Action? Updated;

		/// <inheritdoc cref="IHandle.Deleted"/>
		public event Action? Deleted;

		internal FileHandle(ZipEntry entry, string name, DirectoryHandle directory)
		{
			_entry = entry;

			Path = HPath.Combine(directory, name);
			Name = name;
			Directory = directory;
		}

		/// <inheritdoc cref="IFileHandle.OpenRead"/>
		public Stream OpenRead()
		{
			return _entry.OpenReader();
		}

		public override string ToString()
		{
			return Path;
		}
	}
}
