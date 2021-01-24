namespace Deli.Setup
{
	public sealed class DummyYieldInstruction<T> : ResultYieldInstruction<T>
	{
		public override bool keepWaiting { get; } = false;

		public override T Result { get; }

		public DummyYieldInstruction(T result)
		{
			Result = result;
		}
	}
}
