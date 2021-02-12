using Deli.Runtime.Yielding;
using Deli.VFS;

namespace Deli.Runtime
{
	/// <summary>
	///		A file handle which supports delayed deserialization to a generic type
	/// </summary>
	/// <typeparam name="T">The type to deserialize to</typeparam>
	public class DelayedTypedFileHandle<T> : TypedFileHandle<DelayedReader<T>, ResultYieldInstruction<T>> where T : notnull
	{
		/// <summary>
		///		Creates an instance of <see cref="DelayedTypedFileHandle{T}"/>
		/// </summary>
		/// <param name="handle">The raw file handle to deserialize</param>
		/// <param name="reader">The delayed reader responsible for deserialization</param>
		public DelayedTypedFileHandle(IFileHandle handle, DelayedReader<T> reader) : base(handle, reader)
		{
		}

		/// <inheritdoc cref="DelayedTypedFileHandle{T}"/>
		protected override ResultYieldInstruction<T> Read()
		{
			return Reader(this);
		}
	}
}
