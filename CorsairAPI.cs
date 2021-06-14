using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using CUESaber.Configuration;
using CUESaber.Native;
using CUESaber.Utils;
using IPA.Utilities;
using UnityEngine;
using static BeatmapSaveData;
using IPALogger = IPA.Logging.Logger;

namespace CUESaber
{
    public class CUEHandler
    {
        public static bool hasSetup, adaptive;
        public static long avgNoteRate;

        private static Stopwatch levelStart;
        private static int noteCount;

        public static void OnNoteCut(NoteCutInfo info, NoteData note, NoteController ___instance)
        {
            if(!hasSetup)
            {
                hasSetup = true;
                adaptive = PluginConfig.Instance.AdaptiveIntepolation;
                avgNoteRate = PluginConfig.Instance.InterpolationTimeMS;
                noteCount = 0;
                levelStart = Stopwatch.StartNew();
            }

            noteCount++;

            if (adaptive && levelStart != null && noteCount > 1)
            {
                avgNoteRate = levelStart.ElapsedMilliseconds / noteCount;
            }

            try
            { 
                CorsairAPI.OnNoteCut(info, note, ___instance);
            } catch(Exception err)
            {
                CorsairAPI.log.Error(err);
            }
        }

        public static void OnControllerDestroy()
        {
            noteCount = 0;
            hasSetup = false;
            try
            {
                CorsairAPI.OnControllerDestroy();
            }
            catch (Exception err)
            {
                CorsairAPI.log.Error(err);
            }
        }
    }

    internal class Led
    {
        readonly double x, y;

        internal CorsairLedColor color;

        public Led(CorsairLedPosition pos)
        {
            x = pos.left;
            y = pos.top;
            color = new CorsairLedColor();
            color.ledId = (int)pos.ledId;
        }

        public void ApplyNoise(Interpolation interp, double time)
        {
            float noise = (float) (CorsairAPI.noise.Evaluate(x / PluginConfig.Instance.NoiseScale, y / PluginConfig.Instance.NoiseScale, time) + 1) / 2F;

            noise = noise * noise * noise;

            this.color.r = Mathf.RoundToInt(interp.red * noise);
            this.color.g = Mathf.RoundToInt(interp.green * noise);
            this.color.b = Mathf.RoundToInt(interp.blue * noise);

            CorsairAPI.allLedsCorsair.Add(this.color);
        }
    }

    internal class CorsairAPI
    {
        private static Timer timer;
        private static AutoResetEvent autoEvent;

        private static int deviceCount;

        internal static List<CorsairLedColor> allLedsCorsair = new List<CorsairLedColor>();
        private static List<Led> allLeds = new List<Led>();

        private static Stopwatch stopwatch;

        internal static OpenSimplexNoise noise;

        internal static IPALogger log;

        internal static Interpolation interp = ColorHelper.BLACK_INTERP;

        public static void SetColor(Color color)
        {
            interp = interp.Interpolate(CUEHandler.avgNoteRate, color.r * 255, color.g * 255, color.b * 255);
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
            if(CUESDK.IsLoaded())
            {
                stopwatch = Stopwatch.StartNew();

                noise = new OpenSimplexNoise();

                autoEvent = new AutoResetEvent(false);
                timer = new Timer(Tick, autoEvent, 0, 1000 / 90);

                log.Info("CUE SDK Loaded!");

                CorsairProtocolDetails protocol = CUESDK.CorsairPerformProtocolHandshake();

                log.Info("Performed handshake with iCUE: " + protocol);

                if (!protocol.breakingChanges)
                {
                    if (CUESDK.CorsairRequestControl(CorsairAccessMode.ExclusiveLightingControl))
                    {
                        log.Info("CUE exclusive light control provided!");
                    }
                    else
                    {
                        CorsairError err = CUESDK.CorsairGetLastError();

                        log.Error($"CUE exclusive light control was NOT provided, shutting down SDK! Error: {err}");

                        CUESDK.Shutdown();
                    }
                }
                else log.Error("CUE seems to have breaking changes, SDK will not be enabled!");
            }
        }

        internal static void Stop()
        {
            log.Info("Shutting Down CUE SDK.");
            if (timer != null)
            {
                autoEvent.WaitOne();
                timer.Dispose();
            }
            CUESDK.Shutdown();
            timer = null;
            autoEvent = null;
        }

        private static void UpdateGrid()
        {
            double second = stopwatch.ElapsedMilliseconds / PluginConfig.Instance.NoiseDividerMS;
            foreach (Led l in allLeds)
                l.ApplyNoise(interp, second);
        }

        private static void RefreshDevices()
        {
            allLeds.Clear();
            for (int i = 0; i < deviceCount; ++i)
            {
                CorsairLedPosition[] positions = CorsairLedPositions.FromPtr(CUESDK.CorsairGetLedPositionsByDeviceIndex(i)).GetPositions();
                foreach(CorsairLedPosition pos in positions)
                {
                    Led led = new Led(pos);
                    allLeds.Add(led);
                }
            }
        }

        private static void Tick(object state)
        {
            try
            {
                int devices = CUESDK.CorsairGetDeviceCount();

                if (deviceCount != devices)
                {
                    deviceCount = devices;
                    RefreshDevices();
                }

                UpdateGrid();

                CUESDK.CorsairSetLedsColors(allLedsCorsair.Count, CUESDK.StructArray2MarshalUnmanagedArray(allLedsCorsair));
                allLedsCorsair.Clear();
            }
            catch(Exception err)
            {
                log.Error(err);
            }

            ((AutoResetEvent)state).Set();
        }

        internal static long StopwatchTime()
        {
            return stopwatch.ElapsedMilliseconds;
        }
    }
}