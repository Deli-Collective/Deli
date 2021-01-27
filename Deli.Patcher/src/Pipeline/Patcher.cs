using System;
using Mono.Cecil;

namespace Deli.Patcher
{
	/// <summary>
	///		An assembly patcher. Can read, write, or even replace entire assemblies.
	/// </summary>
	/// <param name="assembly">The assembly to modify.</param>
	public delegate void Patcher(ref AssemblyDefinition assembly);
}
