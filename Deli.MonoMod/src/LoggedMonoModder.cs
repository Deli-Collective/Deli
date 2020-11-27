using BepInEx.Logging;
using MonoMod;

namespace Deli.MonoMod
{
	internal class LoggedMonoModder : MonoModder
	{
		private readonly ManualLogSource _log;

		public LoggedMonoModder(ManualLogSource log)
		{
			_log = log;
		}

		public override void Log(string value)
		{
			_log.LogInfo(value);
		}

		public override void LogVerbose(string value)
		{
			_log.LogDebug(value);
		}
	}
}
