using System;
using System.Collections;

namespace Deli.Setup
{
	public interface IVersionChecker
	{
		ResultYieldInstruction<Version> Check(string path);
	}
}
