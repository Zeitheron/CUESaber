using CUESaber.CueSaber.Native.Logitech;
using CUESaber.Native.Logitech;
using CUESaber.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace CUESaber.CueSaber.Wrappers
{
    internal class LogiLedKeyboard : IRGBZone
    {
        readonly double x, y;

        internal int idx;

        public LogiLedKeyboard(int idx)
        {
            this.x = idx % 21;
            this.y = idx / 21;
            this.idx = idx;
        }

        public void ApplyNoise(Utils.Interpolation currentInterpolation, RGBMethods.GetNoiseMult noise)
        {
            float mul = RGBEngine.RearrangeFV(noise.Invoke(x, y), 0.35F, 1F);
            SetRGB(currentInterpolation.red * mul, currentInterpolation.green * mul, currentInterpolation.blue * mul);
        }

        public void SetRGB(float red, float green, float blue)
        {
            LogitechWrapper.keyboard.SetColor(idx, red, green, blue);
        }
        public void SetRGB(Color color) => SetRGB(color.r, color.g, color.b);
    }

    internal class LogiLedSingleLight : IRGBZone
    {
        readonly double x, y;

        internal int idx;

        public LogiLedSingleLight(int idx)
        {
            this.x = idx % 21;
            this.y = idx / 21;
            this.idx = idx;
        }

        public void ApplyNoise(Utils.Interpolation currentInterpolation, RGBMethods.GetNoiseMult noise)
        {
            float mul = RGBEngine.RearrangeFV(noise.Invoke(x, y), 0.35F, 1F);
            SetRGB(currentInterpolation.red * mul, currentInterpolation.green * mul, currentInterpolation.blue * mul);
        }

        public void SetRGB(float red, float green, float blue)
        {
            LogitechWrapper.light.SetColor(red, green, blue);
        }
        public void SetRGB(Color color) => SetRGB(color.r, color.g, color.b);
    }

    class LogitechWrapper : IRGBManufacturer
    {
        internal static LogiKeyboard keyboard = new LogiKeyboard();
        internal static LogiSingleLight light = new LogiSingleLight();

        private List<IRGBZone> allLeds = new List<IRGBZone>();

        public bool Start()
        {
            if (LogitechGSDK.LogiLedInitWithName(Plugin.PLUGIN_NAME))
            {
                Plugin.Log.Info("LogitechG SDK connected successfully.");

                LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_ALL);
                LogitechGSDK.LogiLedSetLighting(0, 0, 0);

                for (int i = 0; i < 126; ++i)
                    allLeds.Add(new LogiLedKeyboard(i));
                allLeds.Add(new LogiLedSingleLight(126));

                return true;
            } else Plugin.Log.Error("LogitechG seems to be missing or the SDK support is disabled.");

            return false;
        }

        public void Stop()
        {
            Plugin.Log.Info("Shutting Down LogitechG SDK.");
            LogitechGSDK.LogiLedShutdown();
            allLeds.Clear();
        }

        public void Update(Utils.Interpolation currentInterpolation, RGBMethods.GetNoiseMult noise)
        {
            foreach (var l in allLeds)
            {
                l.ApplyNoise(currentInterpolation, noise);
            }

            keyboard.Apply();
            light.Apply();
        }
    }
}