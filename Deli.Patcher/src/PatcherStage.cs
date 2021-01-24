using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Deli.Patcher.Readers;
using Newtonsoft.Json;

namespace Deli.Patcher
{
	public class PatcherStage : Stage
	{
		private static PatcherStage? _instance;

		internal static void Handoff(StageHandoff callback)
		{
			if (_instance is null)
			{
				throw new InvalidOperationException("Stage has not initialized or handoff was already completed.");
			}

			callback(_instance.Logger, _instance.Serializer, _instance.JObjectImmediateReader, _instance.SharedLoaders, _instance.ImmediateReaders);
			_instance = null;
		}

		private readonly Dictionary<string, IPatcherAssetLoader> _patcherAssetLoaders = new();
		private readonly Dictionary<string, List<IPatcher>> _filePatchers;

		internal PatcherStage(ManualLogSource logger, JsonSerializer serializer, Dictionary<string, ISharedAssetLoader> sharedLoaders, ImmediateReaderCollection immediateReaders, Dictionary<string, List<IPatcher>> filePatchers)
			: base(logger, serializer, new JObjectImmediateReader(), sharedLoaders, immediateReaders)
		{
			_filePatchers = filePatchers;

			_instance = this;
		}

		public IDisposable AddAssetLoader(string name, IPatcherAssetLoader loader)
		{
			return AddAssetLoader<IPatcherAssetLoader, PatcherStage>(name, loader, _patcherAssetLoaders);
		}

		public void AddPatcher(string fileName, IPatcher patcher)
		{
			if (!_filePatchers.TryGetValue(fileName, out var patchers))
			{
				patchers = new List<IPatcher>();
				_filePatchers.Add(fileName, patchers);
			}

			patchers.Add(patcher);
		}
	}
}
