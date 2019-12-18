using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading.CustomCon
{
    public class CustomConHandleAttribute : Attribute
    {
        public static Func<CustomConHandleAttribute, int> Translator = (st) => st.ExceutingCommand.GetHashCode();
        private readonly string excutingCommand;
        public CustomConHandleAttribute(string command)
        {
            this.excutingCommand = command;
        }
        public string ExceutingCommand
        {
            get
            {
                return this.excutingCommand;
            }
        }
    }
}
