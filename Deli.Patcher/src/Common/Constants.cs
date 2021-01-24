namespace Deli
{
	public static class DeliConstants
	{
		public static class Metadata
		{
			/// <summary>
			/// 	The name of the runtime plugin and patcher plugin
			/// </summary>
			public const string Name = "Deli";

			/// <summary>
			/// 	The GUID of the runtime plugin
			/// </summary>
			public const string Guid = "deli.deli";

			/// <summary>
			/// 	The version of the runtime plugin and patcher plugin
			/// </summary>
			public const string Version = "MACRO_VERSION";
		}

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

		public static class Filesystem
		{
			public const string Directory = Metadata.Name;
			public const string ConfigsDirectory = Directory + "/configs";
			public const string ModsDirectory = Directory + "/mods";
			public const string ManifestName = "manifest.json";
		}
	}
}
