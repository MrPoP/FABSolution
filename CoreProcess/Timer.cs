using System;
using System.Runtime.InteropServices;

namespace CoreProcess
{
    public class Timer
    {
        private uint time;

        public Timer(bool local = true)
        {
            time = local ? TimeStamp.GetTime() : 0;
        }
        public void Set(uint time, int amount)
        {
            this.time = (uint)(time + amount);
        }
        public void Set(int amount)
        {
            time = (uint)(TimeStamp.GetTime() + amount);
        }
        public bool ValidateFrame(uint now, int leeway)
        {
            if (time == 0)
            {
                this.time = now;
                return true;
            }
            int delta = (int)(now - time);
            bool result = (delta > leeway);
            if (result)
                time = now;
            return result;
        }
        public bool IsReady(uint now)
        {
            return time < now;
        }
        public int RemainingTime
        {
            get { return (int)(time - TimeStamp.GetTime()); }
        }
        public int ElapsedSinceTick
        {
            get { return (int)(TimeStamp.GetTime() - time); }
        }
        public static implicit operator bool(Timer t)
        {
            return t.time < TimeStamp.GetTime();
        }
    }

    public class TimeStamp
    {
        [DllImport("winmm.dll", EntryPoint = "timeGetTime", CallingConvention = CallingConvention.StdCall)]
        public static extern uint GetTime();

        [DllImport("msvcrt.dll", EntryPoint = "_time32", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint GetUnixTime32(int timer = 0);

        [DllImport("msvcrt.dll", EntryPoint = "_time64", CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong GetUnixTime64(long timer = 0);
    }
}
