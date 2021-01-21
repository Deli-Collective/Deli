namespace Deli.VFS
{
	public interface INamedHandle : IHandle
	{
		string Name { get; }
	}
}
