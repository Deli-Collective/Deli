using Deli.VFS;

namespace Deli.Setup
{
	/// <summary>
	///		A deserializer that is performed over multiple frames.
	/// </summary>
	/// <typeparam name="T">The type to deserialize to.</typeparam>
    public interface IDelayedReader<T>
    {
		ResultYieldInstruction<T> Read(IFileHandle handle);
	}
}
