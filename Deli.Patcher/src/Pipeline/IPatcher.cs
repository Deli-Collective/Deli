using Mono.Cecil;

namespace Deli.Patcher
{
	public interface IPatcher
	{
		void Patch(ref AssemblyDefinition assembly);
	}
}
