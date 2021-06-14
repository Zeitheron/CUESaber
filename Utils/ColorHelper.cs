using System;

namespace CUESaber.Utils
{
    public struct Interpolation
    {
        public readonly long startMS, lengthMS;
        public readonly float srcR, srcG, srcB;
        public readonly float dstR, dstG, dstB;

        public float interpolation { get { return GetInterpolation(ColorHelper.time); } }
        public float red { get { return srcR + (dstR - srcR) * interpolation; } }
        public float green { get { return srcG + (dstG - srcG) * interpolation; } }
        public float blue { get { return srcB + (dstB - srcB) * interpolation; } }

        public Interpolation(long startMS, long lengthMS, float srcR, float srcG, float srcB, float dstR, float dstG, float dstB)
        {
            this.startMS = startMS;
            this.lengthMS = lengthMS;
            this.srcR = srcR;
            this.srcG = srcG;
            this.srcB = srcB;
            this.dstR = dstR;
            this.dstG = dstG;
            this.dstB = dstB;
        }

        public Interpolation(long startMS, long lengthMS, Interpolation src, float dstR, float dstG, float dstB)
        {
            this.startMS = startMS;
            this.lengthMS = lengthMS;
            this.srcR = src.GetRed(startMS);
            this.srcG = src.GetGreen(startMS);
            this.srcB = src.GetBlue(startMS);
            this.dstR = dstR;
            this.dstG = dstG;
            this.dstB = dstB;
        }

        private float GetRed(long time)
        {
            return srcR + (dstR - srcR) * GetInterpolation(time);
        }

        private float GetGreen(long time)
        {
            return srcG + (dstG - srcG) * GetInterpolation(time);
        }

        private float GetBlue(long time)
        {
            return srcB + (dstB - srcB) * GetInterpolation(time);
        }

        private float GetInterpolation(long time)
        {
            return Math.Min(1, Math.Max(0, (time - startMS) / (float)lengthMS));
        }

        public Interpolation Interpolate(long lengthMS, float dstR, float dstG, float dstB)
        {
            return new Interpolation(ColorHelper.time, lengthMS, this, dstR, dstG, dstB);
        }
    }

    public class ColorHelper
    {
        public static long time { get { return CorsairAPI.StopwatchTime(); } }

        public static Interpolation BLACK_INTERP = new Interpolation(0, 0, 0F, 0F, 0F, 0F, 0F, 0F);
    }
}
