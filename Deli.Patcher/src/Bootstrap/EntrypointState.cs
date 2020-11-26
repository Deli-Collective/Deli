using System.Collections.Generic;

namespace Deli
{
	internal readonly struct EntrypointState
	{
		public IEnumerable<string> TargetDLLs { get; }
	}
}
