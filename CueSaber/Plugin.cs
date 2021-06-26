using IPA;
using IPA.Config;
using IPA.Config.Stores;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using HarmonyLib;
using System.Reflection;
using CUESaber.Native.Corsair;

namespace CUESaber
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static readonly string PLUGIN_NAME = "CUE Saber";

        internal static Harmony harmony;

        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger)
        {
            Instance = this;
            Log = logger;
            Log.Info(PLUGIN_NAME + " initialized.");
        }

        #region BSIPA Config
        //Uncomment to use BSIPA's config
        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            CUESDK.Reload();
            Log.Debug("Config loaded");
            RGBEngine.Start();
        }
        #endregion

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");
            harmony = new Harmony("org.zeith.BeatSaber.CUESaber");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            RGBEngine.Stop();
            Log.Debug("OnApplicationQuit");
        }
    }
}
