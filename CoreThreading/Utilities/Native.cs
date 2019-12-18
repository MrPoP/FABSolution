using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading.Utilities
{
    public static class Native
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long rand();
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long srand(ulong seed);
        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);
        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceFrequency(out long lpFrequency);
        [DllImport("winmm.dll", EntryPoint = "timeGetTime", CallingConvention = CallingConvention.StdCall)]
        public static extern uint GetTime();

        [DllImport("msvcrt.dll", EntryPoint = "_time32", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint GetUnixTime32(int timer = 0);

        [DllImport("msvcrt.dll", EntryPoint = "_time64", CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong GetUnixTime64(long timer = 0);
        [DllImport("kernel32")]
        public static extern ulong GetTickCount64();
    }
}
