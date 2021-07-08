using System;
using System.Diagnostics;
using System.Threading;
using CUESaber.Configuration;
using CUESaber.CueSaber.Compat;
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

        private static NoteType[] lastCut = new NoteType[2];
        private static long[] cutMs = new long[2];

        public static void OnNoteCut(NoteCutInfo info, BeatmapSaveData.NoteData note, NoteController ___instance)
        {
            if (!hasSetup)
            {
                hasSetup = true;
                adaptive = PluginConfig.Instance.AdaptiveIntepolation;
                avgNoteRate = PluginConfig.Instance.InterpolationTimeMS;
                levelStart = Stopwatch.StartNew();
                Array.Fill(lastCut, NoteType.None);
                Array.Fill(cutMs, 0L);
            }

            NoteType nType = note.type;

            long msSinceLastCut = -1L;

            if (levelStart != null)
            {
                msSinceLastCut = levelStart.ElapsedMilliseconds;
                levelStart = Stopwatch.StartNew();

                if (msSinceLastCut < 25L)
                {
                    bool a = note.type == NoteType.NoteA;
                    bool b = note.type == NoteType.NoteB;
                    if (lastCut[0] != note.type && lastCut[1] == note.type && (a || b))
                    // Make the opposite since we cut it just previosly
                    {
                        nType = lastCut[0];
                        msSinceLastCut = cutMs[0];
                    }
                }

                insertNote(note.type, msSinceLastCut);

                if (adaptive)
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
                DebugLogger.debug("Cut " + note.type + ", took about " + msSinceLastCut + " ms.");
                RGBEngine.OnNoteCut(info, nType, ___instance);
            } catch(Exception err)
            {
                Plugin.Log.Error(err);
            }
        }

        public static void OnControllerDestroy()
        {
            hasSetup = false;
            Array.Fill(lastCut, NoteType.None);
            Array.Fill(cutMs, 0L);
            try
            {
                RGBEngine.OnControllerDestroy();
            }
            catch (Exception err)
            {
                Plugin.Log.Error(err);
            }
        }

        private static void insertNote(NoteType type, long msSinceHit)
        {
            cutMs[1] = cutMs[0];
            cutMs[0] = msSinceHit;

            lastCut[1] = lastCut[0];
            lastCut[0] = type;
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

        internal static void OnNoteCut(NoteCutInfo info, NoteType noteType, NoteController ___instance)
        {
            ColorNoteVisuals visuals = ___instance.GetComponent<ColorNoteVisuals>();
            if (visuals == null) return;

            ColorManager mgr = FieldAccessor< ColorNoteVisuals, ColorManager>.Get(ref visuals, "_colorManager");

            Color cutColor = mgr.ColorForSaberType(info.saberType);
            cutColor = NoteColorOverrides.HandleColorOverride(cutColor, ___instance);

            if (info.allIsOK)
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
                noisePower = PluginConfig.Instance.NoisePower;
                RGB.Update(interp, GenNoise);
            }
            catch(Exception err)
            {
                Plugin.Log.Error(err);
            }

            ((AutoResetEvent)state).Set();
        }

        public static double noisePower;

        public static float GenNoise(double x, double y)
        {
            float noise = (float)(RGBEngine.noise.Evaluate(x / PluginConfig.Instance.NoiseScale, y / PluginConfig.Instance.NoiseScale, time) + 1) / 2F;
            noise = (float) Math.Pow(noise, noisePower);
            return Math.Min(1F, Math.Max(0F, noise));
        }

        internal static long StopwatchTime()
        {
            return stopwatch.ElapsedMilliseconds;
        }
    }
}