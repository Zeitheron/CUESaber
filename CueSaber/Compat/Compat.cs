using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Loader;
using CueSaber.Compat.Chroma;

namespace CUESaber.CueSaber.Compat
{
    class Compats
    {
        public static void Setup()
        {
            foreach (PluginMetadata m in IPA.Loader.PluginManager.EnabledPlugins)
            {
                if (m.Id != null)
                {
                    switch (m.Id.ToLower())
                    {
                        case "chroma":
                            ChromaCompat.Enable();
                            break;
                    }
                }
            }
        }
    }

    class NoteColorOverrides
    {
        public static List<GetOverrideColor> overrides = new List<GetOverrideColor>();

        public delegate Color GetOverrideColor(NoteController controller);

        public static Color HandleColorOverride(Color baseColor, NoteController controller)
        {
            foreach (GetOverrideColor o in overrides)
            {
                Color c = o(controller);
                if (c != null) return c;
            }
            return baseColor;
        }
    }
}
