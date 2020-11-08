using BepInEx;
using BepInEx.Logging;

namespace H3ModLoader
{
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class H3ModLoader : BaseUnityPlugin
    {
        public static H3ModLoader Instance;
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
    }
}