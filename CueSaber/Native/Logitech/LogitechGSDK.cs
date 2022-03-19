using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CUESaber.Native.Logitech
{
    public enum KeyName
    {
        ESC = 0x01,
        F1 = 0x3b,
        F2 = 0x3c,
        F3 = 0x3d,
        F4 = 0x3e,
        F5 = 0x3f,
        F6 = 0x40,
        F7 = 0x41,
        F8 = 0x42,
        F9 = 0x43,
        F10 = 0x44,
        F11 = 0x57,
        F12 = 0x58,
        PRINT_SCREEN = 0x137,
        SCROLL_LOCK = 0x46,
        PAUSE_BREAK = 0x145,
        TILDE = 0x29,
        ONE = 0x02,
        TWO = 0x03,
        THREE = 0x04,
        FOUR = 0x05,
        FIVE = 0x06,
        SIX = 0x07,
        SEVEN = 0x08,
        EIGHT = 0x09,
        NINE = 0x0A,
        ZERO = 0x0B,
        MINUS = 0x0C,
        EQUALS = 0x0D,
        BACKSPACE = 0x0E,
        INSERT = 0x152,
        HOME = 0x147,
        PAGE_UP = 0x149,
        NUM_LOCK = 0x45,
        NUM_SLASH = 0x135,
        NUM_ASTERISK = 0x37,
        NUM_MINUS = 0x4A,
        TAB = 0x0F,
        Q = 0x10,
        W = 0x11,
        E = 0x12,
        R = 0x13,
        T = 0x14,
        Y = 0x15,
        U = 0x16,
        I = 0x17,
        O = 0x18,
        P = 0x19,
        OPEN_BRACKET = 0x1A,
        CLOSE_BRACKET = 0x1B,
        BACKSLASH = 0x2B,
        KEYBOARD_DELETE = 0x153,
        END = 0x14F,
        PAGE_DOWN = 0x151,
        NUM_SEVEN = 0x47,
        NUM_EIGHT = 0x48,
        NUM_NINE = 0x49,
        NUM_PLUS = 0x4E,
        CAPS_LOCK = 0x3A,
        A = 0x1E,
        S = 0x1F,
        D = 0x20,
        F = 0x21,
        G = 0x22,
        H = 0x23,
        J = 0x24,
        K = 0x25,
        L = 0x26,
        SEMICOLON = 0x27,
        APOSTROPHE = 0x28,
        ENTER = 0x1C,
        NUM_FOUR = 0x4B,
        NUM_FIVE = 0x4C,
        NUM_SIX = 0x4D,
        LEFT_SHIFT = 0x2A,
        Z = 0x2C,
        X = 0x2D,
        C = 0x2E,
        V = 0x2F,
        B = 0x30,
        N = 0x31,
        M = 0x32,
        COMMA = 0x33,
        PERIOD = 0x34,
        FORWARD_SLASH = 0x35,
        RIGHT_SHIFT = 0x36,
        ARROW_UP = 0x148,
        NUM_ONE = 0x4F,
        NUM_TWO = 0x50,
        NUM_THREE = 0x51,
        NUM_ENTER = 0x11C,
        LEFT_CONTROL = 0x1D,
        LEFT_WINDOWS = 0x15B,
        LEFT_ALT = 0x38,
        SPACE = 0x39,
        RIGHT_ALT = 0x138,
        RIGHT_WINDOWS = 0x15C,
        APPLICATION_SELECT = 0x15D,
        RIGHT_CONTROL = 0x11D,
        ARROW_LEFT = 0x14B,
        ARROW_DOWN = 0x150,
        ARROW_RIGHT = 0x14D,
        NUM_ZERO = 0x52,
        NUM_PERIOD = 0x53,
        G_1 = 0xFFF1,
        G_2 = 0xFFF2,
        G_3 = 0xFFF3,
        G_4 = 0xFFF4,
        G_5 = 0xFFF5,
        G_6 = 0xFFF6,
        G_7 = 0xFFF7,
        G_8 = 0xFFF8,
        G_9 = 0xFFF9,
        G_LOGO = 0xFFFF1,
        G_BADGE = 0xFFFF2
    };

    public enum DeviceType
    {
        Keyboard = 0x0,
        Mouse = 0x3,
        Mousemat = 0x4,
        Headset = 0x8,
        Speaker = 0xe
    }

    public class LogitechGSDK
    {
        private const int LOGI_DEVICETYPE_MONOCHROME_ORD = 0;
        private const int LOGI_DEVICETYPE_RGB_ORD = 1;
        private const int LOGI_DEVICETYPE_PERKEY_RGB_ORD = 2;

        public const int LOGI_DEVICETYPE_MONOCHROME = (1 << LOGI_DEVICETYPE_MONOCHROME_ORD);
        public const int LOGI_DEVICETYPE_RGB = (1 << LOGI_DEVICETYPE_RGB_ORD);
        public const int LOGI_DEVICETYPE_PERKEY_RGB = (1 << LOGI_DEVICETYPE_PERKEY_RGB_ORD);
        public const int LOGI_DEVICETYPE_ALL = (LOGI_DEVICETYPE_MONOCHROME | LOGI_DEVICETYPE_RGB | LOGI_DEVICETYPE_PERKEY_RGB);

        public const int LOGI_LED_BITMAP_WIDTH = 21;
        public const int LOGI_LED_BITMAP_HEIGHT = 6;
        public const int LOGI_LED_BITMAP_BYTES_PER_KEY = 4;

        public const int LOGI_LED_BITMAP_SIZE = LOGI_LED_BITMAP_WIDTH * LOGI_LED_BITMAP_HEIGHT * LOGI_LED_BITMAP_BYTES_PER_KEY;
        public const int LOGI_LED_DURATION_INFINITE = 0;

        #region Library management

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

            string target = Configuration.PluginConfig.Instance.LogitechGPath + "-x" + (Environment.Is64BitProcess ? "64" : "86") + ".dll";

            // HACK: Load library at runtime to support both, x86 and x64 with one managed dll
            string dllPath = null;
            if (File.Exists(target))
                dllPath = target;

            if (dllPath == null)
            {
                Plugin.Log.Error($"Can't find the LogitechG-SDK at the expected location: '{Path.GetFullPath(target)}'");
                return;
            }

            Plugin.Log.Info($"Loading LogitechG-SDK from '{Path.GetFullPath(target)}'");

            _dllHandle = LoadLibrary(dllPath);

            _logiLedInitPtr = (LogiLedInitPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "LogiLedInit"), typeof(LogiLedInitPointer));
            _logiLedInitWithNamePtr = (LogiLedInitWithNamePointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "LogiLedInitWithName"), typeof(LogiLedInitWithNamePointer));
            _logiLedSetTargetDevicePtr = (LogiLedSetTargetDevicePointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "LogiLedSetTargetDevice"), typeof(LogiLedSetTargetDevicePointer));
            _logiLedSetLightingPtr = (LogiLedSetLightingPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "LogiLedSetLighting"), typeof(LogiLedSetLightingPointer));
            _logiLedSetLightingFromBitmapPtr = (LogiLedSetLightingFromBitmapPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "LogiLedSetLightingFromBitmap"), typeof(LogiLedSetLightingFromBitmapPointer));
            _logiLedShutdownPtr = (LogiLedShutdownPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "LogiLedShutdown"), typeof(LogiLedShutdownPointer));

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

        private static LogiLedInitPointer _logiLedInitPtr;
        private static LogiLedInitWithNamePointer _logiLedInitWithNamePtr;
        private static LogiLedSetTargetDevicePointer _logiLedSetTargetDevicePtr;
        private static LogiLedSetLightingPointer _logiLedSetLightingPtr;
        private static LogiLedSetLightingFromBitmapPointer _logiLedSetLightingFromBitmapPtr;
        private static LogiLedShutdownPointer _logiLedShutdownPtr;

        #endregion

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool LogiLedInitPointer();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool LogiLedInitWithNamePointer(string name);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool LogiLedSetTargetDevicePointer(int targetDevice);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool LogiLedSetLightingPointer(int redPercentage, int greenPercentage, int bluePercentage);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool LogiLedSetLightingFromBitmapPointer(byte[] bitmap);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LogiLedShutdownPointer();

        #endregion

        #endregion

        internal static bool LogiLedInit()
        {
            return _logiLedInitPtr();
        }

        internal static bool LogiLedInitWithName(string name)
        {
            return _logiLedInitWithNamePtr(name);
        }

        internal static bool LogiLedSetTargetDevice(int targetDevice)
        {
            return _logiLedSetTargetDevicePtr(targetDevice);
        }

        internal static bool LogiLedSetLighting(int redPercentage, int greenPercentage, int bluePercentage)
        {
            return _logiLedSetLightingPtr(redPercentage, greenPercentage, bluePercentage);
        }

        internal static bool LogiLedSetLightingFromBitmap(byte[] bitmap)
        {
            return _logiLedSetLightingFromBitmapPtr(bitmap);
        }

        internal static void LogiLedShutdown()
        {
            _logiLedShutdownPtr();
        }
    }

}
