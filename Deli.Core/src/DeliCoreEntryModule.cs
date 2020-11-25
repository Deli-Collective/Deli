using System.Collections.Generic;
using ADepIn;
using ADepIn.Fluent;
using Deli.Core.VersionCheckers;

namespace Deli.Core
{
	public class DeliCoreEntryModule : IEntryModule<DeliCoreEntryModule>
	{
		public void Load(IServiceKernel kernel)
		{
			kernel.Bind<IDictionary<string, IVersionChecker>>().ToConstant(new Dictionary<string, IVersionChecker>());
		}
	}
}
