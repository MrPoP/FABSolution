using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace NetCore.FormProtocol
{
    public static class ProtocolInvokations
    {
        public static EventHandler GotFocus { get { return new EventHandler(GetFocusMethod); } }

        public static EventHandler TextChanged { get { return new EventHandler(TextChangedMethod); } }

        public static EventHandler LostFocus { get { return new EventHandler(LostFocusMethod); } }

        public static MouseEventHandler MouseClick { get { return new MouseEventHandler(MouseClickMethod); } }

        public static EventHandler Validated { get { return new EventHandler(ValidatedMethod); } }

        public static CancelEventHandler Validating { get { return new CancelEventHandler(ValidatingMethod); } }

        static void GetFocusMethod(object sender, EventArgs args)
        {
            ProtocolContract Contract;
            if ((sender is ProtocolContract) && args == EventArgs.Empty)
            {//serverRespond
                Contract = (ProtocolContract)sender;
                if (Contract.ErrorMessage != null)
                {
                }
                return;
            }
            //clientInvokaction
            Contract = (sender as Control);
            if (Contract < (long)0)
            {
                Contract.Flag = ContractFlag.GotFocus;
            }
        }
        static void TextChangedMethod(object sender, EventArgs args)
        {
            if ((sender is ProtocolContract) && args == EventArgs.Empty)
            {//serverRespond
                return;
            }
            //clientInvokaction
            ProtocolContract Contract = (sender as Control);
            if (Contract < (long)0)
            {
                Contract.Flag = ContractFlag.TextChanged;
            }
        }
        static void LostFocusMethod(object sender, EventArgs args)
        {
            if ((sender is ProtocolContract) && args == EventArgs.Empty)
            {//serverRespond
                return;
            }
            //clientInvokaction
            ProtocolContract Contract = (sender as Control);
            if (Contract < (long)0)
            {
                Contract.Flag = ContractFlag.LostFocus;
            }
        }
        static void MouseClickMethod(object sender, MouseEventArgs args)
        {
            if ((sender is ProtocolContract) && args == EventArgs.Empty)
            {//serverRespond
                return;
            }
            //clientInvokaction
            ProtocolContract Contract = (sender as Control);
            if (Contract < (long)0)
            {
                Contract.Flag = ContractFlag.MouseClick;
            }
        }
        static void ValidatedMethod(object sender, EventArgs args)
        {
            if ((sender is ProtocolContract) && args == EventArgs.Empty)
            {//serverRespond
                return;
            }
            //clientInvokaction
            ProtocolContract Contract = (sender as Control);
            if (Contract < (long)0)
            {
                Contract.Flag = ContractFlag.Validated;
            }
        }
        static void ValidatingMethod(object sender, CancelEventArgs args)
        {
            if ((sender is ProtocolContract) && args == EventArgs.Empty)
            {//serverRespond
                return;
            }
            //clientInvokaction
            ProtocolContract Contract = (sender as Control);
            if (Contract < (long)0)
            {
                Contract.Flag = ContractFlag.Validating;
            }
        }
    }
}
