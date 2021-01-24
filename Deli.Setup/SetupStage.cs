using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Deli.Patcher;
using Deli.VFS;

namespace Deli.Setup
{
	public class SetupStage
	{
		private readonly ManualLogSource _logger;

		private readonly Dictionary<string, ISharedAssetLoader> _sharedLoaders;
		private readonly Dictionary<string, IDelayedAssetLoader> _delayedLoaders = new();
		private readonly Dictionary<Type, object> _wrapperReaders = new();

		public ImmediateReaderCollection ImmediateReaders { get; }

		public DelayedReaderCollection CoroutineReaders { get; }

		public event Action? Started;
		public event Action? Finished;

		internal SetupStage(ManualLogSource logger, Dictionary<string, ISharedAssetLoader> sharedAssetLoaders, ImmediateReaderCollection immediateReaders)
		{
			_logger = logger;
			_sharedLoaders = sharedAssetLoaders;

			ImmediateReaders = immediateReaders;
			CoroutineReaders = new DelayedReaderCollection(logger);
		}

		public IDisposable AddAssetLoader(string name, IDelayedAssetLoader loader)
		{
			if (_delayedLoaders.ContainsKey(name))
			{
				throw new InvalidOperationException($"An asset loader with the same name ({name}) already exists.");
			}

			_delayedLoaders.Add(name, loader);
			return new ActionDisposable(() => _delayedLoaders.Remove(name));
		}

		public IDelayedReader<T> GetReader<T>()
		{
			if (CoroutineReaders.TryGet<T>(out var reader))
			{
				_wrapperReaders.Remove(typeof(T));
				return reader;
			}

			var type = typeof(T);
			if (_wrapperReaders.TryGetValue(type, out var obj))
			{
				return (IDelayedReader<T>) obj;
			}

			var immediate = ImmediateReaders.Get<T>();
			var wrapper = new ImmediateReaderWrapper<T>(immediate);
			_wrapperReaders.Add(typeof(T), wrapper);

			return wrapper;
		}

		private class ImmediateReaderWrapper<T> : IDelayedReader<T>
		{
			private readonly IImmediateReader<T> _immediate;

			public ImmediateReaderWrapper(IImmediateReader<T> immediate)
			{
				_immediate = immediate;
			}

			public ResultYieldInstruction<T> Read(IFileHandle handle)
			{
				var result = _immediate.Read(handle);

				return new DummyYieldInstruction<T>(result);
			}
		}
	}
}
