using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;

namespace Deli.Patcher
{
	public class ImmediateReaderCollection : ServiceCollection
	{
		public ImmediateReaderCollection(ManualLogSource logger) : base(logger)
		{
		}

		public void Add<T>(IImmediateReader<T> reader)
		{
			Add(typeof(T), reader);
		}

		public IImmediateReader<T> Get<T>()
		{
			return (IImmediateReader<T>) Get(typeof(T));
		}

		public bool TryGet<T>([MaybeNullWhen(false)] out IImmediateReader<T> reader)
		{
			if (Services.TryGetValue(typeof(T), out var obj))
			{
				reader = (IImmediateReader<T>) obj;
				return true;
			}

			reader = null;
			return false;
		}
	}
}
