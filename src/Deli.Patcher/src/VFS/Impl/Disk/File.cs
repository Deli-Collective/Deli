using System;
using System.IO;

namespace Deli.VFS.Disk
{
	/// <summary>
	///		A file that exists on a physical disk
	/// </summary>
	public sealed class FileHandle : IFileHandle, IDiskChildHandle
	{
		private DateTime _lastWriteUtc;

		private DateTime DiskLastWriteUtc => File.GetLastWriteTimeUtc(PathOnDisk);

		/// <inheritdoc cref="IHandle.IsAlive"/>
		public bool IsAlive { get; private set; } = true;

		/// <inheritdoc cref="IHandle.Path"/>
		public string Path { get; }

		/// <inheritdoc cref="INamedHandle.Name"/>
		public string Name { get; }

		/// <inheritdoc cref="IChildHandle.Directory"/>
		public DirectoryHandle Directory { get; }
		IDirectoryHandle IChildHandle.Directory => Directory;

		/// <inheritdoc cref="IDiskHandle.PathOnDisk"/>
		public string PathOnDisk { get; }

		/// <inheritdoc cref="IHandle.Updated"/>
		public event Action? Updated;

		/// <inheritdoc cref="IHandle.Deleted"/>
		public event Action? Deleted;

		internal FileHandle(string name, string pathOnDisk, DirectoryHandle directory)
		{
			Path = directory.Path + name;
			Name = name;
			Directory = directory;
			PathOnDisk = pathOnDisk;

			_lastWriteUtc = DiskLastWriteUtc;
		}

		/// <inheritdoc cref="IDiskHandle.Refresh"/>
		public void Refresh()
		{
			this.ThrowIfDead();

			if (!File.Exists(PathOnDisk))
			{
				IsAlive = false;
				Deleted?.Invoke();
				return;
			}

			var disk = DiskLastWriteUtc;
			if (disk > _lastWriteUtc)
			{
				Updated?.Invoke();
			}
			_lastWriteUtc = disk;
		}

		/// <inheritdoc cref="IFileHandle.OpenRead"/>
		public Stream OpenRead()
		{
			this.ThrowIfDead();

			return new FileStream(PathOnDisk, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, true);
		}

		/// <inheritdoc cref="object.ToString"/>
		public override string ToString()
		{
			return PathOnDisk;
		}
	}
}
