using ADepIn;

namespace Deli.Core
{
	internal class Module : DeliModule
	{
		public Module()
		{
			Deli.AddVersionCheckable("github.com", new GitHubVersionCheckable());
		}
	}
}
