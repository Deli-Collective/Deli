using System;
namespace Deli.Setup
{
	public interface IVersionChecker
	{
		ResultYieldInstruction<Version> Check(string path);
	}
}
