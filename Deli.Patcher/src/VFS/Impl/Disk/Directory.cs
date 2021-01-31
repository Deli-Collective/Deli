using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Deli.VFS.Disk
{
	public abstract class DirectoryHandle : IDirectoryHandle, IEnumerable<IDiskChildHandle>, IDiskHandle
	{
		private readonly Dictionary<string, IDiskChildHandle> _handles = new();

		public string Path { get; }
		public string PathOnDisk { get; }

		protected DirectoryHandle(string path, string pathOnDisk)
		{
			Path = path;
			PathOnDisk = pathOnDisk;

			AddNew(Directory.GetDirectories(PathOnDisk), Directory.GetFiles(PathOnDisk), _handles);
		}

		// Adds handles that still exist on disk.
		private void AddAlive(HashSet<string> directories, HashSet<string> files, Dictionary<string, IDiskChildHandle> buffer)
		{
			static bool IsAlive<THandle>(HashSet<string> paths, THandle handle) where THandle : IDiskHandle
			{
				var path = handle.PathOnDisk;
				if (paths.Contains(path))
				{
					paths.Remove(path);
					return true;
				}

				return false;
			}

			foreach (var entry in _handles)
			{
				var name = entry.Key;
				var handle = entry.Value;

				if (handle switch
				{
					DirectoryHandle directory => IsAlive(directories, directory),
					FileHandle file => IsAlive(files, file),
					_ => throw new InvalidOperationException("A non-Deli handle implementation got into the Deli directory implementation somehow.")
				}) buffer.Add(name, handle);
			}
		}

		// Add handles that have newly appeared on disk
		private void AddNew(IEnumerable<string> directories, IEnumerable<string> files, Dictionary<string, IDiskChildHandle> buffer)
		{
			foreach (var path in directories)
			{
				var name = System.IO.Path.GetFileName(path);
				var handle = new ChildDirectoryHandle(name, path, this);

				buffer.Add(handle.Name, handle);
			}

			foreach (var path in files)
			{
				var name = System.IO.Path.GetFileName(path);
				var handle = new FileHandle(name, path, this);

				buffer.Add(handle.Name, handle);
			}
		}

		public void Refresh()
		{
			this.ThrowIfDead();

			var directories = new HashSet<string>(Directory.GetDirectories(PathOnDisk));
			var files = new HashSet<string>(Directory.GetFiles(PathOnDisk));
			var buffer = new Dictionary<string, IDiskChildHandle>();

			AddAlive(directories, files, buffer);

			foreach (var existing in buffer.Values)
			{
				existing.Refresh();
			}

			AddNew(directories, files, buffer);
		}

		public IDiskChildHandle? this[string name]
		{
			get
			{
				this.ThrowIfDead();

				return _handles.TryGetValue(name, out var child) ? child : null;
			}
		}

		IChildHandle? IDirectoryHandle.this[string name] => this[name];

		public Dictionary<string, IDiskChildHandle>.ValueCollection.Enumerator GetEnumerator()
		{
			this.ThrowIfDead();

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
