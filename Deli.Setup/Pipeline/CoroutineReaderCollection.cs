using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;

namespace Deli.Setup
{
	public class CoroutineReaderCollection : ServiceCollection
	{
		public CoroutineReaderCollection(ManualLogSource logger) : base(logger)
		{
		}

		public void Add<T>(ICoroutineReader<T> reader)
		{
			Add(typeof(T), reader);
		}

		public ICoroutineReader<T> Get<T>()
		{
			return (ICoroutineReader<T>) Get(typeof(T));
		}

		public bool TryGet<T>([MaybeNullWhen(false)] out ICoroutineReader<T> reader)
		{
			if (Services.TryGetValue(typeof(T), out var obj))
			{
				reader = (ICoroutineReader<T>) obj;
				return true;
			}

			reader = null;
			return false;
		}
	}
}
