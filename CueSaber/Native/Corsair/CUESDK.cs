using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CUESaber.Native.Corsair
{
    internal static class CUESDK
    {
        #region Libary Management

        private static IntPtr _dllHandle = IntPtr.Zero;
        private static bool isLoaded = false;

        /// <summary>
        /// Reloads the SDK.
        /// </summary>
        /// 
        internal static bool IsLoaded()
        {
            return isLoaded;
        }

        internal static void Reload()
        {
            UnloadSDK();
            LoadSDK();
        }

        internal static void Shutdown()
        {
            UnloadSDK();
        }

        private static void LoadSDK()
        {
            if (_dllHandle != IntPtr.Zero) return;

            string target = Configuration.PluginConfig.Instance.CUESDKPath + "-x" + (Environment.Is64BitProcess ? "64" : "86") + ".dll";

            // HACK: Load library at runtime to support both, x86 and x64 with one managed dll
            string dllPath = null;
            if (File.Exists(target))
                dllPath = target;

            if (dllPath == null)
            {
                Plugin.Log.Error($"Can't find the CUE-SDK at the expected location: '{Path.GetFullPath(target)}'");
                return;
            }

            Plugin.Log.Info($"Loading CUE-SDK from '{Path.GetFullPath(target)}'");

            _dllHandle = LoadLibrary(dllPath);

            _corsairSetLedsColorsPointer = (CorsairSetLedsColorsPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "CorsairSetLedsColors"), typeof(CorsairSetLedsColorsPointer));
            _corsairGetLedsColorsPointer = (CorsairGetLedsColorsPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "CorsairGetLedsColors"), typeof(CorsairGetLedsColorsPointer));
            _corsairGetDeviceCountPointer = (CorsairGetDeviceCountPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "CorsairGetDeviceCount"), typeof(CorsairGetDeviceCountPointer));
            _corsairGetDeviceInfoPointer = (CorsairGetDeviceInfoPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "CorsairGetDeviceInfo"), typeof(CorsairGetDeviceInfoPointer));
            _corsairGetLedPositionsPointer = (CorsairGetLedPositionsPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "CorsairGetLedPositions"), typeof(CorsairGetLedPositionsPointer));
            _corsairGetLedPositionsByDeviceIndexPointer = (CorsairGetLedPositionsByDeviceIndexPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "CorsairGetLedPositionsByDeviceIndex"), typeof(CorsairGetLedPositionsByDeviceIndexPointer));
            _corsairGetLedIdForKeyNamePointer = (CorsairGetLedIdForKeyNamePointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "CorsairGetLedIdForKeyName"), typeof(CorsairGetLedIdForKeyNamePointer));
            _corsairRequestControlPointer = (CorsairRequestControlPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "CorsairRequestControl"), typeof(CorsairRequestControlPointer));
            _corsairReleaseControlPointer = (CorsairReleaseControlPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "CorsairReleaseControl"), typeof(CorsairReleaseControlPointer));
            _corsairPerformProtocolHandshakePointer = (CorsairPerformProtocolHandshakePointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "CorsairPerformProtocolHandshake"), typeof(CorsairPerformProtocolHandshakePointer));
            _corsairGetLastErrorPointer = (CorsairGetLastErrorPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "CorsairGetLastError"), typeof(CorsairGetLastErrorPointer));
            _corsairRegisterKeypressCallbackPointer = (CorsairRegisterKeypressCallbackPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "CorsairRegisterKeypressCallback"), typeof(CorsairRegisterKeypressCallbackPointer));

            isLoaded = true;
        }

        private static void UnloadSDK()
        {
            isLoaded = false;
            if (_dllHandle == IntPtr.Zero) return;

            // ReSharper disable once EmptyEmbeddedStatement - DarthAffe 20.02.2016: We might need to reduce the internal reference counter more than once to set the library free
            while (FreeLibrary(_dllHandle)) ;
            _dllHandle = IntPtr.Zero;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr dllHandle);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr dllHandle, string name);

        #endregion

        #region SDK-METHODS

        #region Pointers

        private static CorsairSetLedsColorsPointer _corsairSetLedsColorsPointer;
        private static CorsairGetLedsColorsPointer _corsairGetLedsColorsPointer;
        private static CorsairGetDeviceCountPointer _corsairGetDeviceCountPointer;
        private static CorsairGetDeviceInfoPointer _corsairGetDeviceInfoPointer;
        private static CorsairGetLedPositionsPointer _corsairGetLedPositionsPointer;
        private static CorsairGetLedIdForKeyNamePointer _corsairGetLedIdForKeyNamePointer;
        private static CorsairGetLedPositionsByDeviceIndexPointer _corsairGetLedPositionsByDeviceIndexPointer;
        private static CorsairRequestControlPointer _corsairRequestControlPointer;
        private static CorsairReleaseControlPointer _corsairReleaseControlPointer;
        private static CorsairPerformProtocolHandshakePointer _corsairPerformProtocolHandshakePointer;
        private static CorsairGetLastErrorPointer _corsairGetLastErrorPointer;
        private static CorsairRegisterKeypressCallbackPointer _corsairRegisterKeypressCallbackPointer;

        #endregion

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool CorsairSetLedsColorsPointer(int size, IntPtr ledsColors);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool CorsairGetLedsColorsPointer(int size, IntPtr ledsColors);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int CorsairGetDeviceCountPointer();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CorsairGetDeviceInfoPointer(int deviceIndex);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CorsairGetLedPositionsPointer();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CorsairGetLedPositionsByDeviceIndexPointer(int deviceIndex);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CorsairLedId CorsairGetLedIdForKeyNamePointer(char keyName);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool CorsairRequestControlPointer(CorsairAccessMode accessMode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool CorsairReleaseControlPointer(CorsairAccessMode accessMode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CorsairProtocolDetails CorsairPerformProtocolHandshakePointer();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CorsairError CorsairGetLastErrorPointer();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool CorsairRegisterKeypressCallbackPointer(IntPtr callback, IntPtr context);

        #endregion

        // ReSharper disable EventExceptionNotDocumented

        /// <summary>
        /// CUE-SDK: set specified leds to some colors. The color is retained until changed by successive calls. This function does not take logical layout into account.
        /// </summary>
        internal static bool CorsairSetLedsColors(int size, IntPtr ledsColors)
        {
            return _corsairSetLedsColorsPointer(size, ledsColors);
        }

        /// <summary>
        /// CUE-SDK: get current color for the list of requested LEDs.
        /// </summary>
        internal static bool CorsairGetLedsColors(int size, IntPtr ledsColors)
        {
            return _corsairGetLedsColorsPointer(size, ledsColors);
        }

        /// <summary>
        /// CUE-SDK: returns number of connected Corsair devices that support lighting control.
        /// </summary>
        internal static int CorsairGetDeviceCount()
        {
            return _corsairGetDeviceCountPointer();
        }

        /// <summary>
        /// CUE-SDK: returns information about device at provided index.
        /// </summary>
        internal static IntPtr CorsairGetDeviceInfo(int deviceIndex)
        {
            return _corsairGetDeviceInfoPointer(deviceIndex);
        }

        /// <summary>
        /// CUE-SDK: provides list of keyboard LEDs with their physical positions.
        /// </summary>
        internal static IntPtr CorsairGetLedPositions()
        {
            return _corsairGetLedPositionsPointer();
        }

        /// <summary>
        /// CUE-SDK: provides list of keyboard or mousemat LEDs with their physical positions.
        /// </summary>
        internal static IntPtr CorsairGetLedPositionsByDeviceIndex(int deviceIndex)
        {
            return _corsairGetLedPositionsByDeviceIndexPointer(deviceIndex);
        }

        /// <summary>
        /// CUE-SDK: retrieves led id for key name taking logical layout into account.
        /// </summary>
        internal static CorsairLedId CorsairGetLedIdForKeyName(char keyName)
        {
            return _corsairGetLedIdForKeyNamePointer(keyName);
        }

        /// <summary>
        /// CUE-SDK: requestes control using specified access mode.
        /// By default client has shared control over lighting so there is no need to call CorsairRequestControl unless client requires exclusive control.
        /// </summary>
        internal static bool CorsairRequestControl(CorsairAccessMode accessMode)
        {
            return _corsairRequestControlPointer(accessMode);
        }

        /// <summary>
        /// CUE-SDK: releases previously requested control for specified access mode.
        /// </summary>
        internal static bool CorsairReleaseControl(CorsairAccessMode accessMode)
        {
            return _corsairReleaseControlPointer(accessMode);
        }

        /// <summary>
        /// CUE-SDK: checks file and protocol version of CUE to understand which of SDK functions can be used with this version of CUE.
        /// </summary>
        internal static CorsairProtocolDetails CorsairPerformProtocolHandshake()
        {
            return _corsairPerformProtocolHandshakePointer();
        }

        /// <summary>
        /// CUE-SDK: returns last error that occured while using any of Corsair* functions.
        /// </summary>
        internal static CorsairError CorsairGetLastError()
        {
            return _corsairGetLastErrorPointer();
        }

        internal static bool CorsairRegisterKeypressCallback(IntPtr callback, IntPtr context)
        {
            return _corsairRegisterKeypressCallbackPointer(callback, context);
        }

        // ReSharper restore EventExceptionNotDocumented

        #endregion
    }
}
