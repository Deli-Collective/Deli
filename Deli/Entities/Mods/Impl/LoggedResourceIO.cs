using Atlas;
using BepInEx.Logging;

namespace Deli
{
    public class LoggedModIO : IResourceIO
    {
        private readonly ManualLogSource _log;
        private readonly IResourceIO _resources;

        public LoggedModIO(ManualLogSource log, IResourceIO resources)
        {
            _log = log;
            _resources = resources;
        }

        public Option<T> Get<T>(string path)
        {
            _log.LogDebug($"Retrieving asset [{typeof(T)}: {path}]");
                
            return _resources.Get<T>(path);
        }
    }
}