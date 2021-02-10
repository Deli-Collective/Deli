using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;

namespace Deli.Immediate
{
	public class ImmediateReaderCollection : ServiceCollection
	{
		public ImmediateReaderCollection(ManualLogSource logger) : base(logger)
		{
		}

		public void Add<T>(ImmediateReader<T> reader) where T : notnull
		{
			Add(typeof(T), reader);
		}

		public ImmediateReader<T> Get<T>() where T : notnull
		{
			return (ImmediateReader<T>) Get(typeof(T));
		}

		public bool TryGet<T>([MaybeNullWhen(false)] out ImmediateReader<T> reader) where T : notnull
		{
			if (Services.TryGetValue(typeof(T), out var obj))
			{
				reader = (ImmediateReader<T>) obj;
				return true;
			}

			reader = null;
			return false;
		}
	}
}
