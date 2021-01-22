namespace Deli
{
	public static class DeliMetadata
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
	}
}
