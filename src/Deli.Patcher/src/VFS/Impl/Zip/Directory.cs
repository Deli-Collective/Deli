using System.Collections;
using System.Collections.Generic;
using Ionic.Zip;

namespace Deli.VFS.Zip
{
	public abstract class DirectoryHandle : IDirectoryHandle
	{
		private readonly Dictionary<string, IChildHandle> _children;

		public string Path { get; }

		internal DirectoryHandle(Dictionary<string, IChildHandle> children, string path)
		{
			_children = new Dictionary<string, IChildHandle>();
			Path = path;
		}

		public IChildHandle? this[string name] => _children.TryGetValue(name, out var child) ? child : null;

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
	}

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

		private static FileHandle AppendFile(Dictionary<string, ZipDirectoryInfo> directories, string path, ZipEntry entry)
		{
			var parent = GetParent(directories, path);

			var handle = new FileHandle(entry, System.IO.Path.GetFileName(path), parent.Handle);
			parent.Children.Add(handle.Name, handle);

			return handle;
		}

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

		internal RootDirectoryHandle(Dictionary<string, IChildHandle> children) : base(children, "/")
		{
		}
	}

	public class ChildDirectoryHandle : DirectoryHandle, IChildDirectoryHandle
	{
		public string Name { get; }

		public DirectoryHandle Directory { get; }
		IDirectoryHandle IChildHandle.Directory => Directory;

		internal ChildDirectoryHandle(string name, Dictionary<string, IChildHandle> children, DirectoryHandle directory) : base(children, directory.Path + name + "/")
		{
			Name = name;
			Directory = directory;
		}
	}
}
