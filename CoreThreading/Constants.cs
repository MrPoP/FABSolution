using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading
{
    [Flags]
    public enum StackAction
    {
        None,
        Push,
        Pop,
        Peek
    }
    [Flags]
    public enum StackOrder
    {
        Pop,
        Peek
    }
    [Flags]
    public enum ThreadType
    {
        Invocator,
        MultiThreading,
        LongRunning
    }
    [Flags]
    public enum ThreadStatus
    {
        Active = 1,
        Dead = 2
    }
    [Flags]
    public enum PerlaStatus
    {
        Disposed,
        Created,
        WaitingToRun,
        Running,
        Completed,
        Canceled,
        Faulted
    }
    [Flags]
    public enum PerlaExcutionFlag
    {
        Single,
        Period,
        Times,
        LongRunning
    }
    [Flags]
    public enum ConsoleFontSize : int
    {
        //Start Lucida Font
        _3_X_5,
        _4_X_7,
        _4_X_8,
        _4_X_10,
        _4_X_12,
        _4_X_14,
        _4_X_16,
        _4_X_18,
        _4_X_20,
        _4_X_24,
        _4_X_28,
        _4_X_36,
        _4_X_72,
        //End Lucida Font
        _4_X_6,
        _6_X_8,
        _8_X_8,
        _16_X_8,
        _5_X_12,
        _7_X_12,
        _8_X_12,
        _16_X_12,
        _12_X_16,
        _10_X_18,
    }
    [Flags]
    public enum FontWeight : int
    {
        Thin = 100,
        UltraLight = 200,
        Light = 300,
        Regular = 400,
        Medium = 500,
        SemiBold = 600,
        Bold = 700,
        UltraBold = 800,
        Black = 900,
        UltraBlack = 950
    }
    public enum ClipBoardOPType
    {
        Get,
        Set
    }
    [Flags]
    public enum ClipBoardConType
    {
        Text = 0,
        Image = 1,
        FilesDropList = 2,
        Audio = 3,
        Undefined = 4
    }
    [Flags]
    public enum ClipBoardHandleOptions : int
    {
        WM_CREATE = 0x0001,
        WM_DESTROY = 0x0002,
        WM_CHANGECBCHAIN = 0x030D,
        WM_DRAWCLIPBOARD = 0x0308
    }
}
