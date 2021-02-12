using System;
using System.IO;
using System.Collections.Generic;

namespace Deli.VFS
{
	/// <summary>
	///		A handle to a file system entity
	/// </summary>
	public interface IHandle
	{
		/// <summary>
		///		Whether or not this handle is still accessible
		/// </summary>
		bool IsAlive { get; }

		/// <summary>
		///		The path to the entity
		/// </summary>
		string Path { get; }

		/// <summary>
		///		Invoked when this handle is mutated
		/// </summary>
		event Action? Updated;

		/// <summary>
		///		Invoked when this handle no longer physically exists
		/// </summary>
		event Action? Deleted;
	}

	/// <summary>
	///		A handle which is a file
	/// </summary>
	public interface IFileHandle : IChildHandle
	{
		/// <summary>
		///		Opens a read-only stream of the file contents
		/// </summary>
		Stream OpenRead();
	}

	/// <summary>
	///		A handle which is a directory
	/// </summary>
	public interface IDirectoryHandle : IHandle, IEnumerable<IChildHandle>
	{
		/// <summary>
		///		Returns the immediate child of the directory with the given name, or <see langword="null"/> if it does not exist.
		/// </summary>
		/// <param name="name">The exact, case sensitive name of the the immediate child handle</param>
		IChildHandle? this[string name] { get; }
	}

	/// <summary>
	///		A handle which is within a directory (this could be non-root directories, files)
	/// </summary>
	public interface IChildHandle : IHandle
	{
		/// <summary>
		///		The directory (parent) this handle resides in
		/// </summary>
		IDirectoryHandle Directory { get; }

		/// <summary>
		///		The name of the handle
		/// </summary>
		string Name { get; }
	}

	/// <summary>
	///		A handle which is a non-root directory
	/// </summary>
	public interface IChildDirectoryHandle : IChildHandle, IDirectoryHandle
	{
	}
}
