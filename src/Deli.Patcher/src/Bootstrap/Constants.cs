using Semver;

namespace Deli.Bootstrap
{
	/// <summary>
	///		Constants used by Deli.
	/// </summary>
	public static class Constants
	{
		/// <summary>
		///		Information about Deli itself.
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
			/// 	The textual semversion that this was built from
			/// </summary>
			public const string Version = "MACRO_VERSION";

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
			///		<seealso cref="Version"/>, but already parsed
			/// </summary>
			public static readonly SemVersion SemVersion = SemVersion.Parse(Version);
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
			public const string Directory = Metadata.Name;

			/// <summary>
			///		The directory containing cachable files of Deli. These can be deleted with insignificant repercussions.
			/// </summary>
			public const string CacheDirectory = Directory + "/cache";

			/// <summary>
			///		The directory containing config files of Deli mods.
			/// </summary>
			public const string ConfigsDirectory = Directory + "/configs";

			/// <summary>
			///		The directory containing Deli mods.
			/// </summary>
			public const string ModsDirectory = Directory + "/mods";

			/// <summary>
			/// The name of the manifest file expected in mods.
			/// </summary>
			public const string ManifestName = "manifest.json";
		}

		public static class Assets
		{
			public const string AssemblyLoader = "assembly";
		}
	}
}
