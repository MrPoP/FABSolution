using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading.CustomCon
{
    internal static class CustomConsoleHerlpers
    {
        #region NativeFunctionality
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool GetCurrentConsoleFontEx(
               IntPtr consoleOutput,
               bool maximumWindow,
               ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool SetCurrentConsoleFontEx(
               IntPtr consoleOutput,
               bool maximumWindow,
               CONSOLE_FONT_INFO_EX consoleCurrentFontEx);

        [DllImport("user32.dll")]
        static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("user32.dll")]
        static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
        #endregion
        #region Variables
        internal const int STD_OUTPUT_HANDLE = -11;
        internal static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        #endregion
    }
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CONSOLE_FONT_INFO_EX
    {
        public uint cbSize;
        public uint nFont;
        public COORD dwFontSize;
        public int FontFamily;
        public int FontWeight;
        public fixed char FaceName[32];
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct COORD
    {
        public short X;
        public short Y;

        public COORD(short x, short y)
        {
            X = x;
            Y = y;
        }
        public static COORD Create(ConsoleFontSize size)
        {
            short X, Y;
            switch (size)
            {
                case ConsoleFontSize._3_X_5:
                    {
                        X = 3;
                        Y = 5;
                        break;
                    }
                case ConsoleFontSize._4_X_6:
                    {
                        X = 4;
                        Y = 6;
                        break;
                    }
                case ConsoleFontSize._4_X_7:
                    {
                        X = 4;
                        Y = 7;
                        break;
                    }
                case ConsoleFontSize._4_X_8:
                    {
                        X = 4;
                        Y = 8;
                        break;
                    }
                case ConsoleFontSize._4_X_10:
                    {
                        X = 4;
                        Y = 10;
                        break;
                    }
                case ConsoleFontSize._4_X_12:
                    {
                        X = 4;
                        Y = 12;
                        break;
                    }
                case ConsoleFontSize._4_X_14:
                    {
                        X = 4;
                        Y = 14;
                        break;
                    }
                case ConsoleFontSize._4_X_16:
                    {
                        X = 4;
                        Y = 16;
                        break;
                    }
                case ConsoleFontSize._4_X_18:
                    {
                        X = 4;
                        Y = 18;
                        break;
                    }
                case ConsoleFontSize._4_X_20:
                    {
                        X = 4;
                        Y = 20;
                        break;
                    }
                case ConsoleFontSize._4_X_24:
                    {
                        X = 4;
                        Y = 24;
                        break;
                    }
                case ConsoleFontSize._4_X_28:
                    {
                        X = 4;
                        Y = 28;
                        break;
                    }
                case ConsoleFontSize._4_X_36:
                    {
                        X = 4;
                        Y = 36;
                        break;
                    }
                case ConsoleFontSize._4_X_72:
                    {
                        X = 4;
                        Y = 72;
                        break;
                    }
                case ConsoleFontSize._6_X_8:
                    {
                        X = 6;
                        Y = 8;
                        break;
                    }
                case ConsoleFontSize._8_X_8:
                    {
                        X = 8;
                        Y = 8;
                        break;
                    }
                case ConsoleFontSize._16_X_8:
                    {
                        X = 16;
                        Y = 8;
                        break;
                    }
                case ConsoleFontSize._5_X_12:
                    {
                        X = 5;
                        Y = 12;
                        break;
                    }
                case ConsoleFontSize._7_X_12:
                    {
                        X = 7;
                        Y = 12;
                        break;
                    }
                case ConsoleFontSize._8_X_12:
                    {
                        X = 8;
                        Y = 12;
                        break;
                    }
                case ConsoleFontSize._16_X_12:
                    {
                        X = 16;
                        Y = 12;
                        break;
                    }
                case ConsoleFontSize._12_X_16:
                    {
                        X = 12;
                        Y = 16;
                        break;
                    }
                case ConsoleFontSize._10_X_18:
                    {
                        X = 10;
                        Y = 18;
                        break;
                    }
                default:
                    {
                        X = 8;
                        Y = 12;
                        break;
                    }
            }
            return new COORD(X, Y);
        }
    }
}
