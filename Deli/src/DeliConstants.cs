using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Deli
{
	// Constants beginning with MACRO are set via the "macros-precompile" recipe
	/// <summary>
	/// 	Constants that Deli uses.
	/// </summary>
	public static class DeliConstants
	{
		#region Meta

		/// <summary>
		/// 	The name of the runtime plugin and patcher plugin
		/// </summary>
		public const string Name = "Deli";

		/// <summary>
		/// 	The GUID of the runtime plugin
		/// </summary>
		public const string Guid = "nrgill28.deli";

		/// <summary>
		/// 	The version of the runtime plugin and patcher plugin
		/// </summary>
		public const string Version = "MACRO_VERSION";

		#endregion

		#region Git

		/// <summary>
		/// 	The result of the following command at compile-time:
		/// 	<code>git describe --long --always --dirty</code>
		/// </summary>
		public const string GitDescribe = "MACRO_GIT_DESCRIBE";

		/// <summary>
		/// 	The result of the following command at compile-time:
		/// 	<code>git rev-parse --abbrev-ref HEAD</code>
		/// </summary>
		public const string GitBranch = "MACRO_GIT_BRANCH";

		/// <summary>
		/// 	The result of the following command at compile-time:
		/// 	<code>git rev-parse HEAD</code>
		/// </summary>
		public const string GitHash = "MACRO_GIT_HASH";

		#endregion

		#region Filesystem

		/// <summary>
		/// 	The directory that contains mod files
		/// </summary>
		public const string ModDirectory = "mods";
		/// <summary>
		/// 	The file extensions that Deli will load as mods, given that it is in <seealso cref="ModDirectory"/>
		/// </summary>
		public static IEnumerable<string> ModExtensions { get; } = new ReadOnlyCollection<string>(new []{"zip", "deli"});

		/// <summary>
		/// 	The directory that contains mod configuration files
		/// </summary>
		public const string ConfigDirectory = "mods/configs";
		/// <summary>
		/// 	The file extensions that Deli will load as mod configurations, given that it is in <seealso cref="ConfigDirectory"/>
		/// </summary>
		public const string ConfigExtension = "cfg";

		/// <summary>
		/// 	The file name of the manifest file of a Deli mod
		/// </summary>
		public const string ManifestFileName = "manifest.json";

		/// <summary>
		///		A dictionary of glob characters to replace with regex stuff.
		/// </summary>
		public static readonly Dictionary<string, string> GlobReplacements = new Dictionary<string, string>
		{
			{"\\*", "[^/]+"},
			{"\\?", "[^/]"},
			{"\\*\\*", ".+?"}
		};

		#endregion

		/// <summary>
		/// 	The name of the asset loader that loads managed assemblies (applies to patch-time and runtime)
		/// </summary>
		public const string AssemblyLoaderName = "assembly";
	}
}
