using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading.CustomCon
{
    public delegate void CustomConsoleKeyPressedEventHandler(object sender, CustomConsoleKeyPressedEventArgs args);
    public class CustomConsoleKeyPressedEventArgs : EventArgs
    {
        public readonly ConsoleKey KeyInfo;
        public readonly ConsoleModifiers Modifiers;
        public CustomConsoleKeyPressedEventArgs(ConsoleKey key, ConsoleModifiers mod)
        {
            this.KeyInfo = key;
            this.Modifiers = mod;
        }
    }
}
