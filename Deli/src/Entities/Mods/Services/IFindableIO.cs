using System.Collections.Generic;

namespace Deli
{
	/// <summary>
	/// 	Represents a searchable handle to paths
	/// </summary>
	public interface IFindableIO
	{
		/// <summary>
		///		Enumerates the files in the mod whose filenames match the provided pattern.
		/// </summary>
		/// <param name="pattern">The pattern to search with.</param>
		IEnumerable<string> Find(string pattern);
	}
}
