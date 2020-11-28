using ADepIn;
using Deli.Core.VersionCheckers;

namespace Deli.Core
{
	internal class Module : IEntryModule<Module>
	{
		public void Load(IServiceKernel kernel)
		{
			Deli.AddVersionChecker("https://github.com", new GitHubVersionChecker());
		}
	}
}
