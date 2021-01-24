namespace Deli.VFS.Disk
{
	public interface IDiskHandle
	{
		string PathOnDisk { get; }
	}

	public interface IDiskChildHandle : IDiskHandle, IChildHandle
	{
	}
}
