using UnityEngine.SceneManagement;

namespace Deli.Core
{
	internal class Module : DeliModule
	{
		public Module()
		{
			Deli.AddVersionCheckable("github.com", new GitHubVersionCheckable());

			SceneManager.activeSceneChanged += SceneChanged;
		}

		private void SceneChanged(Scene current, Scene next)
		{
			foreach (var mod in Deli.Mods)
			{
				mod.Config.Reload();
			}
		}
	}
}
