using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using Deli.Patcher;
using Deli.VFS;

namespace Deli.Setup
{
	internal class DeliSetupStage : ISetupStage
	{
		private readonly ManualLogSource _logger;

		private readonly Dictionary<string, ICoroutineAssetLoader> _loaders = new();
		private readonly Dictionary<Type, object> _wrapperReaders = new();

		public ImmediateReaderCollection ImmediateReaders { get; }

		public CoroutineReaderCollection CoroutineReaders { get; }

		public event Action? Started;
		public event Action? Finished;

		public DeliSetupStage(ManualLogSource logger, ImmediateReaderCollection immediateReaders)
		{
			_logger = logger;

			ImmediateReaders = immediateReaders;
			CoroutineReaders = new CoroutineReaderCollection(logger);
		}

		public IDisposable AddAssetLoader(string name, ICoroutineAssetLoader loader)
		{
			if (_loaders.ContainsKey(name))
			{
				throw new InvalidOperationException($"An asset loader with the same name ({name}) already exists.");
			}

			_loaders.Add(name, loader);
			return new ActionDisposable(() => _loaders.Remove(name));
		}

		public ICoroutineReader<T> GetReader<T>()
		{
			if (CoroutineReaders.TryGet<T>(out var reader))
			{
				_wrapperReaders.Remove(typeof(T));
				return reader;
			}

			var type = typeof(T);
			if (_wrapperReaders.TryGetValue(type, out var obj))
			{
				return (ICoroutineReader<T>) obj;
			}

			var immediate = ImmediateReaders.Get<T>();
			var wrapper = new ImmediateReaderWrapper<T>(immediate);
			_wrapperReaders.Add(typeof(T), wrapper);

			return wrapper;
		}

		private class ImmediateReaderWrapper<T> : ICoroutineReader<T>
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
