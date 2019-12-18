using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading.Threading
{
    internal class PerlaExceptionHolder
    {
        private readonly PerlaItem m_item;
        private volatile List<ExceptionDispatchInfo> m_faultExceptions;
        private ExceptionDispatchInfo m_cancellationException;
        private volatile bool m_isHandled;
        internal PerlaExceptionHolder(PerlaItem item)
        {
            Debug.Assert(item != null, "Expected a non-null item.");
            m_item = item;
        }
        ~PerlaExceptionHolder()
        {
            if (m_faultExceptions != null && !m_isHandled)
            {
                AggregateException exceptionToThrow = new AggregateException(
                    "PerlaExceptionHolder UnhandledException",
                    m_faultExceptions.Select(p => p.SourceException));
                UnobservedPerlaExceptionEventArgs ueea = new UnobservedPerlaExceptionEventArgs(exceptionToThrow);
                PerlaScheduler.PublishUnobservedPerlaException(m_item, ueea);
            }
        }
        internal bool ContainsFaultList { get { return m_faultExceptions != null; } }
        internal void Add(object exceptionObject, bool representsCancellation)
        {
            Debug.Assert(exceptionObject != null, "PerlaExceptionHolder.Add(): Expected a non-null exceptionObject");
            Debug.Assert(
                exceptionObject is Exception || exceptionObject is IEnumerable<Exception> ||
                exceptionObject is ExceptionDispatchInfo || exceptionObject is IEnumerable<ExceptionDispatchInfo>,
                "PerlaExceptionHolder.Add(): Expected Exception, IEnumerable<Exception>, ExceptionDispatchInfo, or IEnumerable<ExceptionDispatchInfo>");

            if (representsCancellation) SetCancellationException(exceptionObject);
            else AddFaultException(exceptionObject);
        }
        private void SetCancellationException(object exceptionObject)
        {
            Debug.Assert(exceptionObject != null, "Expected exceptionObject to be non-null.");
            Debug.Assert(m_cancellationException == null,
                "Expected SetCancellationException to be called only once.");
            Debug.Assert(m_faultExceptions == null,
                "Expected SetCancellationException to be called before any faults were added.");
            OperationCanceledException oce;
            if (exceptionObject is OperationCanceledException)
            {
                oce = (exceptionObject as OperationCanceledException);
                m_cancellationException = ExceptionDispatchInfo.Capture(oce);
            }
            else
            {
                var edi = exceptionObject as ExceptionDispatchInfo;
                Debug.Assert(edi != null && edi.SourceException is OperationCanceledException,
                    "Expected an OCE or an EDI that contained an OCE");
                m_cancellationException = edi;
            }
            MarkAsHandled(false);
        }
        private void AddFaultException(object exceptionObject)
        {
            Debug.Assert(exceptionObject != null, "AddFaultException(): Expected a non-null exceptionObject");
            List<ExceptionDispatchInfo> exceptions = m_faultExceptions;
            if (exceptions == null) m_faultExceptions = exceptions = new List<ExceptionDispatchInfo>(1);
            else Debug.Assert(exceptions.Count > 0, "Expected existing exceptions list to have > 0 exceptions.");
            Exception exception;
            if (exceptionObject is Exception)
            {
                exception = (exceptionObject as Exception);
                exceptions.Add(ExceptionDispatchInfo.Capture(exception));
            }
            else
            {
                ExceptionDispatchInfo edi;
                if (exceptionObject is ExceptionDispatchInfo)
                {
                    edi = (exceptionObject as ExceptionDispatchInfo);
                    exceptions.Add(edi);
                }
                else
                {
                    IEnumerable<Exception> exColl;
                    if (exceptionObject is IEnumerable<Exception>)
                    {
                        exColl = (exceptionObject as IEnumerable<Exception>);
#if DEBUG
                        int numExceptions = 0;
#endif
                        foreach (Exception exc in exColl)
                        {
#if DEBUG
                            Debug.Assert(exc != null, "No exceptions should be null");
                            numExceptions++;
#endif
                            exceptions.Add(ExceptionDispatchInfo.Capture(exc));
                        }
#if DEBUG
                        Debug.Assert(numExceptions > 0, "Collection should contain at least one exception.");
#endif
                    }
                    else
                    {
                        IEnumerable<ExceptionDispatchInfo> ediColl;
                        if (exceptionObject is IEnumerable<ExceptionDispatchInfo>)
                        {
                            ediColl = (exceptionObject as IEnumerable<ExceptionDispatchInfo>);
                            exceptions.AddRange(ediColl);
#if DEBUG
                            Debug.Assert(exceptions.Count > 0, "There should be at least one dispatch info.");
                            foreach (ExceptionDispatchInfo tmp in exceptions)
                            {
                                Debug.Assert(tmp != null, "No dispatch infos should be null");
                            }
#endif
                        }
                        else
                        {
                            throw new ArgumentException("TaskExceptionHolder UnknownExceptionType", exceptionObject.ToString());
                        }
                    }
                }
            }

            if (exceptions.Count > 0)
                MarkAsUnhandled();
        }
        private void MarkAsUnhandled()
        {
            if (m_isHandled)
            {
                GC.ReRegisterForFinalize(this);
                m_isHandled = false;
            }
        }
        internal void MarkAsHandled(bool calledFromFinalizer)
        {
            if (!m_isHandled)
            {
                if (!calledFromFinalizer)
                {
                    GC.SuppressFinalize(this);
                }

                m_isHandled = true;
            }
        }
        internal AggregateException CreateExceptionObject(bool calledFromFinalizer, Exception includeThisException)
        {
            List<ExceptionDispatchInfo> exceptions = m_faultExceptions;
            Debug.Assert(exceptions != null, "Expected an initialized list.");
            Debug.Assert(exceptions.Count > 0, "Expected at least one exception.");
            MarkAsHandled(calledFromFinalizer);
            if (includeThisException == null)
                return new AggregateException(exceptions.Select(p => p.SourceException));
            Exception[] combinedExceptions = new Exception[exceptions.Count + 1];
            for (int i = 0; i < combinedExceptions.Length; i++)
            {
                if (i == 0)
                    continue;
                combinedExceptions[i] = exceptions[i].SourceException;
            }
            combinedExceptions[0] = includeThisException;
            return new AggregateException(combinedExceptions);
        }
        internal ReadOnlyCollection<ExceptionDispatchInfo> GetExceptionDispatchInfos()
        {
            List<ExceptionDispatchInfo> exceptions = m_faultExceptions;
            Debug.Assert(exceptions != null, "Expected an initialized list.");
            Debug.Assert(exceptions.Count > 0, "Expected at least one exception.");
            MarkAsHandled(false);
            return new ReadOnlyCollection<ExceptionDispatchInfo>(exceptions);
        }
        internal ExceptionDispatchInfo GetCancellationExceptionDispatchInfo()
        {
            ExceptionDispatchInfo edi = m_cancellationException;
            Debug.Assert(edi == null || edi.SourceException is OperationCanceledException,
                "Expected the EDI to be for an OperationCanceledException");
            return edi;
        }
    }
}
