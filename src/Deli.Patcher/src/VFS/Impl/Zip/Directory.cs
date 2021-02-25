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
			return Path;
		}
	}

	/// <summary>
	///		A root directory that exists within a zip file
	/// </summary>
	public sealed class RootDirectoryHandle : DirectoryHandle
	{
		private readonly struct ZipDirectoryInfo
		{
			public readonly DirectoryHandle Handle;
			public readonly Dictionary<string, IChildHandle> Children;

			public ZipDirectoryInfo(DirectoryHandle handle, Dictionary<string, IChildHandle> children)
			{
				Handle = handle;
				Children = children;
			}
		}

		private static ZipDirectoryInfo GetParent(Dictionary<string, ZipDirectoryInfo> directories, string path)
		{
			var parentName = System.IO.Path.GetDirectoryName(path);
			if (!directories.TryGetValue(parentName, out var parent))
			{
				parent = AppendDirectory(directories, parentName);
			}

			return parent;
		}

		private static ZipDirectoryInfo AppendDirectory(Dictionary<string, ZipDirectoryInfo> directories, string path)
		{
			var parent = GetParent(directories, path);

			var children = new Dictionary<string, IChildHandle>();
			var handle = new ChildDirectoryHandle(System.IO.Path.GetFileName(path), children, parent.Handle);
			var info = new ZipDirectoryInfo(handle, children);
			parent.Children.Add(handle.Name, handle);

			return info;
		}

		private static void AppendFile(Dictionary<string, ZipDirectoryInfo> directories, string path, ZipEntry entry)
		{
			var parent = GetParent(directories, path);

			var handle = new FileHandle(entry, System.IO.Path.GetFileName(path), parent.Handle);
			parent.Children.Add(handle.Name, handle);
		}

		/// <summary>
		///		Creates a VFS from a <see cref="ZipFile"/>
		/// </summary>
		/// <param name="zip">The zip containing files and directories</param>
		public static RootDirectoryHandle Create(ZipFile zip)
		{
			var rootChildren = new Dictionary<string, IChildHandle>();
			var root = new RootDirectoryHandle(rootChildren);
			var directories = new Dictionary<string, ZipDirectoryInfo>
			{
				["/"] = new(root, rootChildren)
			};

			foreach (var entry in zip.Entries)
			{
				var path = "/" + entry.FileName.TrimEnd('/');

				if (directories.ContainsKey(path))
				{
					continue;
				}

				if (entry.IsDirectory)
				{
					AppendDirectory(directories, path);
				}
				else
				{
					AppendFile(directories, path, entry);
				}
			}

			return root;
		}

		private RootDirectoryHandle(Dictionary<string, IChildHandle> children) : base(children, "/")
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

		internal ChildDirectoryHandle(string name, Dictionary<string, IChildHandle> children, DirectoryHandle directory) : base(children, directory.Path + name + "/")
		{
			Name = name;
			Directory = directory;
		}
	}
}
