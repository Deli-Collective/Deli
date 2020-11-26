using System;

namespace Deli
{
	public abstract class QuickBindAttribute : Attribute
	{
		protected QuickBindAttribute(Type[] asServices)
		{
			AsServices = asServices;
		}

		public Type[] AsServices { get; }
	}
}
