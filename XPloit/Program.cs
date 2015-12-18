using System;
using System.Collections.Generic;
using XPloit.Configs;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;
using XPloit.Core.Listeners;
using XPloit.Res;

namespace XPloit
{
    class Program
    {
        /// <summary>
        /// Set color for console
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="isForeGround">Is for foreground or for background</param>
        /// <returns>Returns null</returns>
        static string Color2Console(ConsoleColor color, bool isForeGround)
        {
            if (isForeGround) Console.ForegroundColor = color;
            else Console.BackgroundColor = color;
            return null;
        }
        static int Main(string[] args)
        {
            // Configure
            //Console.InputEncoding = Encoding.UTF8;
            //Console.OutputEncoding = Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            // TODO: Fix \"CryptKey=#Crypt0 M3#\" -> broken line whith white space
            // \"CryptKey=#Crypt0M3#\" 
            Config cfg = ArgumentHelper.Parse<Config>("\"TelnetInterface={ListenPort=23 IPFilter={OnlyAllowed=127.0.0.1,172.22.32.51}}\" \"User={UserName=root Password=toor}\"");

            List<IListener> listeners = new List<IListener>();

            // Lauch Telnet
            if (cfg.TelnetInterface != null) listeners.Add(new TelnetListener(cfg.TelnetInterface));

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
            StreamListener cmd = new StreamListener(Console.OutputEncoding, Console.Out, Console.In, Color2Console);
            cmd.Start();

            // Wait exit signal
            return 0;
        }
    }
}