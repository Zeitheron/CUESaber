using CUESaber.Native.Corsair;
using CUESaber.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CUESaber.CueSaber.Wrappers
{
    internal class Led : IRGBZone
    {
        readonly double x, y;

        internal CorsairLedColor color;

        public Led(CorsairLedPosition pos)
        {
            x = pos.left;
            y = pos.top;

            this.color = new CorsairLedColor();
            this.color.ledId = (int)pos.ledId;
        }

        public void ApplyNoise(Interpolation currentInterpolation, RGBMethods.GetNoiseMult noise)
        {
            float mul = noise.Invoke(x, y);
            SetRGB(currentInterpolation.red * mul, currentInterpolation.green * mul, currentInterpolation.blue * mul);
        }

        public void SetRGB(int red, int green, int blue)
        {
            this.color.r = red;
            this.color.g = green;
            this.color.b = blue;
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
        private List<Led> allLeds = new List<Led>();
        private int deviceCount;

        private void RefreshDevices()
        {
            allLeds.Clear();
            allLedsCorsair.Clear();
            for (int i = 0; i < deviceCount; ++i)
            {
                CorsairLedPosition[] positions = CorsairLedPositions.FromPtr(CUESDK.CorsairGetLedPositionsByDeviceIndex(i)).GetPositions();
                foreach (CorsairLedPosition pos in positions)
                {
                    Led led = new Led(pos);
                    allLeds.Add(led);
                    allLedsCorsair.Add(led.color);
                }
            }
        }

        public bool Start()
        {
            if (CUESDK.IsLoaded())
            {
                DebugLogger.debug("iCUE SDK Loaded!");

                CorsairProtocolDetails protocol = CUESDK.CorsairPerformProtocolHandshake();

                DebugLogger.debug("Performed handshake with iCUE: " + protocol);

                if (!protocol.breakingChanges)
                {
                    if (CUESDK.CorsairRequestControl(CorsairAccessMode.ExclusiveLightingControl))
                    {
                        DebugLogger.debug("iCUE exclusive light control provided!");
                        return true;
                    }
                    else
                    {
                        CorsairError err = CUESDK.CorsairGetLastError();

                        DebugLogger.debug($"iCUE exclusive light control was NOT provided, shutting down SDK! Error: {err}");

                        CUESDK.Shutdown();
                    }
                }
                else DebugLogger.debug("iCUE seems to have breaking changes, SDK will not be enabled!");
            }
            return false;
        }

        public void Update(Interpolation currentInterpolation, RGBMethods.GetNoiseMult noise)
        {
            int devices = CUESDK.CorsairGetDeviceCount();

            if (deviceCount != devices)
            {
                deviceCount = devices;
                RefreshDevices();
                DebugLogger.debug("[iCUE] Total led count refreshed to: " + allLeds.Count);
            }

            foreach (Led l in allLeds)
            {
                l.ApplyNoise(currentInterpolation, noise);
            }

            if (allLedsCorsair.Count > 0)
                CUESDK.CorsairSetLedsColors(allLedsCorsair.Count, NativeMemoryUtils.StructArray2MarshalUnmanagedArray(allLedsCorsair));
        }

        public void Stop()
        {
            DebugLogger.debug("Shutting Down iCUE SDK.");
            CUESDK.Shutdown();
        }
    }
}
