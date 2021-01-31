using System;
namespace Deli.Setup
{
	/// <summary>
	///		Checks the version of a mod from a URL's path.
	/// </summary>
	public interface IVersionChecker
	{
		ResultYieldInstruction<Version> Check(string path);
	}
}
