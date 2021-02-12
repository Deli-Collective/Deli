using System.IO;

namespace Deli.VFS
{
	/// <summary>
	///		Extension methods pertaining to <see cref="INamedHandle"/>
	/// </summary>
	public static class ExtINamedHandle
	{
		/// <summary>
		///		Returns the stem of the handle name
		/// </summary>
		public static string GetStem(this INamedHandle @this)
		{
			return Path.GetFileNameWithoutExtension(@this.Name);
		}

		/// <summary>
		///		Returns the extension of the handle name. If it does not have an extension, returns <see langword="null"/>.
		/// </summary>
		public static string? GetExtension(this INamedHandle @this)
		{
			var extension = Path.GetExtension(@this.Name);
			return extension?.Length > 0 ? extension.Substring(1) : null;
		}
	}
}
