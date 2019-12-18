using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CoreDataStore;
using CoreThreading.CustomCon;

namespace TestProject
{
    /*[CoreStruct("User", Crypted=true)]
    public class User
    {
        [CoreProperty("User", "UID", (int)0, AutoIncreasment = true, Unique = true)]
        public int UID { get; set; }
        [CoreProperty("User", "UserName", "", Unique = true)]
        public string UserName { get; set; }
        [CoreProperty("User", "UserName", "")]
        public string PassWord { get; set; }
    }
    class Program
    {
        [Property("b1e461e0-eb0b-413a-8f11-b8b377548a65", typeof(string))]
        static string POP { get { return "MahmoudPOP."; } }
        static void Main(string[] args)
        {
            Resolver.Start();
        }
        [Method("b1e461e0-eb0b-413a-8f11-b8b377548a65", 20, ThreadFlag = ThreadType.Invocator)]
        public static void Invoker(string str)
        {
            Console.WriteLine(str);
        }
    }*/
    class Program
    {
        static void Main(string[] args)
        {
            new MyConsole().HandleAsyncCommand(Console.ReadLine());
        }
    }
    public class MyConsole : CustomConsole
    {
        public MyConsole()
            : base()
        {
        }
        public override async System.Threading.Tasks.Task HandleAsyncCommand(string str)
        {
            await base.HandleAsyncCommand(str);
        }

        public override async System.Threading.Tasks.Task Write(string msg)
        {
            await base.Write((object)msg);
        }
    }
}
