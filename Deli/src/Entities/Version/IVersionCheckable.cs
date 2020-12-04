using System.Collections.Generic;
using ADepIn;

namespace Deli
{
	/// <summary>
	/// 	Represents a version checker for a domain(s). Similar to the <see cref="IEnumerable{T}"/> pattern.
	/// </summary>
	public interface IVersionCheckable
	{
		/// <summary>
		/// 	Gets a checker for the path if it valid, otherwise returns None
		/// </summary>
		/// <param name="mod">The mod to check</param>
		/// <param name="path">The path of the URL, without leading and trailing forward slashes</param>
		Option<IVersionChecker> Check(Mod mod, string path);
	}
}
