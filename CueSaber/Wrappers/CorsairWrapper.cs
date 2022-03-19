using CUESaber.Native.Corsair;
using CUESaber.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace CUESaber.CueSaber.Wrappers
{
    internal class CUELed : IRGBZone
    {
        readonly double x, y;

        internal CorsairLedColor color;

        public CUELed(CorsairLedPosition pos)
        {
            x = pos.left;
            y = pos.top;

            this.color = new CorsairLedColor();
            this.color.ledId = (int) pos.ledId;
        }

        public void ApplyNoise(Utils.Interpolation currentInterpolation, RGBMethods.GetNoiseMult noise)
        {
            float mul = noise.Invoke(x, y);
            SetRGB(currentInterpolation.red * mul, currentInterpolation.green * mul, currentInterpolation.blue * mul);
        }

        public void SetRGB(float red, float green, float blue)
        {
            this.color.r = Mathf.RoundToInt(red * 255F);
            this.color.g = Mathf.RoundToInt(green * 255F);
            this.color.b = Mathf.RoundToInt(blue * 255F);
        }
        public void SetRGB(Color color) => SetRGB(color.r, color.g, color.b);
    }

    class CorsairWrapper : IRGBManufacturer
    {
        private List<CorsairLedColor> allLedsCorsair = new List<CorsairLedColor>();
        private List<CUELed> allLeds = new List<CUELed>();
        private int deviceCount;

        private void RefreshDevices()
        {
            allLeds.Clear();
            allLedsCorsair.Clear();
            for (int i = 0; i < deviceCount; ++i)
            {
                var positions = CorsairLedPositions.FromPtr(CUESDK.CorsairGetLedPositionsByDeviceIndex(i)).GetPositions();
                foreach (var pos in positions)
                {
                    var led = new CUELed(pos);
                    allLeds.Add(led);
                    allLedsCorsair.Add(led.color);
                }
            }
        }

        public bool Start()
        {
            if (CUESDK.IsLoaded())
            {
                Plugin.Log.Info("iCUE SDK Loaded!");

                var protocol = CUESDK.CorsairPerformProtocolHandshake();

                Plugin.Log.Info("Performed handshake with iCUE: " + protocol);

                if (!protocol.breakingChanges)
                {
                    if (CUESDK.CorsairRequestControl(CorsairAccessMode.ExclusiveLightingControl))
                    {
                        Plugin.Log.Info("iCUE exclusive light control provided!");
                        return true;
                    }
                    else
                    {
                        var err = CUESDK.CorsairGetLastError();

                        Plugin.Log.Error($"iCUE exclusive light control was NOT provided, shutting down SDK! Error: {err}");

                        CUESDK.Shutdown();
                    }
                } else Plugin.Log.Error("iCUE seems to have breaking changes, SDK will not be enabled!");
            } else Plugin.Log.Error("iCUE seems to be missing or the SDK support is disabled.");
            return false;
        }

        public void Update(Utils.Interpolation currentInterpolation, RGBMethods.GetNoiseMult noise)
        {
            int devices = CUESDK.CorsairGetDeviceCount();

            if (deviceCount != devices)
            {
                deviceCount = devices;
                RefreshDevices();
                Plugin.Log.Info("[iCUE] Total led count refreshed to: " + allLeds.Count);
            }

            foreach (var l in allLeds)
            {
                l.ApplyNoise(currentInterpolation, noise);
            }

            if (allLedsCorsair.Count > 0)
                CUESDK.CorsairSetLedsColors(allLedsCorsair.Count, NativeMemoryUtils.StructArray2MarshalUnmanagedArray(allLedsCorsair));
        }

        public void Stop()
        {
            Plugin.Log.Info("Shutting Down iCUE SDK.");
            CUESDK.Shutdown();
            allLeds.Clear();
            allLedsCorsair.Clear();
        }
    }
}
