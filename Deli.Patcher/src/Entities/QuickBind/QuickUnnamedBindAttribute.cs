using System;

namespace Deli
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
	public class QuickUnnamedBindAttribute : QuickBindAttribute
	{
		public QuickUnnamedBindAttribute(params Type[] asServices) : base(asServices)
		{
		}
	}
}
