using System.Reflection;

namespace Deli.Patcher
{
	internal class Module : DeliModule
	{
		private bool _setupRan;

		private readonly AssetLoaders _assetLoaders;

		public Module(Mod source) : base(source)
		{
			var hookGenDebug = Config.Bind("Patchers", "MonoModDebug", false, "Whether or not to enable debug logging for MonoMod.");

			_assetLoaders = new AssetLoaders(Source, hookGenDebug);

			Stages.Patcher += OnPatcher;
			Stages.Other += OnOther;
		}

		private void OnPatcher(PatcherStage stage)
		{
			stage.PatcherAssetLoaders[Source, "monomod"] = _assetLoaders.MonoModAssetLoader;
			stage.PatcherAssetLoaders[Source, "monomod.hookgen"] = _assetLoaders.MonoModHookGenAssetLoader;
		}

		private void OnOther(Stage stage)
		{
			if (_setupRan) return;

			foreach (var generated in _assetLoaders.Hooks)
			{
				Logger.LogDebug($"Loading HookGen'd result of '{generated.Key}'");

				var bytes = generated.Value.ToArray();
				Assembly.Load(bytes);
			}

			_setupRan = true;
		}
	}
}
