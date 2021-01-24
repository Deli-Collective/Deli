using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;

namespace Deli.Setup
{
	public class DelayedReaderCollection : ServiceCollection
	{
		public DelayedReaderCollection(ManualLogSource logger) : base(logger)
		{
		}

		public void Add<T>(IDelayedReader<T> reader)
		{
			Add(typeof(T), reader);
		}

		public IDelayedReader<T> Get<T>()
		{
			return (IDelayedReader<T>) Get(typeof(T));
		}

		public bool TryGet<T>([MaybeNullWhen(false)] out IDelayedReader<T> reader)
		{
			if (Services.TryGetValue(typeof(T), out var obj))
			{
				reader = (IDelayedReader<T>) obj;
				return true;
			}

			reader = null;
			return false;
		}
	}
}
