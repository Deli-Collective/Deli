using System.Collections.Generic;
using Mono.Cecil;

namespace Deli
{
	public interface IPatcher
	{
		void Patch(ref AssemblyDefinition assembly);
	}
}
