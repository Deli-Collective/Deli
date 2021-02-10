using Deli.Runtime.Yielding;
using Deli.VFS;

namespace Deli.Runtime
{
	public class DelayedTypedFileHandle<T> : TypedFileHandle<DelayedReader<T>, ResultYieldInstruction<T>> where T : notnull
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
