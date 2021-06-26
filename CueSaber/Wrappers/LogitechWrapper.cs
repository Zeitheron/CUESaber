using CUESaber.Native.Logitech;
using CUESaber.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUESaber.CueSaber.Wrappers
{
    class LogitechWrapper : IRGBManufacturer
    {
        public bool Start()
        {
            if (LogitechGSDK.LogiLedInitWithName(Plugin.PLUGIN_NAME))
            {

            }
            return false;
        }

        public void Stop()
        {
            LogitechGSDK.LogiLedShutdown();
        }

        public void Update(Interpolation currentInterpolation, RGBMethods.GetNoiseMult noise)
        {
        }
    }
}