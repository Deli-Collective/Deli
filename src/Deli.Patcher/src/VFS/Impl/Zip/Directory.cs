using System;
using System.Collections;
using System.Collections.Generic;
using Ionic.Zip;

namespace Deli.VFS.Zip
{
	/// <summary>
	///		A directory that exists within a zip file
	/// </summary>
	public abstract class DirectoryHandle : IDirectoryHandle
	{
		private readonly Dictionary<string, IChildHandle> _children;

		/// <inheritdoc cref="IHandle.IsAlive"/>
		public bool IsAlive { get; } = true;

		/// <inheritdoc cref="IHandle.Path"/>
		public string Path { get; }

		/// <inheritdoc cref="IHandle.Updated"/>
		public event Action? Updated;

		/// <inheritdoc cref="IHandle.Deleted"/>
		public event Action? Deleted;

		internal DirectoryHandle(Dictionary<string, IChildHandle> children, string path)
		{
			_children = children;

			Path = path;
		}

		/// <inheritdoc cref="IDirectoryHandle.this"/>
		public IChildHandle? this[string name] => _children.TryGetValue(name, out var child) ? child : null;

		/// <summary>
		///		Enumerates over the child handles
		/// </summary>
		public Dictionary<string, IChildHandle>.ValueCollection.Enumerator GetEnumerator()
		{
			return _children.Values.GetEnumerator();
		}

		IEnumerator<IChildHandle> IEnumerable<IChildHandle>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override string ToString()
		{
			return Path + "/";
		}
	}

	/// <summary>
	///		A root directory that exists within a zip file
	/// </summary>
	public sealed class RootDirectoryHandle : DirectoryHandle
	{
		private class RootCreator
		{
			private readonly struct Directory
			{
				public readonly DirectoryHandle Handle;
				public readonly Dictionary<string, IChildHandle> Children;

				public Directory(DirectoryHandle handle, Dictionary<string, IChildHandle> children)
				{
					Handle = handle;
					Children = children;
				}
			}

			private readonly Dictionary<string, Directory> _directories = new();

			public RootDirectoryHandle Root { get; }

			public RootCreator()
			{
				var rootChildren = new Dictionary<string, IChildHandle>();
				Root = new RootDirectoryHandle(rootChildren);

				_directories.Add(Root.Path, new(Root, rootChildren));
			}

			private Directory GetParent(string childPath)
			{
				var path = System.IO.Path.GetDirectoryName(childPath);
				if (!_directories.TryGetValue(path, out var parent))
				{
					var name = System.IO.Path.GetFileName(path);
					parent = AppendDirectory(path, name);
				}

				return parent;
			}

			private Directory AppendDirectory(string path, string name)
			{
				var parent = GetParent(path);

				var children = new Dictionary<string, IChildHandle>();
				var handle = new ChildDirectoryHandle(name, children, parent.Handle);
				var info = new Directory(handle, children);

				_directories.Add(path, info);
				parent.Children.Add(handle.Name, handle);

				return info;
			}

			private void AppendFile(string path, string name, ZipEntry entry)
			{
				var parent = GetParent(path);

				var handle = new FileHandle(entry, name, parent.Handle);
				parent.Children.Add(handle.Name, handle);
			}

			public void Append(ZipFile zip)
			{
				foreach (var entry in zip.Entries)
				{
					var path = entry.FileName.TrimEnd('/');
					if (_directories.ContainsKey(path))
					{
						continue;
					}

					var name = System.IO.Path.GetFileName(path);
					if (entry.IsDirectory)
					{
						AppendDirectory(path, name);
					}
					else
					{
						AppendFile(path, name, entry);
					}
				}
			}
		}

		/// <summary>
		///		Creates a VFS from a <see cref="ZipFile"/>
		/// </summary>
		/// <param name="zip">The zip containing files and directories</param>
		public static RootDirectoryHandle Create(ZipFile zip)
		{
			var creator = new RootCreator();
			creator.Append(zip);

			return creator.Root;
		}

		private RootDirectoryHandle(Dictionary<string, IChildHandle> children) : base(children, "")
		{
		}
	}

	/// <summary>
	///		A child directory that exists within a zip file
	/// </summary>
	public class ChildDirectoryHandle : DirectoryHandle, IChildDirectoryHandle
	{
		/// <inheritdoc cref="IChildHandle.Directory"/>
		public DirectoryHandle Directory { get; }
		IDirectoryHandle IChildHandle.Directory => Directory;

		/// <inheritdoc cref="IChildHandle.Name"/>
		public string Name { get; }

		internal ChildDirectoryHandle(string name, Dictionary<string, IChildHandle> children, DirectoryHandle directory) : base(children, HPath.Combine(directory, name))
		{
			Name = name;
			Directory = directory;
		}
	}
}
