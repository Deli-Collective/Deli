using System;
using System.Collections.Generic;
using Deli.Patcher;
using Deli.Patcher.Common;
using UnityEngine;

namespace Deli.Setup
{
	public class SetupStage : ImmediateStage<SetupStage>
	{
		private readonly GameObject _manager;

		protected override string Name { get; } = "setup";
		protected override SetupStage GenericThis => this;

		public NestedServiceCollection<Mod, string, ImmediateAssetLoader<SetupStage>> SetupAssetLoaders { get; } = new();

		public SetupStage(Blob data, GameObject manager) : base(data)
		{
			_manager = manager;
		}

		protected override void TypeLoader(Stage stage, Mod mod, Type type)
		{
			base.TypeLoader(stage, mod, type);

			if (!type.IsAbstract || typeof(DeliBehaviour).IsAssignableFrom(type))
			{
				ref var source = ref DeliBehaviour.GlobalSource;

				source = mod;
				try
				{
					_manager.AddComponent(type);
				}
				finally
				{
					source = null;
				}
			}
		}

		protected override ImmediateAssetLoader<SetupStage>? GetLoader(Mod mod, string name)
		{
			if (SetupAssetLoaders.TryGet(mod, name, out var setup))
			{
				return setup;
			}

			if (SharedAssetLoaders.TryGet(mod, name, out var shared))
			{
				return shared;
			}

			return null;
		}

		protected override Dictionary<string, AssetLoaderID>? GetAssets(Mod.Manifest manifest)
		{
			return manifest.Setup;
		}

		internal new IEnumerable<Mod> LoadMods(IEnumerable<Mod> mods) => base.LoadMods(mods);
	}
}
