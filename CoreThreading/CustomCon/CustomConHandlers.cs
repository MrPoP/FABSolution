using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading.CustomCon
{
    public static partial class CustomConHandlers
    {
        public delegate Task ProcessHandlers(CustomConsole console, string arg);
        [CustomConHandle("help")]
        public static async Task Handlehelp(CustomConsole console, string arg)
        {
            string[] args = arg.Split(' ');
            switch(args[0])
            {
                case "help":
                case "HELP":
                case "Help":
                    {
                        if (args[1] != null)
                        {
                            switch (args[1])
                            {
                                case "closekey":
                                case "CLOSEKEY":
                                case "Closekey":
                                    {
                                        goto SHOWHELPCLOSEKEY;
                                    }
                                default:
                                    await console.Write("This command is not supported by the help utility.");
                                    return;
                            }
                        }
                        await console.Write("For more information on a specific command, type HELP command-name");
                        goto SHOWMODIFIERS;
                    }
                default:
                    await console.Write("'{0}' is not recognized as an internal or external command.", args[0]);
                    break;
            }
        SHOWHELPCLOSEKEY:
            {
                await console.Write("Modifies console close key");
                await console.Write(" ");
                await console.Write(" closekey [KeyFlag]");
                await console.Write(" [KeyName]          [KeyFlag]");
                for (byte x = 8; x < 255; x++)
                {
                    await console.Write(" {0}          {1}", (ConsoleKey)x, x);
                }
                using (TextReader reader = console.Reader)
                {
                    string input = reader.ReadLine();
                    byte val = 0;
                    if (byte.TryParse(input, out val))
                    {
                        await console.Write("Selected keyflag is {0}", (ConsoleKey)val);
                        console.Config.Set("CloseKey", val);
                    }
                }
                return;
            }
            SHOWMODIFIERS:
            {
                await console.Write("CLOSEKEY          Selecting close key for the console to shut down when pressed.");
                await console.Write("For more information on tools see the command-line reference in the online help.");
                return;
            }
        }
        [CustomConHandle("closekey")]
        public static async Task HandleCloseKey(CustomConsole console, string arg)
        {
            string[] args = arg.Split(' ');
            await console.Write("Modifies console close key");
            await console.Write(" ");
            await console.Write(" closekey [KeyFlag]");
            await console.Write(" [KeyName]          [KeyFlag]");
            for (byte x = 8; x < 255; x++)
            {
                await console.Write(" {0}          {1}", (ConsoleKey)x, x);
            }
            using (TextReader reader = console.Reader)
            {
                string input = await reader.ReadLineAsync();
                byte val = 0;
                if (byte.TryParse(input, out val))
                {
                    await console.Write("Selected keyflag is {0}", (ConsoleKey)val);
                    console.Config.CloseKey = (ConsoleKey)val;
                }
                else
                {
                    await console.Write("Couldn't recognize value.");
                }
            }
            return;
        }
    }
}
