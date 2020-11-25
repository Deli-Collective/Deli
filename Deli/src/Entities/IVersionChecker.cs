using System;
using System.Collections;
using ADepIn;

namespace Deli
{
	public interface IVersionChecker
	{
		Option<Version> Result { get; }
		IEnumerator GetLatestVersion(Mod mod);
	}
}
