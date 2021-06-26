using CUESaber.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace CUESaber.CueSaber.Wrappers
{
    public class GlobalRGBWrapper : IRGBManufacturer
    {
        private readonly List<IRGBManufacturer> children;

        public GlobalRGBWrapper(IEnumerable<IRGBManufacturer> children)
        {
            this.children = new List<IRGBManufacturer>();
            this.children.AddRange(children);
        }

        public static GlobalRGBWrapper Create()
        {
            List<IRGBManufacturer> c = new List<IRGBManufacturer>();

            c.Add(new CorsairWrapper()); // iCUE (Corsair)
            // c.Add(new ASUSWrapper()); // Aura Sync (ASUS)

            return new GlobalRGBWrapper(c);
        }

        public bool Start()
        {
            this.children.RemoveAll(m => !m.Start());
            return this.children.Count > 0;
        }

        public void Stop()
        {
            this.children.ForEach(m => m.Stop());
        }

        public void Update(Interpolation currentInterpolation, RGBMethods.GetNoiseMult noise)
        {
            this.children.ForEach(m => m.Update(currentInterpolation, noise));
        }

        public int GetWrapperCount()
        {
            return this.children.Count;
        }
    }

    public interface IRGBZone
    {
        void SetRGB(int red, int green, int blue);
        void SetRGB(float red, float green, float blue);
        void SetRGB(Color color);
        void ApplyNoise(Interpolation currentInterpolation, RGBMethods.GetNoiseMult noise);
    }

    public interface IRGBManufacturer
    {
        bool Start();

        void Update(Interpolation currentInterpolation, RGBMethods.GetNoiseMult noise);

        void Stop();
    }

    public class RGBMethods
    {
        public delegate float GetNoiseMult(double x, double y);
    }
}