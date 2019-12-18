using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CoreProcess.Logging
{
    public sealed class Logger : ILogger
    {
        public static ILogger BaseLogger = new Logger();

        private Dictionary<LogFlag, bool> enabledFlags = null;
        public string Name { get { return "FABSolution"; } }
        public bool IsEnabled(LogFlag level) { return (byte)level > 0 && (byte)level < 7 ? this.enabledFlags[level] : false; }
        public bool TraceEnabled { get { return this.IsEnabled(LogFlag.Trace); } }
        public bool DebugEnabled { get { return this.IsEnabled(LogFlag.Debug); } }
        public bool InformationEnabled { get { return this.IsEnabled(LogFlag.Information); } }
        public bool WarningEnabled { get { return this.IsEnabled(LogFlag.Warning); } }
        public bool ErrorEnabled { get { return this.IsEnabled(LogFlag.Error); } }
        public bool CriticalEnabled { get { return this.IsEnabled(LogFlag.Critical); } }

        public Logger()
        {
            if (!EventLog.SourceExists("FABSolution"))
            {
                EventLog.CreateEventSource(new EventSourceCreationData("FABSolution", "Application"));
            }
            this.enabledFlags = new Dictionary<LogFlag, bool>()
            {
                {LogFlag.Trace, true},
                {LogFlag.Debug, true},
                {LogFlag.Information, true},
                {LogFlag.Warning, true},
                {LogFlag.Error, true},
                {LogFlag.Critical, true}
            };
        }

        ~Logger()
        {
            this.enabledFlags = null;
        }

        public ILogger Log(LogFlag level, string msg)
        {
            if (IsEnabled(level))
            {
                switch (level)
                {
                    case LogFlag.Critical:
                    case LogFlag.Error:
                        {
                            EventLog.WriteEntry(this.Name, msg, EventLogEntryType.Error);
                            break;
                        }
                    case LogFlag.Warning:
                        {
                            EventLog.WriteEntry(this.Name, msg, EventLogEntryType.Warning);
                            break;
                        }
                    case LogFlag.Information:
                        {
                            EventLog.WriteEntry(this.Name, msg, EventLogEntryType.Information);
                            break;
                        }
                    case LogFlag.Debug:
                        {
                            EventLog.WriteEntry(this.Name, msg, EventLogEntryType.FailureAudit);
                            break;
                        }
                    case LogFlag.Trace:
                        {
                            EventLog.WriteEntry(this.Name, msg, EventLogEntryType.SuccessAudit);
                            break;
                        }
                    default:
                        break;
                }
            }
            return this;
        }
        public ILogger Log(LogFlag level, string format, object arg) { return this.Log(level, string.Format(format, arg)); }
        public ILogger Log(LogFlag level, string format, object argA, object argB) { return this.Log(level, string.Format(format, argA, argB)); }
        public ILogger Log(LogFlag level, string format, params object[] arguments) { return this.Log(level, string.Format(format, arguments)); }
        public ILogger Log(LogFlag level, string msg, Exception t)
        {
            if (IsEnabled(level))
            {
                switch (level)
                {
                    case LogFlag.Critical:
                    case LogFlag.Error:
                        {
                            EventLog.WriteEntry(this.Name, string.Format("{0} {1} {2}", msg, Environment.NewLine, t.ToString()), EventLogEntryType.Error);
                            break;
                        }
                    case LogFlag.Warning:
                        {
                            EventLog.WriteEntry(this.Name, string.Format("{0} {1} {2}", msg, Environment.NewLine, t.ToString()), EventLogEntryType.Warning);
                            break;
                        }
                    case LogFlag.Information:
                        {
                            EventLog.WriteEntry(this.Name, string.Format("{0} {1} {2}", msg, Environment.NewLine, t.ToString()), EventLogEntryType.Information);
                            break;
                        }
                    case LogFlag.Debug:
                        {
                            EventLog.WriteEntry(this.Name, string.Format("{0} {1} {2}", msg, Environment.NewLine, t.ToString()), EventLogEntryType.FailureAudit);
                            break;
                        }
                    case LogFlag.Trace:
                        {
                            EventLog.WriteEntry(this.Name, string.Format("{0} {1} {2}", msg, Environment.NewLine, t.ToString()), EventLogEntryType.SuccessAudit);
                            break;
                        }
                    default:
                        break;
                }
            }
            return this;
        }
        public ILogger Log(LogFlag level, Exception t) { return this.Log(level, string.Empty, t); }

        public ILogger Trace(string msg) { return this.Log(LogFlag.Trace, msg); }
        public ILogger Trace(string format, object arg) { return this.Log(LogFlag.Trace, format, arg); }
        public ILogger Trace(string format, object argA, object argB) { return this.Log(LogFlag.Trace, format, argA, argB); }
        public ILogger Trace(string format, params object[] arguments) { return this.Log(LogFlag.Trace, format, arguments); }
        public ILogger Trace(string msg, Exception t) { return this.Log(LogFlag.Trace, msg, t); }
        public ILogger Trace(Exception t) { return this.Log(LogFlag.Trace, t); }

        public ILogger Debug(string msg) { return this.Log(LogFlag.Debug, msg); }
        public ILogger Debug(string format, object arg) { return this.Log(LogFlag.Debug, format, arg); }
        public ILogger Debug(string format, object argA, object argB) { return this.Log(LogFlag.Debug, format, argA, argB); }
        public ILogger Debug(string format, params object[] arguments) { return this.Log(LogFlag.Debug, format, arguments); }
        public ILogger Debug(string msg, Exception t) { return this.Log(LogFlag.Debug, msg, t); }
        public ILogger Debug(Exception t) { return this.Log(LogFlag.Debug, t); }

        public ILogger Information(string msg) { return this.Log(LogFlag.Information, msg); }
        public ILogger Information(string format, object arg) { return this.Log(LogFlag.Information, format, arg); }
        public ILogger Information(string format, object argA, object argB) { return this.Log(LogFlag.Information, format, argA, argB); }
        public ILogger Information(string format, params object[] arguments) { return this.Log(LogFlag.Information, format, arguments); }
        public ILogger Information(string msg, Exception t) { return this.Log(LogFlag.Information, msg, t); }
        public ILogger Information(Exception t) { return this.Log(LogFlag.Information, t); }

        public ILogger Warning(string msg) { return this.Log(LogFlag.Warning, msg); }
        public ILogger Warning(string format, object arg) { return this.Log(LogFlag.Warning, format, arg); }
        public ILogger Warning(string format, object argA, object argB) { return this.Log(LogFlag.Warning, format, argA, argB); }
        public ILogger Warning(string format, params object[] arguments) { return this.Log(LogFlag.Warning, format, arguments); }
        public ILogger Warning(string msg, Exception t) { return this.Log(LogFlag.Warning, msg, t); }
        public ILogger Warning(Exception t) { return this.Log(LogFlag.Warning, t); }

        public ILogger Error(string msg) { return this.Log(LogFlag.Error, msg); }
        public ILogger Error(string format, object arg) { return this.Log(LogFlag.Error, format, arg); }
        public ILogger Error(string format, object argA, object argB) { return this.Log(LogFlag.Error, format, argA, argB); }
        public ILogger Error(string format, params object[] arguments) { return this.Log(LogFlag.Error, format, arguments); }
        public ILogger Error(string msg, Exception t) { return this.Log(LogFlag.Error, msg, t); }
        public ILogger Error(Exception t) { return this.Log(LogFlag.Error, t); }

        public ILogger Critical(string msg) { return this.Log(LogFlag.Critical, msg); }
        public ILogger Critical(string format, object arg) { return this.Log(LogFlag.Critical, format, arg); }
        public ILogger Critical(string format, object argA, object argB) { return this.Log(LogFlag.Critical, format, argA, argB); }
        public ILogger Critical(string format, params object[] arguments) { return this.Log(LogFlag.Critical, format, arguments); }
        public ILogger Critical(string msg, Exception t) { return this.Log(LogFlag.Critical, msg, t); }
        public ILogger Critical(Exception t) { return this.Log(LogFlag.Critical, t); }
    }
}
