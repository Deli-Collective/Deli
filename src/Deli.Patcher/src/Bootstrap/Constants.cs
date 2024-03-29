using System;
using System.IO;
using System.Reflection;
using Semver;

namespace Deli.Bootstrap
{
	/// <summary>
	///		Constants used by Deli
	/// </summary>
	public static class Constants
	{
		/// <summary>
		///		Information about Deli itself
		/// </summary>
		public static class Metadata
		{
			/// <summary>
			/// 	The name of this project
			/// </summary>
			public const string Name = "Deli";

			/// <summary>
			/// 	The GUID of the implicit mod and setup plugin
			/// </summary>
			public const string Guid = "deli";

			/// <summary>
			///		The SemVersion this was built from but already parsed
			/// </summary>
			public static readonly SemVersion Version = SemVersion.Parse("MACRO_VERSION");

			/// <summary>
			///		A short except of what Deli is
			/// </summary>
			public const string Description = "Deli Eliminates Loader Intricacies; a mod loader built on top of BepInEx that can be extended to load any format of content.";

			/// <summary>
			///		The maintainers of Deli
			/// </summary>
			public static string[] Authors => new[]
			{
				"nrgill28",
				"AshHat"
			};

			/// <summary>
			///		The URL to the source code
			/// </summary>
			public const string SourceUrl = "https://github.com/Deli-Collective/Deli";

			/// <summary>
			/// 	The system-compliant version that this was built from
			/// </summary>
			public const string SysVersion = "MACRO_SYS_VERSION";
		}

		/// <summary>
		///		The Git information present when Deli was built.
		/// </summary>
		public static class Git
		{
			/// <summary>
			/// 	The result of the following command at compile-time:
			/// 	<code>git describe --long --always --dirty</code>
			/// </summary>
			public const string Describe = "MACRO_GIT_DESCRIBE";

			/// <summary>
			/// 	The result of the following command at compile-time:
			/// 	<code>git rev-parse --abbrev-ref HEAD</code>
			/// </summary>
			public const string Branch = "MACRO_GIT_BRANCH";

			/// <summary>
			/// 	The result of the following command at compile-time:
			/// 	<code>git rev-parse HEAD</code>
			/// </summary>
			public const string Hash = "MACRO_GIT_HASH";
		}

		/// <summary>
		///		Information pertaining to the filesystem that Deli uses.
		/// </summary>
		public static class Filesystem
		{
			/// <summary>
			///		The directory containing all Deli related content.
			/// </summary>
			public static string Directory { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
			                                          ?? throw new IOException("Deli is installed in the root directory. Don't do that.");

			/// <summary>
			///		The directory containing cachable files of Deli. These can be deleted with insignificant repercussions.
			/// </summary>
			public static string CacheDirectory { get; } = Path.Combine(Directory, "cache");

			/// <summary>
			/// The name of the manifest file expected in mods.
			/// </summary>
			public const string ManifestName = "manifest.json";
		}

		/// <summary>
		///		Information related to the asset pipeline
		/// </summary>
		public static class Assets
		{
			/// <summary>
			///		The name of the assembly asset loaders
			/// </summary>
			public const string AssemblyLoaderName = "assembly";
		}
	}
}
