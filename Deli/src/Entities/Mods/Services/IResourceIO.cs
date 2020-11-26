using System.Collections.Generic;
using ADepIn;

namespace Deli
{
	/// <summary>
	///		Represents a typed handle to paths.
	/// </summary>
	public interface IResourceIO : IFindableIO
	{
		/// <summary>
		///		Returns Some with the value if the resource exists, otherwise None.
		/// </summary>
		/// <param name="path">The path to the resource.</param>
		/// <typeparam name="T">The type of the resource.</typeparam>
		Option<T> Get<T>(string path);

		/// <summary>
		///		Enumerates all resources whose filenames match the provided pattern.
		/// </summary>
		/// <param name="pattern">The pattern to search with.</param>
		/// <typeparam name="T">The type of the resource.</typeparam>
		IEnumerable<Option<T>> GetAll<T>(string pattern);
	}
}
