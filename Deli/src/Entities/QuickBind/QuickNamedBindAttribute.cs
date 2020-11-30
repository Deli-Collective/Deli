using System;

namespace Deli
{
	/// <summary>
	/// 	Binds a type to a named service entry
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
	public class QuickNamedBindAttribute : QuickBindAttribute
	{
		/// <summary>
		/// 	Constructs an instance of <see cref="QuickBindAttribute"/>
		/// </summary>
		/// <param name="name">The name of the bound service</param>
		/// <param name="asServices">The services that the type should be bound to. Leave empty to bind to all implemented interfaces.</param>
		public QuickNamedBindAttribute(string name, params Type[] asServices) : base(asServices)
		{
			Name = name;
		}

		/// <summary>
		/// 	The name of the bound service
		/// </summary>
		public string Name { get; }
	}
}
