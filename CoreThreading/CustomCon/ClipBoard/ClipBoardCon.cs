using CoreThreading.Collections.Generic;
using CoreThreading.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using IComDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace CoreThreading.CustomCon
{
    public class ClipBoardCon : NativeWindow, IDisposable
    {
        #region ActivePermission
        protected Func<object, bool> CheckActivate = new Func<object, bool>(perm =>
        {
            try
            {
                if (perm is CodeAccessPermission)
                {
                    (perm as CodeAccessPermission).Demand();
                }
                if (perm is PermissionSet)
                {
                    (perm as PermissionSet).Demand();
                }
                return true;
            }
            catch
            {
                return false;
            }
        });
        #endregion
        private object syncRoot;
        private event ClipBoardConEventHandler OnClipBoardChangeIO;
        private bool restrictFormats = false;
        private ConcurrentQueue<object> i_AsyncArgs;
        private CodeAccessPermission OwnClip
        {
            get
            {
                return new UIPermission(UIPermissionClipboard.OwnClipboard);
            }
        }
        private CodeAccessPermission AllClip
        {
            get
            {
                return new UIPermission(UIPermissionClipboard.AllClipboard);
            }
        }
        private PermissionSet WriteClip
        {
            get
            {
                PermissionSet permession = new PermissionSet(PermissionState.None);
                permession.SetPermission(this.UnmanagedCode);
                permession.SetPermission(this.OwnClip);
                return permession;
            }
        }
        private CodeAccessPermission UnmanagedCode
        {
            get { return new SecurityPermission(SecurityPermissionFlag.UnmanagedCode); }
        }
        private Thread GetThread;
        private IntPtr hWndNextWindow;
        private Thread SetThread;
        public ClipBoardCon()
            :base()
        {
            this.syncRoot = new object();
            this.i_AsyncArgs = new ConcurrentQueue<object>();
            this.OnClipBoardChangeIO = new ClipBoardConEventHandler(ClipBoardCon_OnClipBoardChangeIO);
            this.restrictFormats = this.CheckActivate.Invoke(this.AllClip);
            CreateThreads();
        }
        protected void CreateThreads()
        {
            #region Set
            this.SetThread = new Thread(new ParameterizedThreadStart(x =>
            {
                int retryTimes = (int)Thread.GetData(Thread.GetNamedDataSlot("set_retryTimes"))
                    , retryDelay = (int)Thread.GetData(Thread.GetNamedDataSlot("set_retryDelay"));
                bool copy = (bool)Thread.GetData(Thread.GetNamedDataSlot("set_copy"));
                Contract.Requires<ArgumentNullException>(x != null);
                Contract.Requires<ArgumentOutOfRangeException>(retryTimes > -1);
                Contract.Requires<ArgumentOutOfRangeException>(retryDelay > -1);
                DataObject dataObject = null;
                if (!(x is IComDataObject))
                    dataObject = new DataObject(x);
                if (this.restrictFormats)
                {
                    if (dataObject == null)
                    {
                        dataObject = x as DataObject;
                    }
                    Contract.Requires<SecurityException>(IsFormatValid(dataObject));
                }
                int hr, retry = retryTimes;
                this.UnmanagedCode.Assert();
                try
                {
                    do
                    {
                        if (x is IComDataObject)
                        {
                            hr = ClipBoardConHelpers.OleSetClipboard((IComDataObject)x);
                        }
                        else
                        {
                            hr = ClipBoardConHelpers.OleSetClipboard(dataObject);
                        }
                        if (hr != 0)
                        {
                            Contract.Requires<ExternalException>(retry > 0);
                            retry--;
                            System.Threading.Thread.Sleep(retryDelay /*ms*/);
                        }
                    }
                    while (hr != 0);
                    if (copy)
                    {
                        retry = retryTimes;
                        do
                        {
                            hr = ClipBoardConHelpers.OleFlushClipboard();
                            if (hr != 0)
                            {
                                Contract.Requires<ExternalException>(retry > 0);
                                retry--;
                                System.Threading.Thread.Sleep(retryDelay);
                            }
                        }
                        while (hr != 0);
                    }
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            })) { IsBackground = true };
            this.SetThread.SetApartmentState(ApartmentState.STA);
            #endregion
            #region Get
            this.GetThread = new Thread(new ParameterizedThreadStart(x =>
            {
                int retryTimes = (int)x
                    , retryDelay = (int)Thread.GetData(Thread.GetNamedDataSlot("get_retryDelay"));
                IComDataObject dataObject = null;
                int hr, retry = retryTimes;
                do
                {
                    hr = ClipBoardConHelpers.OleGetClipboard(ref dataObject);
                    if (hr != 0)
                    {
                        Contract.Requires<ExternalException>(retry != 0);
                        retry--;
                        System.Threading.Thread.Sleep(retryDelay /*ms*/);
                    }
                }
                while (hr != 0);
                if (dataObject != null)
                {
                    if ((dataObject is System.Runtime.InteropServices.ComTypes.IDataObject) && !Marshal.IsComObject(dataObject))
                    {
                        Thread.SetData(Thread.GetNamedDataSlot("get_Result"), (System.Runtime.InteropServices.ComTypes.IDataObject)dataObject);
                    }
                    else
                    {
                        Thread.SetData(Thread.GetNamedDataSlot("get_Result"), new DataObject(dataObject));
                    }
                }
                else
                {
                    Thread.SetData(Thread.GetNamedDataSlot("get_Result"), null);
                }
            })) { IsBackground = true };
            this.GetThread.SetApartmentState(ApartmentState.STA);
            #endregion
        }
        protected object this[bool copy, int retryTimes = 10, int retryDelay = 100]
        {
            get
            {
                object data = null;
                if (!this.GetThread.IsAlive)
                {
                    Thread.SetData(Thread.GetNamedDataSlot("get_retryDelay"), retryDelay);
                    this.GetThread.Start(retryTimes);
                    data = Thread.GetData(Thread.GetNamedDataSlot("get_Result"));
                }
                Contract.Requires<NullReferenceException>(data != null);
                return data;
            }
            set
            {
                if (!this.SetThread.IsAlive)
                {
                    Thread.SetData(Thread.GetNamedDataSlot("set_retryTimes"), retryTimes);
                    Thread.SetData(Thread.GetNamedDataSlot("set_retryDelay"), retryDelay);
                    Thread.SetData(Thread.GetNamedDataSlot("set_copy"), copy);
                    this.SetThread.Start(value);
                }
            }
        }
        public object this[byte normaldequeue]
        {
            get
            {
                object data = null;
                if (normaldequeue == 1)
                    this.i_AsyncArgs.TryPeek(out data);
                else
                    this.i_AsyncArgs.TryDequeue(out data);
                return data;
            }
        }
        private bool IsFormatValid(DataObject data)
        {
            return IsFormatValid(data.GetFormats());
        }
        private bool IsFormatValid(string[] formats)
        {
            Contract.Assert(formats != null, "Null returned from GetFormats");
            if (formats != null)
            {
                if (formats.Length <= 4)
                {
                    for (int i = 0; i < formats.Length; i++)
                    {
                        switch (formats[i])
                        {
                            case "Text":
                            case "UnicodeText":
                            case "System.String":
                            case "Csv":
                                break;
                            default:
                                return false;

                        }
                    }
                    return true;
                }
            }
            return false;
        }
        private bool IsFormatValid(FORMATETC[] formats)
        {
            Contract.Assert(formats != null, "Null returned from GetFormats");
            if (formats != null)
            {
                if (formats.Length <= 4)
                {
                    for (int i = 0; i < formats.Length; i++)
                    {
                        short format = formats[i].cfFormat;
                        if (format != 1 && //  CF_TEXT
                            format != 13 && //   CF_UNICODETEXT
                            format != DataFormats.GetFormat("System.String").Id &&
                            format != DataFormats.GetFormat("Csv").Id)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        private void ClipBoardCon_OnClipBoardChangeIO(object sender, ClipBoardConEventArgs e)
        {
            Contract.Requires<NullReferenceException>(e != null);
            Contract.Requires<InvalidCastException>(e != EventArgs.Empty);
            switch(e.OPType)
            {
                case ClipBoardOPType.Get:
                    {
                        i_AsyncArgs.Enqueue(e.DataContent);
                        break;
                    }
                case ClipBoardOPType.Set:
                    {
                        this[false] = e.DataContent;
                        break;
                    }
                default: break;
            }
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        protected override void WndProc(ref Message m)
        {
            switch ((ClipBoardHandleOptions)m.Msg)
            {
                case ClipBoardHandleOptions.WM_CREATE:
                    {
                        this.hWndNextWindow = ClipBoardConHelpers.SetClipboardViewer(this.Handle);
                        break;
                    }
                case ClipBoardHandleOptions.WM_CHANGECBCHAIN:
                    {
                        if (m.WParam == hWndNextWindow)
                        {
                            hWndNextWindow = m.LParam;
                        }
                        else if (hWndNextWindow != IntPtr.Zero)
                        {
                            ClipBoardConHelpers.SendMessage(hWndNextWindow, m.Msg, m.WParam, m.LParam);
                        }
                        break;
                    }
                case ClipBoardHandleOptions.WM_DRAWCLIPBOARD:
                    {
                        this.OnClipBoardChangeIO.Invoke(this, new ClipBoardConEventArgs(ClipBoardOPType.Get, this[false]));
                        ClipBoardConHelpers.SendMessage(hWndNextWindow, m.Msg, m.WParam, m.LParam);
                        break;
                    }
                case ClipBoardHandleOptions.WM_DESTROY:
                    {
                        ClipBoardConHelpers.ChangeClipboardChain(this.Handle, hWndNextWindow);
                        break;
                    }
            }
            base.WndProc(ref m);
        }
    }
}
