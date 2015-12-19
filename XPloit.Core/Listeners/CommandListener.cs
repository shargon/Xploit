using System;
using System.Globalization;
using XPloit.Core.Interfaces;
using XPloit.Res;

namespace XPloit.Core.Listeners
{
    public class CommandListener : IListener
    {
        const string CommandStart = " > ";

        ICommandLayer _Command;
        bool _IsStarted;

        /// <summary>
        /// Constructor
        /// </summary>
        public CommandListener(ICommandLayer command) { _Command = command; }
        public override bool IsStarted { get { return _IsStarted; } }
        public override bool Start()
        {
            _IsStarted = true;

            _Command.SetForeColor(ConsoleColor.Green);
            _Command.Write(Lang.Get("Wellcome"), true);

            //Menu[] menus = new Menu[]
            //{
            //};

            string read;
            do
            {
                CultureInfo c = CultureInfo.CurrentCulture;
                _Command.SetForeColor(ConsoleColor.White);
                _Command.Write(CommandStart, false);
                _Command.SetForeColor(ConsoleColor.White);
                read = _Command.ReadLine();
                if (read == null) continue;

                read = read.Trim();

                if (read.StartsWith("cd ", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (string.Compare("cd ..", read, true) == 0)
                    {
                        // Down one level
                        continue;
                    }
                    else if (string.Compare("cd /", read, true) == 0 || string.Compare("CD \\", read, true) == 0)
                    {
                        // Down to main
                        continue;
                    }
                }

                if (string.Compare("exit", read, true) != 0)
                {
                    _Command.SetForeColor(ConsoleColor.Red);
                    _Command.Write(Lang.Get("Unknown_Command"), true);
                }
            }
            while (string.Compare("exit", read, true) != 0);

            return _IsStarted;
        }
        public override bool Stop()
        {
            _IsStarted = false;
            return true;
        }
    }
}