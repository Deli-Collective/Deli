using System.Collections.Generic;
using Mono.Cecil;

namespace Deli
{
	public interface IPatcher
	{
		string TargetDLL { get; }

		void Patch(ref AssemblyDefinition assembly);
	}
}
