using System.Reflection;

namespace Deli.Patcher
{
	internal class Module : DeliModule
	{
		private bool _setupRan;

		private readonly MonoModAssetLoader _monoMod;
		private readonly MonoModHookGenAssetLoader _monoModHookGen;

		public Module(Mod source) : base(source)
		{
			_monoMod = new MonoModAssetLoader(Source);
			_monoModHookGen = new MonoModHookGenAssetLoader(Source);

			Stages.Patcher += OnPatcher;
			Stages.Other += OnOther;
		}

		private void OnPatcher(PatcherStage stage)
		{
			stage.PatcherAssetLoaders[Source, "monomod"] = _monoMod.AssetLoader;
			stage.PatcherAssetLoaders[Source, "monomod.hookgen"] = _monoModHookGen.AssetLoader;
		}

		private void OnOther(Stage stage)
		{
			if (_setupRan) return;

			foreach (var generated in _monoModHookGen.Outputs)
			{
				Logger.LogDebug($"Loading HookGen'd result of '{generated.Key}'...");

				var bytes = generated.Value.ToArray();
				Assembly.Load(bytes);
			}

			_setupRan = true;
		}
	}
}
