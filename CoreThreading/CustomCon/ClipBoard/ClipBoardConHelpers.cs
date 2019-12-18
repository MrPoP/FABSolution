using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using IComDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace CoreThreading.CustomCon
{
    public static class ClipBoardConHelpers
    {
        [StructLayout(LayoutKind.Sequential, Pack=8)]
        public struct MSG
        {
            public IntPtr hwnd;
            public int Message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint Time;
            public POINT Point;
        }
        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int X;
            public int Y;
            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
        [DllImport("user32.dll")]
        static extern int GetMessage(out System.Windows.Forms.Message lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        [DllImport("user32.dll")]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("user32.dll")]
        public static extern IntPtr GetClipboardViewer();
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        static extern IntPtr GetClipboardOwner();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("ole32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int OleGetClipboard(ref IComDataObject data);
        [DllImport("ole32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int OleSetClipboard(IComDataObject pDataObj);
        [DllImport("ole32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int OleFlushClipboard();
    }
    
}
