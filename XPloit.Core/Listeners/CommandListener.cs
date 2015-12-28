using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using XPloit.Core.Collections;
using XPloit.Core.Command;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;
using XPloit.Core.Multi;
using XPloit.Res;

namespace XPloit.Core.Listeners
{
    public class CommandListener : IListener
    {
        CommandMenu _Command = null;
        bool _IsStarted;

        Exploit _Current = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public CommandListener(ICommandLayer command)
        {
            _Command = new CommandMenu(command);

            _Command.Add(new string[] { "banner" }, cmdBanner, "Show a beautifull banner");
            _Command.Add(new string[] { "clear" }, cmdClear, "Clear console");
            _Command.Add(new string[] { "cd", "cd..", "cd\\", "cd/", }, cmdCD, "Unuse exploit");

            // Modules command
            _Command.Add(new string[] { "use" }, cmdUse, "Use a xploit module");
            _Command.Add(new string[] { "show" }, null, "show [options] for current exploit");
            _Command.Add(new string[] { "set" }, null, "set [variable] [value] for current exploit");
            _Command.Add(new string[] { "search" }, null, "set [variable] [value] for current exploit");
            _Command.Add(new string[] { "check" }, null, "check the current module");
            _Command.Add(new string[] { "exploit", "run" }, null, "run the current module");
        }
        public void cmdClear(string args)
        {
            _Command.IO.Clear();
        }
        public void cmdCD(string args)
        {
            _Current = null;
            _Command.PromptCharacter = "> ";
        }
        public void cmdBanner(string args)
        {
            _Command.IO.WriteLine("");
            BannerHelper.GetRandomBanner(_Command.IO);
            _Command.IO.WriteLine("");
        }
        public void cmdUse(string args)
        {
            _Current = ExploitCollection.Current.GetByFullPath(args);

            if (_Current == null) WriteError(Lang.Get("Module_Not_Found", args));
            else
            {
                _Command.PromptCharacter = "(" + _Current.Name + ")> ";
            }
        }

        void WriteError(string error)
        {
            _Command.IO.SetForeColor(ConsoleColor.Red);
            _Command.IO.WriteLine(error);
        }

        public override bool IsStarted { get { return _IsStarted; } }
        public override bool Start()
        {
            _IsStarted = true;

            _Command.IO.AddInput("banner");
            //cmdBanner("");

            _Command.Run();

            return _IsStarted;
        }
        public override bool Stop()
        {
            _Command.Quit();
            _IsStarted = false;
            return true;
        }
    }
}