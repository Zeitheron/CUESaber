﻿#pragma warning disable 169 // Field 'x' is never used
#pragma warning disable 414 // Field 'x' is assigned but its value never used
#pragma warning disable 649 // Field 'x' is never assigned

using System;
using System.Runtime.InteropServices;

namespace CUESaber.Native
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// CUE-SDK: contains information about device
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class CorsairDeviceInfo
    {
        /// <summary>
        /// CUE-SDK: enum describing device type
        /// </summary>
        internal CorsairDeviceType type;

        /// <summary>
        /// CUE-SDK: null - terminated device model(like “K95RGB”)
        /// </summary>
        internal IntPtr model;

        /// <summary>
        /// CUE-SDK: enum describing physical layout of the keyboard or mouse
        /// </summary>
        internal int physicalLayout;

        /// <summary>
        /// CUE-SDK: enum describing logical layout of the keyboard as set in CUE settings
        /// </summary>
        internal int logicalLayout;

        /// <summary>
        /// CUE-SDK: mask that describes device capabilities, formed as logical “or” of CorsairDeviceCaps enum values
        /// </summary>
        internal int capsMask;

        /// <summary>
        /// CUE-SDK: number of controllable LEDs on the device
        /// </summary>
        internal int ledsCount;

        internal static CorsairDeviceInfo FromPtr(IntPtr ptr)
        {
            CorsairDeviceInfo inf = new CorsairDeviceInfo();
            Marshal.PtrToStructure(ptr, inf);
            return inf;
        }
    }
}
