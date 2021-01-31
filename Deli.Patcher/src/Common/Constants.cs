namespace Deli
{
	/// <summary>
	///		Constants used by Deli.
	/// </summary>
	public static class DeliConstants
	{
		/// <summary>
		///		Information about Deli itself.
		/// </summary>
		public static class Metadata
		{
			/// <summary>
			/// 	The name of the implicit mod, setup plugin, and patcher plugin
			/// </summary>
			public const string Name = "Deli";

			/// <summary>
			/// 	The GUID of the implicit mod, setup plugin
			/// </summary>
			public const string Guid = "deli";

			/// <summary>
			/// 	The version of the implicit mod and setup plugin
			/// </summary>
			public const string Version = "MACRO_VERSION";

			/// <summary>
			///		The URL to the source code of the implicit mod
			/// </summary>
			public const string SourceUrl = "https://github.com/Deli-Counter/Deli";
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
