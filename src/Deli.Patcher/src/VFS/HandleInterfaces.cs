using System;
using System.IO;
using System.Collections.Generic;

namespace Deli.VFS
{
	public interface IHandle
	{
		string Path { get; }
	}

	public interface INamedHandle : IHandle
	{
		string Name { get; }
	}

	public interface IFileHandle : IChildHandle
	{
		event Action Updated;

		Stream OpenRead();
	}

	public interface IDirectoryHandle : IHandle, IEnumerable<IChildHandle>
	{
		IChildHandle? this[string name] { get; }
	}

	public interface IChildHandle : INamedHandle
	{
		IDirectoryHandle Directory { get; }
	}

	public interface IChildDirectoryHandle : IChildHandle, IDirectoryHandle
	{
	}
}
