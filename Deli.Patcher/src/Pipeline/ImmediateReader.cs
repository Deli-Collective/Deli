using Deli.VFS;

namespace Deli.Patcher
{
	/// <summary>
	///		A deserializer that operates using a single method call.
	/// </summary>
	/// <typeparam name="T">The type to deserialize to.</typeparam>
	public delegate T ImmediateReader<out T>(IFileHandle handle);
}
