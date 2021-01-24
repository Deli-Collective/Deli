using Deli.VFS;

namespace Deli.Patcher
{
	/// <summary>
	///		A deserializer that operates using a single method call.
	/// </summary>
	/// <typeparam name="T">The type to deserialize to.</typeparam>
	public interface IImmediateReader<out T>
	{
		T Read(IFileHandle handle);
	}
}
