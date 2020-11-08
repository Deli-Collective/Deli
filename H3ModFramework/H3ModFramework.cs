using BepInEx;
using BepInEx.Logging;

namespace H3ModFramework
{
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class H3ModFramework : BaseUnityPlugin
    {
        public static H3ModFramework Instance;
        public static ManualLogSource PublicLogger;

        private void Awake()
        {
            Instance = this;
            PublicLogger = GetLogger("H3ML");
        }

        public static ManualLogSource GetLogger(string name)
        {
            var logger = new ManualLogSource(name);
            BepInEx.Logging.Logger.Sources.Add(logger);
            return logger;
        }

        public static void LoadMods()
        {
            // Steps required to load mods:
            // 1: Read all the mod metadata
        }
    }
}