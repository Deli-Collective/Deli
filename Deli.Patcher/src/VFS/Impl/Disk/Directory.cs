using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Deli.VFS.Disk
{
	public abstract class DirectoryHandle : IDirectoryHandle, IDiskHandle, IEnumerable<IDiskChildHandle>
	{
		private readonly Dictionary<string, IDiskChildHandle> _handles = new();

		public string Path { get; }

		public string PathOnDisk { get; }

		protected DirectoryHandle(string path, string pathOnDisk)
		{
			Path = path;
			PathOnDisk = pathOnDisk;

			Refresh();
		}

		protected void Refresh()
		{
			// TODO: flush out implementation (deleted files + updated files)

			foreach (var path in Directory.GetFiles(PathOnDisk))
			{
				var name = System.IO.Path.GetFileName(path);
				var handle = new FileHandle(name, path, this);

				_handles.Add(handle.Name, handle);
			}

			foreach (var path in Directory.GetDirectories(PathOnDisk))
			{
				var name = System.IO.Path.GetFileName(path);
				var handle = new ChildDirectoryHandle(name, path, this);

				_handles.Add(handle.Name, handle);
			}
		}

		public IDiskChildHandle? this[string name] => _handles.TryGetValue(name, out var child) ? child : null;

		IChildHandle? IDirectoryHandle.this[string name] => this[name];

		public Dictionary<string, IDiskChildHandle>.ValueCollection.Enumerator GetEnumerator()
		{
			return _handles.Values.GetEnumerator();
		}

		IEnumerator<IDiskChildHandle> IEnumerable<IDiskChildHandle>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<IChildHandle> IEnumerable<IChildHandle>.GetEnumerator()
		{
			return this.ImplicitCast<IDiskChildHandle, IChildHandle>().GetEnumerator();
		}

		public override string ToString()
		{
			return PathOnDisk;
		}
	}

	public sealed class RootDirectoryHandle : DirectoryHandle
	{
		public RootDirectoryHandle(string pathOnDisk) : base("/", pathOnDisk)
		{
		}
	}

	public sealed class ChildDirectoryHandle : DirectoryHandle, IChildDirectoryHandle, IDiskChildHandle
	{
		public string Name { get; }

		public DirectoryHandle Directory { get; }
		IDirectoryHandle IChildHandle.Directory => Directory;

		internal ChildDirectoryHandle(string name, string pathOnDisk, DirectoryHandle directory) : base(directory.Path + name + "/", pathOnDisk)
		{
			Name = name;
			Directory = directory;
		}
	}
}
