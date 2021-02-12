namespace Deli.VFS.Disk
{
	/// <summary>
	///		A handle which exists on a physical disk
	/// </summary>
	public interface IDiskHandle : IHandle
	{
		/// <summary>
		///		The non-virtual path to this handle
		/// </summary>
		string PathOnDisk { get; }

		/// <summary>
		///		Checks for changes or deletions to this handle or children, and invokes the corresponding events
		/// </summary>
		void Refresh();
	}

	/// <summary>
	///		A handle which exists on a physical disk and is within a directory
	/// </summary>
	public interface IDiskChildHandle : IDiskHandle, IChildHandle
	{
	}
}
