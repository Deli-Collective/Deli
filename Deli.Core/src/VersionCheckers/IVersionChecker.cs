using System;
using System.Collections;
using ADepIn;

namespace Deli.Core.VersionCheckers
{
	public interface IVersionChecker
	{
		Option<Version> Result { get; }
		IEnumerator GetLatestVersion(Mod mod);
	}
}
