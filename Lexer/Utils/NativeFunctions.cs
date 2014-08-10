using System;
using System.Runtime.InteropServices;

namespace Lexer.Utils
{
    internal static class NativeFunctions
    {
        private static IntPtr s_ProcessHeap;

        static NativeFunctions()
        {
            s_ProcessHeap = GetProcessHeap();
        }

        internal static IntPtr AllocateProcessMemory(int bytes)
        {
            return HeapAlloc(s_ProcessHeap, 0, (IntPtr)bytes);
        }

        internal static void FreeProcessMemory(IntPtr ptr)
        {
            HeapFree(s_ProcessHeap, 0, ptr);
        }

        [DllImport("kernel32.dll", SetLastError = false)]
        private static extern IntPtr HeapAlloc(IntPtr hHeap, uint dwFlags, IntPtr dwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcessHeap();
    }
}
