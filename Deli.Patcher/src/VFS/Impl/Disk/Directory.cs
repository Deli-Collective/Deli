using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Deli.VFS.Disk
{
	public abstract class DirectoryHandle : IDirectoryHandle, IDiskHandle, IEnumerable<IDiskChildHandle>
	{
		private readonly Dictionary<string, IDiskChildHandle> _handles;

		public string PathOnDisk { get; }

		protected DirectoryHandle(string pathOnDisk)
		{
			_handles = new Dictionary<string, IDiskChildHandle>();

			PathOnDisk = pathOnDisk;

			Refresh();
		}

		protected void Refresh()
		{
			// TODO: flush out implementation (deleted files + updated files)

			foreach (var path in System.IO.Directory.GetFiles(PathOnDisk))
			{
				var name = Path.GetFileName(path);
				var handle = new FileHandle(name, path, this);

				_handles.Add(handle.Name, handle);
			}

			foreach (var path in System.IO.Directory.GetDirectories(PathOnDisk))
			{
				var handle = new ChildDirectoryHandle(path, this);

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
			return this.Cast<IChildHandle>().GetEnumerator();
		}

		public override string ToString()
		{
			return PathOnDisk;
		}
	}

	public sealed class RootDirectoryHandle : DirectoryHandle
	{
		public RootDirectoryHandle(string pathOnDisk) : base(pathOnDisk)
		{
		}
	}

	public sealed class ChildDirectoryHandle : DirectoryHandle, IDiskChildHandle
	{
		public string Name { get; }

		public DirectoryHandle Directory { get; }
		IDirectoryHandle IChildHandle.Directory => Directory;

		internal ChildDirectoryHandle(string pathOnDisk, DirectoryHandle directory) : base(pathOnDisk)
		{
			Name = System.IO.Path.GetFileName(pathOnDisk);
			Directory = directory;
		}
	}
}
