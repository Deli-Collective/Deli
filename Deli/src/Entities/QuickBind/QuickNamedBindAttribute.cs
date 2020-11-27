using System;

namespace Deli
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
	public class QuickNamedBindAttribute : QuickBindAttribute
	{
		public QuickNamedBindAttribute(string name, params Type[] asServices) : base(asServices)
		{
			Name = name;
		}

		public string Name { get; }
	}
}
