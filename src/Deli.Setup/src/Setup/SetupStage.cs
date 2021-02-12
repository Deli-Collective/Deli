using System;
using System.Collections.Generic;
using Deli.Immediate;
using UnityEngine;

namespace Deli.Setup
{
	/// <summary>
	///		The immediate stage of the loading sequence which allows for patched assemblies to be used
	/// </summary>
	public class SetupStage : ImmediateStage<SetupStage>
	{
		private readonly GameObject _manager;
		private readonly Dictionary<Mod, List<DeliBehaviour>> _modBehaviours;

#pragma warning disable CS1591

		protected override string Name { get; } = "setup";
		protected override SetupStage GenericThis => this;

#pragma warning restore CS1591

		/// <summary>
		///		Asset loaders specific to this stage
		/// </summary>
		public NestedServiceCollection<Mod, string, ImmediateAssetLoader<SetupStage>> SetupAssetLoaders { get; } = new();

		internal SetupStage(Blob data, GameObject manager, Dictionary<Mod, List<DeliBehaviour>> modBehaviours) : base(data)
		{
			_manager = manager;
			_modBehaviours = modBehaviours;

			SharedAssetLoaders[Mod, Bootstrap.Constants.Assets.AssemblyLoaderName] = AssemblyLoader;
		}

#pragma warning disable CS1591

		protected override void TypeLoader(Stage stage, Mod mod, Type type)
		{
			base.TypeLoader(stage, mod, type);

			if (!type.IsAbstract || typeof(DeliBehaviour).IsAssignableFrom(type))
			{
				ref var source = ref DeliBehaviour.GlobalSource;

				DeliBehaviour behaviour;
				source = mod;
				try
				{
					behaviour = (DeliBehaviour) _manager.AddComponent(type);
				}
				finally
				{
					source = null;
				}

				if (!_modBehaviours.TryGetValue(mod, out var behaviours))
				{
					behaviours = new List<DeliBehaviour>();
					_modBehaviours.Add(mod, behaviours);
				}
				behaviours.Add(behaviour);

				try
				{
					behaviour.Run(stage);
				}
				catch
				{
					Logger.LogFatal(Locale.PluginException(mod, "behaviour"));
					throw;
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

		protected override Dictionary<string, AssetLoaderID>? GetAssets(Mod.AssetTable table)
		{
			return table.Setup;
		}

#pragma warning restore CS1591

		internal IEnumerable<Mod> RunInternal(IEnumerable<Mod> mods) => Run(mods);
	}
}
