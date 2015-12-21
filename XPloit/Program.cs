using System;
using System.Collections.Generic;
using XPloit.Configs;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;
using XPloit.Core.Listeners;
using XPloit.Core.Multi;
using XPloit.Res;

namespace XPloit
{
    class Program
    {
        static int Main(string[] args)
        {
            // Configure
            //Console.InputEncoding = Encoding.UTF8;
            //Console.OutputEncoding = Encoding.UTF8;
            Console.CursorSize = 100;
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            // TODO: Fix \"CryptKey=#Crypt0 M3#\" -> broken line whith white space
            // \"CryptKey=#Crypt0M3#\" 
            Config cfg = ArgumentHelper.Parse<Config>("\"Listen={Port=23 CryptKey=#Test# IPFilter={OnlyAllowed=127.0.0.1,172.22.32.51}}\" \"User={UserName=root Password=toor}\"");

            ConsoleCommand command = new ConsoleCommand();

            if (cfg.Connect != null)
            {
                // Connect to server
                SocketListener client = new SocketListener(cfg.Connect);

                command.SetForeColor(ConsoleColor.Gray);
                command.Write(Lang.Get("Connecting_To", client.ToString()));
                if (client.Start())
                {
                    command.SetForeColor(ConsoleColor.Green);
                    command.Write(Lang.Get("Ok").ToUpperInvariant(), true);
                }
                else
                {
                    command.SetForeColor(ConsoleColor.Red);
                    command.Write(Lang.Get("Error").ToUpperInvariant(), true);
                }

                command.SetForeColor(ConsoleColor.DarkGray);
                command.Write(Lang.Get("Press_Any_Key"), true);

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
                        command.Write(Lang.Get("Ok").ToUpperInvariant(), true);
                    }
                    else
                    {
                        command.SetForeColor(ConsoleColor.Red);
                        command.Write(Lang.Get("Error").ToUpperInvariant(), true);
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