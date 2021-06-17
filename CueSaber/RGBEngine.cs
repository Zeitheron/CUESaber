using System;
using System.Diagnostics;
using System.Threading;
using CUESaber.Configuration;
using CUESaber.CueSaber.Wrappers;
using CUESaber.Utils;
using IPA.Utilities;
using UnityEngine;
using static BeatmapSaveData;

namespace CUESaber
{
    public class HarmonyHandler
    {
        public static bool hasSetup, adaptive;
        public static long avgNoteRate;

        private static Stopwatch levelStart;

        public static void OnNoteCut(NoteCutInfo info, NoteData note, NoteController ___instance)
        {
            if(!hasSetup)
            {
                hasSetup = true;
                adaptive = PluginConfig.Instance.AdaptiveIntepolation;
                avgNoteRate = PluginConfig.Instance.InterpolationTimeMS;
                levelStart = Stopwatch.StartNew();
            }

            if(levelStart != null)
            {
                long msSinceLastCut = levelStart.ElapsedMilliseconds;
                levelStart = Stopwatch.StartNew();

                if(adaptive)
                {
                    long msMax = PluginConfig.Instance.AdaptiveInterpolationTimeMSMax;
                    long msMin = PluginConfig.Instance.AdaptiveInterpolationTimeMSMin;
                    long msShift = PluginConfig.Instance.AdaptiveInterpolationTimeMSShift;

                    if (msSinceLastCut - 25L < avgNoteRate && msSinceLastCut > msMin)
                    {
                        long prev = avgNoteRate;

                        avgNoteRate -= msShift;

                        /*clip the value in range specified by configs*/
                        avgNoteRate = Math.Min(msMax, Math.Max(msMin, avgNoteRate));

                        if(avgNoteRate != prev)
                            DebugLogger.debug($"Decreased interpolation rate from {prev} to {avgNoteRate}");
                    }
                    else if (msSinceLastCut > avgNoteRate && msSinceLastCut < msMax /*probably a break between notes or smh*/)
                    {
                        long prev = avgNoteRate;
                        avgNoteRate += msShift;

                        /*clip the value in range specified by configs*/
                        avgNoteRate = Math.Min(msMax, Math.Max(msMin, avgNoteRate));

                        if (avgNoteRate != prev)
                            DebugLogger.debug($"Increased interpolation rate from {prev} to {avgNoteRate}");
                    }
                }
            }

            try
            {
                DebugLogger.debug("Cut " + note.type);
                RGBEngine.OnNoteCut(info, note, ___instance);
            } catch(Exception err)
            {
                Plugin.Log.Error(err);
            }
        }

        public static void OnControllerDestroy()
        {
            hasSetup = false;
            try
            {
                RGBEngine.OnControllerDestroy();
            }
            catch (Exception err)
            {
                Plugin.Log.Error(err);
            }
        }
    }

    internal class RGBEngine
    {
        private static Timer timer;
        private static AutoResetEvent autoEvent;
        private static Stopwatch stopwatch;
        internal static OpenSimplexNoise noise;
        internal static Interpolation interp = ColorHelper.BLACK_INTERP;

        private static GlobalRGBWrapper RGB;

        public static void SetColor(Color color)
        {
            interp = interp.Interpolate(HarmonyHandler.avgNoteRate, color.r, color.g, color.b);
        }

        internal static void OnControllerDestroy()
        {
            SetColor(Color.black);
        }

        internal static void OnNoteCut(NoteCutInfo info, NoteData note, NoteController ___instance)
        {
            ColorNoteVisuals visuals = ___instance.GetComponent<ColorNoteVisuals>();
            if (visuals == null) return;

            ColorManager mgr = FieldAccessor< ColorNoteVisuals, ColorManager>.Get(ref visuals, "_colorManager");

            NoteType noteType = note.type;

            Color cutColor = mgr.ColorForSaberType(info.saberType);

            if(info.allIsOK)
            {
                if (noteType == NoteType.Bomb)
                    SetColor(Color.gray);
                else
                    SetColor(cutColor);
            } else if(noteType != NoteType.None)
            {
                SetColor(noteType == NoteType.Bomb ? Color.gray : Color.black);
            }
        }

        internal static void Start()
        {
            DebugLogger.debug("Starting up RGB Engine.");

            RGB = GlobalRGBWrapper.Create();

            if (RGB.Start())
            {
                stopwatch = Stopwatch.StartNew();

                noise = new OpenSimplexNoise();

                autoEvent = new AutoResetEvent(false);
                timer = new Timer(Tick, autoEvent, 0, 1000 / 90);

                DebugLogger.debug($"RGB Engine started with {RGB.GetWrapperCount()} wrappers.");
            } else
                DebugLogger.debug("RGB Engine unable to start, no supported wrappers found.");
        }

        internal static void Stop()
        {
            DebugLogger.debug("Shutting Down RGB Engine.");

            if (timer != null)
            {
                autoEvent.WaitOne();
                timer.Dispose();
            }

            RGB.Stop();

            timer = null;
            autoEvent = null;
        }

        static double time;

        private static void Tick(object state)
        {
            try
            {
                time = stopwatch.ElapsedMilliseconds / PluginConfig.Instance.NoiseDividerMS;
                RGB.Update(interp, GenNoise);
            }
            catch(Exception err)
            {
                Plugin.Log.Error(err);
            }

            ((AutoResetEvent)state).Set();
        }

        public static float GenNoise(double x, double y)
        {
            float noise = (float)(RGBEngine.noise.Evaluate(x / PluginConfig.Instance.NoiseScale, y / PluginConfig.Instance.NoiseScale, time) + 1) / 2F;
            noise = noise * noise * noise;
            return Math.Min(1F, Math.Max(0F, noise));
        }

        internal static long StopwatchTime()
        {
            return stopwatch.ElapsedMilliseconds;
        }
    }
}