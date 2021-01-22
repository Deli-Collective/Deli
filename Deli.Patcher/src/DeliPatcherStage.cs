using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace Deli.Patcher
{
	internal class DeliPatcherStage : IPatcherStage
	{
		private static DeliPatcherStage? _instance;

		internal static void Handoff(Action<ManualLogSource, ImmediateReaderCollection> callback)
		{
			if (_instance is null)
			{
				throw new InvalidOperationException("Stage has not initialized or handoff was already completed.");
			}

			callback(_instance._logger, _instance.ImmediateReaders);
			_instance = null;
		}

		private readonly ManualLogSource _logger;
		private readonly Dictionary<string, IImmediateAssetLoader> _assetLoaders = new();
		internal readonly Dictionary<string, List<IPatcher>> FilePatchers = new();

		public ImmediateReaderCollection ImmediateReaders { get; }

		public event Action? Started;
		public event Action? Finished;

		public DeliPatcherStage()
		{
			_logger = Logger.CreateLogSource(DeliMetadata.Name);

			ImmediateReaders = new ImmediateReaderCollection(_logger);

			_instance = this;
		}

		public IDisposable AddAssetLoader(string name, IImmediateAssetLoader loader)
		{
			if (_assetLoaders.ContainsKey(name))
			{
				throw new InvalidOperationException($"An asset loader with the same name ({name}) already exists.");
			}

			_assetLoaders.Add(name, loader);
			return new ActionDisposable(() => _assetLoaders.Remove(name));
		}

		public void AddPatcher(string fileName, IPatcher patcher)
		{
			if (!FilePatchers.TryGetValue(fileName, out var patchers))
			{
				patchers = new List<IPatcher>();
				FilePatchers.Add(fileName, patchers);
			}

			patchers.Add(patcher);
		}
	}
}
