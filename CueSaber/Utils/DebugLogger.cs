using CUESaber.Configuration;

namespace CUESaber.Utils
{
    class DebugLogger
    {
        public static void debug(string message)
        {
            if(PluginConfig.Instance.DebugLogging)
                Plugin.Log.Info($"[DEBUG] {message}");
        }
    }
}