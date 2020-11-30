using ADepIn;

namespace Deli.Core
{
	internal class Module : IEntryModule<Module>
	{
		public void Load(IServiceKernel kernel)
		{
			Deli.AddVersionCheckable("github.com", new GitHubVersionCheckable());
		}
	}
}
