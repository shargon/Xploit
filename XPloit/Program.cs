using System;
using System.Collections.Generic;
using XPloit.Configs;
using XPloit.Core.Command;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;
using XPloit.Core.Listeners;
using XPloit.Modules.Payloads.Multi;
using XPloit.Res;

namespace XPloit
{
    class Program
    {
        static int Main(string[] args)
        {
            ProcessStartPayload p = new ProcessStartPayload();

            // Configure
            //Console.InputEncoding = Encoding.UTF8;
            //Console.OutputEncoding = Encoding.UTF8;
            ConsoleCommand command = new ConsoleCommand();
            command.AddInput("use Auxiliary/Local/System");
            //command.AddInput("version");
            command.AddInput("banner");

            //command.AddInput("show options");

            Console.CursorSize = 100;
            Console.CursorVisible = false;

            command.SetBackgroundColor(ConsoleColor.White);
            command.SetBackgroundColor(ConsoleColor.Black);

            // TODO: Fix \"CryptKey=#Crypt0 M3#\" -> broken line whith white space
            // \"CryptKey=#Crypt0M3#\" 
            Config cfg = ArgumentHelper.Parse<Config>("\"Listen={Port=23 CryptKey=#Test# IPFilter={OnlyAllowed=127.0.0.1,172.22.32.51}}\" \"User={UserName=root Password=toor}\"");

            if (cfg.Connect != null)
            {
                // Connect to server
                SocketListener client = new SocketListener(cfg.Connect);

                command.SetForeColor(ConsoleColor.Gray);
                command.Write(Lang.Get("Connecting_To", client.ToString()));
                if (client.Start())
                {
                    command.SetForeColor(ConsoleColor.Green);
                    command.WriteLine(Lang.Get("Ok").ToUpperInvariant());
                }
                else
                {
                    command.SetForeColor(ConsoleColor.Red);
                    command.WriteLine(Lang.Get("Error").ToUpperInvariant());
                }

                command.SetForeColor(ConsoleColor.DarkGray);
                command.WriteLine(Lang.Get("Press_Any_Key"));

                Console.ReadKey();
            }
            else
            {
                List<IListener> listeners = new List<IListener>();

                // Launch socket listener
                if (cfg.Listen != null) listeners.Add(new SocketListener(cfg.Listen));

                // Run listeners
                foreach (IListener listener in listeners)
                {
                    command.SetForeColor(ConsoleColor.Gray);
                    command.Write(Lang.Get("Starting_Listener", listener.ToString()));

                    if (listener.Start())
                    {
                        command.SetForeColor(ConsoleColor.Green);
                        command.WriteLine(Lang.Get("Ok").ToUpperInvariant());
                    }
                    else
                    {
                        command.SetForeColor(ConsoleColor.Red);
                        command.WriteLine(Lang.Get("Error").ToUpperInvariant());
                    }
                }

                // Console listener
                CommandListener cmd = new CommandListener(command);
                cmd.Start();
            }

            // Wait exit signal
            return 0;
        }
    }
}