using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NetCore.FormProtocol
{
    public struct ProtocolContract
    {
        public static readonly ProtocolContract Default = default(ProtocolContract);

        public long Time;
        public ContractFlag Flag;
        public int ID;
        public string Value;
        public List<int> ResetContents;
        public string ErrorMessage;

        public static ProtocolContract Create(long _time, ContractFlag _flag, int _ID, string _value = null, string _errormsg = null)
        {
            return new ProtocolContract()
            {
                Time = _time,
                Flag = _flag,
                ID = _ID,
                Value = _value,
                ErrorMessage = _errormsg,
                ResetContents =new List<int>()
            };
        }
        public static ProtocolContract Create(params int[] _contents)
        {
            return new ProtocolContract()
            {
                ResetContents = _contents.ToList()
            };
        }

        public static implicit operator ProtocolContract(Control control)
        {
            return ProtocolContract.Create(0, ContractFlag.Ping, (int)control.Tag, control.Text);
        }

        public static bool operator <(ProtocolContract contract, long time)
        {
            contract.Time = time;
            return true;
        }
        public static bool operator >(ProtocolContract contract, long time)
        {
            time = contract.Time;
            return true;
        }
    }
}
