using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CUESaber.Utils
{
    public class NativeMemoryUtils
    {
        public static void MarshalUnmananagedArray2Struct<T>(IntPtr unmanagedArray, int length, out T[] mangagedArray)
        {
            int size = Marshal.SizeOf(typeof(T));
            mangagedArray = new T[length];

            for (int i = 0; i < length; i++)
            {
                IntPtr ins = new IntPtr(unmanagedArray.ToInt64() + i * size);
                mangagedArray[i] = Marshal.PtrToStructure<T>(ins);
            }
        }

        public static IntPtr StructArray2MarshalUnmanagedArray<T>(List<T> managedArray)
        {
            int byteLen = Marshal.SizeOf(managedArray[0]);
            IntPtr ptr = Marshal.AllocHGlobal(byteLen * managedArray.Count);
            long LongPtr = ptr.ToInt64(); // Must work both on x86 and x64
            for (int I = 0; I < managedArray.Count; I++)
            {
                IntPtr RectPtr = new IntPtr(LongPtr);
                Marshal.StructureToPtr<T>(managedArray[I], RectPtr, false); // You do not need to erase struct in this case
                LongPtr += byteLen;
            }
            return ptr;
        }
    }
}
