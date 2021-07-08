using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace CUESaber.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }

        public virtual string CUESDKPath { get; set; } = "Libs/Native/CUESDK";
        public virtual string LogitechGPath { get; set; } = "Libs/Native/LogitechLedEnginesWrapper";

        public virtual long InterpolationTimeMS { get; set; } = 150L;

        public virtual long AdaptiveInterpolationTimeMSMin { get; set; } = 50L;
        public virtual long AdaptiveInterpolationTimeMSShift { get; set; } = 20L;
        public virtual long AdaptiveInterpolationTimeMSMax { get; set; } = 500L;

        public virtual double NoiseDividerMS { get; set; } = 2000D;

        public virtual double NoiseScale { get; set; } = 8D;
        public virtual double NoisePower { get; set; } = 2.5D;

        public virtual bool AdaptiveIntepolation { get; set; } = false;

        public virtual bool DebugLogging { get; set; } = false;

        public virtual void OnReload()
        {
            // Do stuff after config is read from disk.
        }

        public virtual void Changed()
        {
            // Do stuff when the config is changed.
        }

        public virtual void CopyFrom(PluginConfig other)
        {
            // This instance's members populated from other
        }
    }
}