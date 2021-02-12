using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Deli.VFS.Disk
{
	/// <summary>
	///		A directory that exists on a physical disk
	/// </summary>
	public abstract class DirectoryHandle : IDirectoryHandle, IEnumerable<IDiskChildHandle>, IDiskHandle
	{
		private readonly Dictionary<string, IDiskChildHandle> _handles = new();

		/// <inheritdoc cref="IHandle.IsAlive"/>
		public bool IsAlive { get; private set; } = true;

		/// <inheritdoc cref="IHandle.Path"/>
		public string Path { get; }

		/// <inheritdoc cref="IDiskHandle.PathOnDisk"/>
		public string PathOnDisk { get; }

		/// <inheritdoc cref="IHandle.Updated"/>
		public event Action? Updated;

		/// <inheritdoc cref="IHandle.Deleted"/>
		public event Action? Deleted;

		internal DirectoryHandle(string path, string pathOnDisk)
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

		/// <inheritdoc cref="IDiskHandle.Refresh"/>
		public void Refresh()
		{
			this.ThrowIfDead();

			if (!Directory.Exists(PathOnDisk))
			{
				IsAlive = false;
				_handles.Clear();
				Deleted?.Invoke();
				return;
			}

			var directories = new HashSet<string>(Directory.GetDirectories(PathOnDisk));
			var files = new HashSet<string>(Directory.GetFiles(PathOnDisk));
			var buffer = new Dictionary<string, IDiskChildHandle>();

			AddAlive(directories, files, buffer);

			foreach (var existing in buffer.Values)
			{
				existing.Refresh();
			}

			var hasNew = directories.Count > 0 || files.Count > 0;
			if (hasNew)
			{
				AddNew(directories, files, buffer);
			}

			if (hasNew || buffer.Count != _handles.Count || !buffer.Keys.All(_handles.ContainsKey))
			{
				Updated?.Invoke();
			}
		}

		/// <inheritdoc cref="IDirectoryHandle.this"/>
		public IDiskChildHandle? this[string name]
		{
			get
			{
				this.ThrowIfDead();

				return _handles.TryGetValue(name, out var child) ? child : null;
			}
		}

		IChildHandle? IDirectoryHandle.this[string name] => this[name];

		/// <summary>
		///		Enumerates over the child handles
		/// </summary>
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

		/// <inheritdoc cref="object.ToString"/>
		public override string ToString()
		{
			return PathOnDisk;
		}
	}

	/// <summary>
	///		A root directory that exists on a physical disk
	/// </summary>
	public sealed class RootDirectoryHandle : DirectoryHandle
	{
		/// <summary>
		///		Creates an instance of <see cref="RootDirectoryHandle"/>
		/// </summary>
		/// <param name="pathOnDisk">The physical path to the root directory</param>
		public RootDirectoryHandle(string pathOnDisk) : base("/", pathOnDisk)
		{
		}
	}

	/// <summary>
	///		A child directory that exists on a physical disk
	/// </summary>
	public sealed class ChildDirectoryHandle : DirectoryHandle, IChildDirectoryHandle, IDiskChildHandle
	{
		/// <inheritdoc cref="INamedHandle.Name"/>
		public string Name { get; }

		/// <inheritdoc cref="IChildHandle.Directory"/>
		public DirectoryHandle Directory { get; }
		IDirectoryHandle IChildHandle.Directory => Directory;

		internal ChildDirectoryHandle(string name, string pathOnDisk, DirectoryHandle directory) : base(directory.Path + name + "/", pathOnDisk)
		{
			Name = name;
			Directory = directory;
		}
	}
}
