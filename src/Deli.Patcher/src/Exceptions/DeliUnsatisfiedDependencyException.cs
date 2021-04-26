using System;
using Semver;

namespace Deli.Patcher.Exceptions
{
	/// <summary>
	///		Exception for when a mod's dependency is unsatisfied.
	/// </summary>
	public class DeliUnsatisfiedDependencyException : DeliException
	{
		/// <summary>
		///		The depended mod's GUID.
		/// </summary>
		public string DependencyGuid { get; }

		/// <summary>
		///		The depended mod's info, if it is installed.
		/// </summary>
		public Mod.Manifest? DependencyInfo { get; }

		/// <summary>
		///		The required version.
		/// </summary>
		public SemVersion DependencyVersion { get; }

		/// <summary>
		///		Constructor for DeliUnsatisfiedDependencyException when the depended mod is installed,
		///		but does not satisfy the dependent's version requirement
		/// </summary>
		/// <param name="mod">The mod that has the unsatisfied dependency</param>
		/// <param name="dependencyInfo">The manifest of the depended mod</param>
		/// <param name="version">The required version of the depended mod</param>
		public DeliUnsatisfiedDependencyException(Mod mod, Mod.Manifest dependencyInfo, SemVersion version) :
			base(mod, $"{mod.Info.Name} depends on {dependencyInfo.Name ?? dependencyInfo.Guid} @ {version} but {dependencyInfo.Version} is installed!")
		{
			DependencyGuid = dependencyInfo.Guid;
			DependencyInfo = dependencyInfo;
			DependencyVersion = version;
		}

		/// <summary>
		///		Constructor for DeliUnsatisfiedDependencyException when the dependency is completely missing
		/// </summary>
		/// <param name="mod">The mod that has the unsatisfied dependency</param>
		/// <param name="guid">The depended mod's GUID</param>
		/// <param name="version"></param>
		public DeliUnsatisfiedDependencyException(Mod mod, string guid, SemVersion version) :
			base(mod, $"{mod.Info.Name} depends on {guid} @ {version} but it is not installed!")
		{
			DependencyGuid = guid;
			DependencyInfo = null;
			DependencyVersion = version;
		}
	}
}
