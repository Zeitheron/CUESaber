using AuraServiceLib;
using CUESaber.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CUESaber.CueSaber.Wrappers
{
    class ASUSWrapper : IRGBManufacturer
    {
        private static IAuraSdk2 sdk;

        public bool Start()
        {
            if (sdk == null)
            {
                try
                {
                    // TODO:
                    // HELP with this
                    // System.PlatformNotSupportedException: Operation is not supported on this platform.
                    // Invalid dll?
                    // I have an ASUS Crosshair VIII Hero
                    sdk = (IAuraSdk2)new AuraSdk();
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(e);
                    return false;
                }
            }

            sdk.SwitchMode();

            return true;
        }

        public void Stop()
        {
            sdk.ReleaseControl(0);
        }

        public void Update(Interpolation currentInterpolation, RGBMethods.GetNoiseMult noise)
        {
            IAuraSyncDeviceCollection devices = sdk.Enumerate(0);

            int x = 0;
            int y = 0;

            foreach (IAuraSyncDevice dev in devices)
            {
                foreach (IAuraRgbLight light in dev.Lights)
                {
                    float mul = noise.Invoke(x, y);

                    float red = currentInterpolation.red * mul * 255F;
                    float green = currentInterpolation.green * mul * 255F;
                    float blue = currentInterpolation.blue * mul * 255F;

                    light.Red = (byte) Mathf.RoundToInt(red);
                    light.Green = (byte) Mathf.RoundToInt(green);
                    light.Blue = (byte) Mathf.RoundToInt(blue);

                    ++x;
                }
                dev.Apply();
                ++y;
            }
        }
    }
}