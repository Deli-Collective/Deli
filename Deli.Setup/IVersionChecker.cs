using System;
using System.Collections;

namespace Deli
{
	public interface IVersionChecker
	{
		ResultYieldInstruction<Version> Check(string path);
	}
}
