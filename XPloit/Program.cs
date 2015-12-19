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
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            // TODO: Fix \"CryptKey=#Crypt0 M3#\" -> broken line whith white space
            // \"CryptKey=#Crypt0M3#\" 
            Config cfg = ArgumentHelper.Parse<Config>("\"SocketInterface={ListenPort=23 CryptKey=#Test# IPFilter={OnlyAllowed=127.0.0.1,172.22.32.51}}\" \"User={UserName=root Password=toor}\"");

            List<IListener> listeners = new List<IListener>();

            // Lauch Telnet
            if (cfg.SocketInterface != null) listeners.Add(new SocketListener(cfg.SocketInterface, true));

            // Run listeners
            foreach (IListener l in listeners)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(Lang.Get("Starting_Listener", l.ToString()));

                if (l.Start())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(Lang.Get("Ok").ToUpperInvariant());
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Lang.Get("Error").ToUpperInvariant());
                }
            }

            // Console listener
            CommandListener cmd = new CommandListener(new ConsoleCommand());
            cmd.Start();

            // Wait exit signal
            return 0;
        }
    }
}