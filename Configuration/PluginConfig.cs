using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace CUESaber.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }

        public virtual string CUESDKPath { get; set; } = "Libs/Native/CUESDK";

        public virtual long InterpolationTimeMS { get; set; } = 150L;

        public virtual double NoiseDividerMS { get; set; } = 2000D;

        public virtual double NoiseScale { get; set; } = 8D;

        public virtual bool AdaptiveIntepolation { get; set; } = false;

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