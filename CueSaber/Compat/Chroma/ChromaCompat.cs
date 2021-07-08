using Chroma.Colorizer;
using CUESaber;
using CUESaber.CueSaber.Compat;
using UnityEngine;
using static CUESaber.CueSaber.Compat.NoteColorOverrides;

namespace CueSaber.Compat.Chroma
{
    class ChromaCompat
    {
        public static GetOverrideColor overrideColor = GetNoteColor;

        public static void Enable()
        {
            Plugin.Log.Info("Enabling support for Chroma!");
            NoteColorOverrides.overrides.Add(overrideColor);
        }

        public static Color GetNoteColor(NoteControllerBase controllerBase)
        {
            NoteColorizer c = controllerBase.GetNoteColorizer();
            return c.Color;
        }
    }
}