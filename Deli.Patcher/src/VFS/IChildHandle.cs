namespace Deli.VFS
{
	public interface IChildHandle : INamedHandle
	{
		IDirectoryHandle Directory { get; }
	}
}
