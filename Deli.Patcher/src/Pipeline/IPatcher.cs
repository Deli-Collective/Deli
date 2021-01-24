using System;
using Mono.Cecil;

namespace Deli.Patcher
{
	/// <summary>
	///		An assembly mutator. Can read, write, and even replace entire assemblies.
	///		The dispose method is called when all patching is complete, in case this patcher is registered on multiple assemblies.
	/// </summary>
	public interface IPatcher : IDisposable
	{
		/// <summary>
		///		Mutates an assembly. Do whatever you want, but don't break the assembly.
		/// </summary>
		/// <param name="assembly">The assembly to mutate.</param>
		void Patch(ref AssemblyDefinition assembly);
	}
}
