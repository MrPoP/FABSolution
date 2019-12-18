using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreDataStore
{
    public delegate void XParameterizedThreadStart(object obj);
    public sealed class POPThread
    {
        private readonly EventWaitHandle completed;
        private static POPThread currentThread;
        private static int maxThreadId;
        private readonly EventWaitHandle readyToStart;
        private object startupParameter;
        private Task task;
        private readonly int threadId;
        private POPThread()
        {
            this.completed = new EventWaitHandle(false, EventResetMode.AutoReset);
            this.readyToStart = new EventWaitHandle(false, EventResetMode.AutoReset);
            this.threadId = GetNewThreadId();
            this.IsAlive = false;
        }

        public POPThread(XParameterizedThreadStart threadStartFunc)
        {
            this.completed = new EventWaitHandle(false, EventResetMode.AutoReset);
            this.readyToStart = new EventWaitHandle(false, EventResetMode.AutoReset);
            this.threadId = GetNewThreadId();
            this.IsAlive = false;
            this.CreateLongRunningTask(threadStartFunc);
        }

        public POPThread(Action action)
        {
            this.completed = new EventWaitHandle(false, EventResetMode.AutoReset);
            this.readyToStart = new EventWaitHandle(false, EventResetMode.AutoReset);
            this.threadId = GetNewThreadId();
            this.IsAlive = false;
            this.CreateLongRunningTask(x => action());
        }
        private void CreateLongRunningTask(XParameterizedThreadStart threadStartFunc)
        {
            this.task = Task.Factory.StartNew(delegate
            {
                this.readyToStart.WaitOne();
                currentThread = this;
                threadStartFunc(this.startupParameter);
                this.completed.Set();
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        private static int GetNewThreadId()
        {
            return Interlocked.Increment(ref maxThreadId);
        }

        public bool Join(int millisecondsTimeout)
        {
            return this.completed.WaitOne(millisecondsTimeout);
        }

        public bool Join(TimeSpan timeout)
        {
            return this.completed.WaitOne(timeout);
        }

        public static void Sleep(int millisecondsTimeout)
        {
            Task.Delay(millisecondsTimeout).Wait();
        }

        public void Start()
        {
            this.task.Start();
            this.readyToStart.Set();
            this.IsAlive = true;
        }

        public void Start(object parameter)
        {
            this.startupParameter = parameter;
            this.Start();
        }

        public static POPThread CurrentThread
        {
            get
            {
                return (currentThread ?? (currentThread = new POPThread()));
            }
        }

        public int Id
        {
            get
            {
                return this.threadId;
            }
        }

        public bool IsAlive { get; private set; }

        public string Name { get; set; }
    }
}
