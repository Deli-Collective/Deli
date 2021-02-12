using Deli.Immediate;

namespace Deli.VFS
{
	/// <summary>
	///		A file handle which supports immediate deserialization to a generic type
	/// </summary>
	/// <typeparam name="T">The type to deserialize to</typeparam>
	public class ImmediateTypedFileHandle<T> : TypedFileHandle<ImmediateReader<T>, T> where T : notnull
	{
		/// <summary>
		///		Creates an instance of <see cref="ImmediateTypedFileHandle{T}"/>
		/// </summary>
		/// <param name="handle">The raw file handle to deserialize</param>
		/// <param name="reader">The immediate reader responsible for deserialization</param>
		public ImmediateTypedFileHandle(IFileHandle handle, ImmediateReader<T> reader) : base(handle, reader)
		{
		}

		/// <inheritdoc cref="TypedFileHandle{TReader,TOut}.Read"/>
		protected override T Read()
		{
			return Reader(this);
		}
	}
}
