using Mono.Cecil;

namespace Deli
{
	/// <summary>
	/// 	Represents an assembly patcher for a specific assembly
	/// </summary>
	public interface IPatcher
	{
		/// <summary>
		/// 	Modifies the assembly, replacing it entirely if desired.
		/// </summary>
		/// <param name="assembly">The assembly to modify or replace</param>
		void Patch(ref AssemblyDefinition assembly);
	}
}
