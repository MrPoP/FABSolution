using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CustomConsoleContainer = CoreThreading.Collections.Generic.AttributeInvocationMapper<CoreThreading.CustomCon.CustomConHandlers.ProcessHandlers, CoreThreading.CustomCon.CustomConHandleAttribute, int>;

namespace CoreThreading.CustomCon
{
    [Serializable]
    public abstract class CustomConsole : ISerializable, IDeserializationCallback
    {
        private readonly char[] LoadingSymbols = new char[] { '|', '/', '-', '\\', '|', '/', '-', '\\' };
        private readonly CustomConsoleContainer _methods;
        protected ClipBoardCon Clip = null;
        private event CustomConsoleKeyPressedEventHandler _keyPressed;
        private TaskCompletionSource<CustomConsoleKeyPressedEventArgs> _keyPressing;
        public ConsoleConfig Config = ConsoleConfig.Default;
        public TextReader Reader { get { return Console.In; } }
        public TextWriter Writer { get { return Console.Out; } }
        public TextWriter Error { get { return Console.Error; } }
        public bool CapsLock { get { return Console.CapsLock; } }
        public bool NumberLock { get { return Console.NumberLock; } }
        private ConcurrentQueue<string> _commands;
        #region Serialization
        [NonSerialized]
        private SerializationInfo _siInfo;
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            this.GetObjectData(info, context);
        }
        public void OnDeserialization(object sender)
        {
            Debug.Assert(this._siInfo == null, "Null value exception", string.Format("{0}"), new ArgumentNullException("info"));
            this.Config.OnDeserialization(sender);
        }
        #endregion
        #region Constructors
        public CustomConsole()
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            this._keyPressing = new TaskCompletionSource<CustomConsoleKeyPressedEventArgs>();
            if (Config.Commands)
                this._keyPressed = new CustomConsoleKeyPressedEventHandler(HandleKeyPress);
            if (Config.AccessClipBoard)
                this.Clip = new ClipBoardCon();
            this._commands = new ConcurrentQueue<string>();
            this._methods = new CustomConsoleContainer(CustomConHandleAttribute.Translator);
        }
        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            ConsoleKeyInfo key = default(ConsoleKeyInfo);
            if(!Console.KeyAvailable)
            {
                key = Console.ReadKey(true);
                if (this._keyPressed != null)
                {
                    this._keyPressed.Invoke(this, new CustomConsoleKeyPressedEventArgs(key.Key, key.Modifiers));
                }
                if (Config.CloseKey == key.Key)
                {
                    Environment.Exit(0);
                }
            }
        }
        protected CustomConsole(SerializationInfo info, StreamingContext context)
            :this()
        {
            this._siInfo = info;
        }
        #endregion
        #region Methods
        protected async void HandleKeyPress(object sender, CustomConsoleKeyPressedEventArgs args)
        {
            switch(args.KeyInfo)
            {
                case ConsoleKey.C:
                    {
                        if (sender is CustomConsole)
                        {
                            if ((sender as CustomConsole).Config.AccessClipBoard)
                            {
                                if(args.Modifiers == ConsoleModifiers.Control)
                                {
                                    //
                                }
                            }
                        }
                        break;
                    }
                case ConsoleKey.V:
                    {
                        if (sender is CustomConsole)
                        {
                            if ((sender as CustomConsole).Config.AccessClipBoard)
                            {
                                if (args.Modifiers == ConsoleModifiers.Control)
                                {

                                }
                            }
                        }
                        break;
                    }
            }
            await Task.Run(() => Console.In.ReadLineAsync()).ContinueWith((s) => enququCommands(s.Result));
            
        }
        protected async Task enququCommands(params string[] args)
        {
            if (args.Length > 0)
            {
                var commands = new List<Task>();
                foreach (string str in args)
                    commands.Add(innerCommandsHandler(str));
                await Task.WhenAll(commands);
            }
        }
        protected async Task innerCommandsHandler(string Command)
        {
            string[] args = Command.Split(' ');
            CoreThreading.CustomCon.CustomConHandlers.ProcessHandlers delegatetoexcute;
            if (_methods.TryGetValue(args[0].GetHashCode(), out delegatetoexcute))
            {
                await delegatetoexcute.Invoke(this, Command);
            }
            else
            {
                await Write("'{0}' is not recognized as an internal or external command.", args[0]);
            }
            await Task.Delay(10).ConfigureAwait(false);
        }
        public virtual async Task HandleAsyncCommand(string str)
        {
            await enququCommands(str);
        }
        protected virtual async Task WriteLog(object exception) 
        { 
            await WriteLog((Exception)exception);
        }
        protected virtual async Task WriteLog(Exception exception)
        {
            await WriteLog(string.Format("Exception Message : [{0}] Full Exception Data : {1}", exception.Message, exception));
        }
        protected virtual async Task WriteLog(string msg, params object[] args)
        {
            await WriteLog(string.Format(msg, args));
        }
        protected async Task WriteLog(string msg)
        {
            Contract.Requires<NullReferenceException>(msg == null);
            if (!File.Exists(this.Config.LogFile))
                File.Create(this.Config.LogFile).Dispose();
            using (StreamWriter stream = new StreamWriter(File.Open(this.Config.LogFile, FileMode.Append, FileAccess.Write)))
            {
                await stream.WriteLineAsync(msg);
                stream.Close();
            }
            await Task.Delay(10).ConfigureAwait(false);
        }
        public async virtual Task Write(object message)
        {
            using(var writer = Console.Out)
            {
                if (message is string)
                {
                    await writer.WriteLineAsync(message as string);
                    await WriteLog(message as string);
                }
                else if (message is ushort)
                {
                    await writer.WriteLineAsync("Ushort " + message.ToString());
                    await WriteLog("Ushort " + message.ToString());
                }
                else if (message is short)
                {
                    await writer.WriteLineAsync("Short " + message.ToString());
                    await WriteLog("Short " + message.ToString());
                }
                else if (message is float)
                {
                    await writer.WriteLineAsync("Float " + message.ToString());
                    await WriteLog("Float " + message.ToString());
                }
                else if (message is decimal)
                {
                    await writer.WriteLineAsync("Decimal " + message.ToString());
                    await WriteLog("Decimal " + message.ToString());
                }
                else if (message is char)
                {
                    await writer.WriteAsync("Char " + message.ToString());
                    await WriteLog("Char " + message.ToString());
                }
                else if (message is sbyte)
                {
                    await writer.WriteLineAsync("Sbyte " + message.ToString());
                    await WriteLog("Sbyte " + message.ToString());
                }
                else if (message is byte)
                {
                    await writer.WriteLineAsync("Byte " + message.ToString());
                    await WriteLog("Byte " + message.ToString());
                }
                else if (message is ulong)
                {
                    await writer.WriteLineAsync("Ulong " + message.ToString());
                    await WriteLog("Ulong " + message.ToString());
                }
                else if (message is long)
                {
                    await writer.WriteLineAsync("Long " + message.ToString());
                    await WriteLog("Long " + message.ToString());
                }
                else if (message is uint)
                {
                    await writer.WriteLineAsync("Uint " + message.ToString());
                    await WriteLog("Uint " + message.ToString());
                }
                else if (message is int)
                {
                    await writer.WriteLineAsync("Int " + message.ToString());
                    await WriteLog("Int " + message.ToString());
                }
                else if (message is bool)
                {
                    await writer.WriteLineAsync("Boolean " + ((bool)message == true ? "TRUE" : "FALSE"));
                    await WriteLog("Boolean " + ((bool)message == true ? "TRUE" : "FALSE"));
                }
                else if (message is byte[])
                {
                    byte[] Data = (message as byte[]);
                    int PacketLength = Data.Length;
                    string DataStr = string.Format("Byte Array With Length {0} bits.", PacketLength);
                    DataStr += Environment.NewLine;
                    for (int i = 0; i < Math.Ceiling((double)PacketLength / 16); i++)
                    {
                        int t = 16;
                        if (((i + 1) * 16) > PacketLength)
                            t = PacketLength - (i * 16);
                        for (int a = 0; a < t; a++)
                        {
                            DataStr += Data[i * 16 + a].ToString("X2") + " ";

                        }
                        if (t < 16)
                            for (int a = t; a < 16; a++)
                                DataStr += "   ";
                        DataStr += "     ;";

                        for (int a = 0; a < t; a++)
                        {
                            DataStr += Convert.ToChar(Data[i * 16 + a]);
                        }
                        DataStr += Environment.NewLine;
                    }
                    DataStr.Replace(Convert.ToChar(0), '.');
                    await writer.WriteLineAsync(DataStr);
                    await WriteLog(DataStr);
                }
                else
                {
                    await writer.WriteLineAsync("Object " + message.ToString());
                    await WriteLog("Object " + message.ToString());
                }
                await writer.FlushAsync();
            }
        }
        public async virtual Task Write(string message, params object[] args)
        {
            await Write(string.Format(message, args));
        }
        public async virtual Task Write(Exception exception)
        {
            await Write("Exception Message : [{0}] Full Exception Data : {1}", exception.Message, exception);
        }
        public abstract Task Write(string msg);
        #endregion
    }
}
