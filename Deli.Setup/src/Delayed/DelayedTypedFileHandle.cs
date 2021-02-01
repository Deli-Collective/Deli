using Deli.VFS;

namespace Deli.Setup
{
	public class DelayedTypedFileHandle<T> : TypedFileHandle<DelayedReader<T>, ResultYieldInstruction<T>>
	{
		public DelayedTypedFileHandle(IFileHandle handle, DelayedReader<T> reader) : base(handle, reader)
		{
		}

		protected override ResultYieldInstruction<T> Read()
		{
			return Reader(this);
		}
	}
}
