namespace Deli.VFS.Disk
{
	public interface IDiskHandle : IHandle
	{
		string PathOnDisk { get; }

		void Refresh();
	}

	public interface IDiskChildHandle : IDiskHandle, IChildHandle
	{
	}
}
