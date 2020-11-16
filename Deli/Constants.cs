namespace Deli
{
    internal static class Constants
    {
        // GUID and Version
        public const string Name = "Deli";
        public const string Guid = "nrgill28.deli";
        public const string Version = "1.0.0.0";

        // Git
        // These are set in the makefile, via the "macros" recipe
        public const string GitBranch = "STUB_GIT_BRANCH";
        public const string GitHash = "STUB_GIT_HASH";

        // Loader constants
        public const string ModExtension = "zip";
        public const string ModDirectory = "mods";
        public const string ConfigDirectory = "mods/configs";
        public const string ManifestFileName = "manifest.json";
    }
}