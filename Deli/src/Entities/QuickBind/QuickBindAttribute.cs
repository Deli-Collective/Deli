using System;

namespace Deli
{
	/// <summary>
	/// 	An attribute that binds a singleton instance of a type to a kernel
	/// </summary>
	public abstract class QuickBindAttribute : Attribute
	{
		/// <summary>
		/// 	Constructs an instance of <see cref="QuickBindAttribute"/>
		/// </summary>
		/// <param name="asServices">The services that the type should be bound to. Leave empty to bind to all implemented interfaces.</param>
		protected QuickBindAttribute(Type[] asServices)
		{
			AsServices = asServices;
		}

		/// <summary>
		/// 	The services that the type should be bound to. If empty, it is all of the interfaces the type implements.
		/// </summary>
		public Type[] AsServices { get; }
	}
}
