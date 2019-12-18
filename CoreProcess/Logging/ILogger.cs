using System;

namespace CoreProcess.Logging
{
    public interface ILogger
    {
        string Name { get; }
        bool TraceEnabled { get; }
        ILogger Trace(string msg);
        ILogger Trace(string format, object arg);
        ILogger Trace(string format, object argA, object argB);
        ILogger Trace(string format, params object[] arguments);
        ILogger Trace(string msg, Exception t);
        ILogger Trace(Exception t);
        bool DebugEnabled { get; }
        ILogger Debug(string msg);
        ILogger Debug(string format, object arg);
        ILogger Debug(string format, object argA, object argB);
        ILogger Debug(string format, params object[] arguments);
        ILogger Debug(string msg, Exception t);
        ILogger Debug(Exception t);
        bool InformationEnabled { get; }
        ILogger Information(string msg);
        ILogger Information(string format, object arg);
        ILogger Information(string format, object argA, object argB);
        ILogger Information(string format, params object[] arguments);
        ILogger Information(string msg, Exception t);
        ILogger Information(Exception t);
        bool WarningEnabled { get; }
        ILogger Warning(string msg);
        ILogger Warning(string format, object arg);
        ILogger Warning(string format, object argA, object argB);
        ILogger Warning(string format, params object[] arguments);
        ILogger Warning(string msg, Exception t);
        ILogger Warning(Exception t);
        bool ErrorEnabled { get; }
        ILogger Error(string msg);
        ILogger Error(string format, object arg);
        ILogger Error(string format, object argA, object argB);
        ILogger Error(string format, params object[] arguments);
        ILogger Error(string msg, Exception t);
        ILogger Error(Exception t);
        bool CriticalEnabled { get; }
        ILogger Critical(string msg);
        ILogger Critical(string format, object arg);
        ILogger Critical(string format, object argA, object argB);
        ILogger Critical(string format, params object[] arguments);
        ILogger Critical(string msg, Exception t);
        ILogger Critical(Exception t);
        bool IsEnabled(LogFlag level);
        ILogger Log(LogFlag level, string msg);
        ILogger Log(LogFlag level, string format, object arg);
        ILogger Log(LogFlag level, string format, object argA, object argB);
        ILogger Log(LogFlag level, string format, params object[] arguments);
        ILogger Log(LogFlag level, string msg, Exception t);
        ILogger Log(LogFlag level, Exception t);
    }
}
