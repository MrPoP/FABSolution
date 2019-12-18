using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading
{
    public static class Class1
    {
        [Property("b1e461e0-eb0b-413a-8f11-b8b377548a65", typeof(string))]
        static string POP { get { return "MahmoudPOP."; } }
        public static void Main(string[] args)
        {
            Console.WriteLine(Guid.NewGuid());
            //Console.Read();
        }
        [Method("b1e461e0-eb0b-413a-8f11-b8b377548a65", 20, ThreadFlag = ThreadType.Invocator)]
        public static void Invoker(string str)
        {
            Console.WriteLine(str);
        }
    }
}
