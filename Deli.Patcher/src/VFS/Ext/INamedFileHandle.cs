using System.IO;

namespace Deli.VFS
{
	public static class ExtINamedHandle
	{
		public static string GetStem(this INamedHandle @this)
		{
			return Path.GetFileNameWithoutExtension(@this.Name);
		}

		public static string GetExtension(this INamedHandle @this)
		{
			return Path.GetExtension(@this.Name);
		}
	}
}
