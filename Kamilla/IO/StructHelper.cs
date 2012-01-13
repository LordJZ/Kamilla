using System;
using System.Runtime.InteropServices;

namespace Kamilla.IO
{
    internal static class StructHelper<T> where T : struct
    {
        public static readonly int Size;
        public static readonly IntPtr UnmanagedDataBank;
        public static readonly byte[] ManagedDataBank;
        public static readonly object SyncRoot;
        public static readonly Type Type;

        static StructHelper()
        {
            SyncRoot = new object();
            Type = typeof(T);
            Size = Marshal.SizeOf(Type);
            ManagedDataBank = new byte[Size];
            UnmanagedDataBank = Marshal.AllocHGlobal(Size);
        }
    }
}
