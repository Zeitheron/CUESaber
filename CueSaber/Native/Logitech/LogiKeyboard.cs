using CUESaber.Native.Logitech;

namespace CUESaber.CueSaber.Native.Logitech
{
    class LogiKeyboard
    {
        internal static readonly Rectangle boundaries = new Rectangle(0, 0, 21, 6);

        private readonly byte[] colors = new byte[504];

        public void SetColor(int i, float red, float green, float blue)
        {
            colors[i * 4 + 3] = byte.MaxValue; // a
            colors[i * 4 + 2] = (byte)(red * 255f); // r
            colors[i * 4 + 1] = (byte)(green * 255f); // g
            colors[i * 4] = (byte)(blue * 255f); // b
        }

        public void Apply()
        {
            if (LogitechGSDK.LogiLedSetTargetDevice(4))
            {
                LogitechGSDK.LogiLedSetLightingFromBitmap(colors);
            }
        }
    }
    class LogiSingleLight
    {
        private int rp, gp, bp;

        public void SetColor(float red, float green, float blue)
        {
            rp = (int)(red * 100f);
            gp = (int)(green * 100f);
            bp = (int)(blue * 100f);
        }

        public void Apply()
        {
            if (LogitechGSDK.LogiLedSetTargetDevice(2))
            {
                LogitechGSDK.LogiLedSetLighting(rp, gp, bp);
            }
        }
    }
}
