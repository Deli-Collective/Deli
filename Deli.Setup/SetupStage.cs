using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Deli.Patcher;
using Deli.Patcher.Readers;
using Deli.VFS;
using Newtonsoft.Json;

namespace Deli.Setup
{
	public class SetupStage : Stage
	{
		private readonly Dictionary<string, IDelayedAssetLoader> _delayedLoaders = new();
		private readonly Dictionary<Type, object> _wrapperReaders = new();

		/// <summary>
		///		The collection of all the <see cref="IDelayedReader{T}"/>s publicly available. This does not include wrappers for <see cref="IImmediateReader{T}"/>.
		///		For getting readers including <see cref="IImmediateReader{T}"/> wrappers, use <seealso cref="GetReader{T}"/>.
		/// </summary>
		public DelayedReaderCollection DelayedReaders { get; }

		internal SetupStage(ManualLogSource logger, JsonSerializer serializer, JObjectImmediateReader jObjectImmediateReader, Dictionary<string, ISharedAssetLoader> sharedLoaders, ImmediateReaderCollection immediateReaders) : base(logger, serializer, jObjectImmediateReader, sharedLoaders, immediateReaders)
		{
			DelayedReaders = new DelayedReaderCollection(logger);
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

		/// <summary>
		///		Gets a reader from <seealso cref="DelayedReaders"/>, otherwise gets a reader from <see cref="Stage.ImmediateReaders"/> and wraps it.
		/// </summary>
		/// <typeparam name="T">The type to deserialize.</typeparam>
		public IDelayedReader<T> GetReader<T>()
		{
			var type = typeof(T);
			if (DelayedReaders.TryGet<T>(out var reader))
			{
				_wrapperReaders.Remove(type);
				return reader;
			}

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
