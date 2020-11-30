using System;

namespace Deli
{
	/// <summary>
	/// 	Binds a type to an unnamed (global) service entry
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
	public class QuickUnnamedBindAttribute : QuickBindAttribute
	{
		/// <summary>
		/// 	Constructs an instance of <see cref="QuickUnnamedBindAttribute"/>
		/// </summary>
		/// <param name="asServices">The services that the type should be bound to. Leave empty to bind to all implemented interfaces.</param>
		public QuickUnnamedBindAttribute(params Type[] asServices) : base(asServices)
		{
		}
	}
}
