using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using XPloit.Core.Collections;
using XPloit.Core.Command;
using XPloit.Core.Command.Interfaces;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;
using XPloit.Core.PayloadRequirements;
using XPloit.Res;

namespace XPloit.Core.Listeners
{
    public class CommandListener : IListener, IAutoCompleteSource
    {
        CommandMenu _Command = null;
        bool _IsStarted;

        Module _Current = null;

        #region IAutoCompleteSource
        public IEnumerable<string> GetCommand()
        {
            foreach (CommandMenuItem ci in _Command)
                foreach (string s in ci.Selector)
                    yield return s;
        }
        public IEnumerable<string> GetArgument(string command, string[] arguments)
        {
            switch (command.ToLowerInvariant().Trim())
            {
                case "use":
                    {
                        foreach (Module e in ModuleCollection.Current) yield return e.FullPath;
                        break;
                    }
                case "show":
                    {
                        if (arguments == null || arguments.Length <= 1)
                        {
                            foreach (string e in new string[] { "options", "config", "payloads", "targets" }) yield return e;
                        }
                        break;
                    }
                case "set":
                    {
                        if (_Current == null) break;

                        if (arguments == null || arguments.Length <= 1)
                        {
                            foreach (PropertyInfo pi in _Current.GetProperties(true, true, false))
                                yield return pi.Name;
                        }
                        else
                        {
                            switch (arguments[0].ToLowerInvariant().Trim())
                            {
                                case "payload":
                                    {
                                        IPayloadRequirements req = _Current.PayloadRequirements;
                                        if (req != null && !(req is NoPayloadRequired))
                                        {
                                            foreach (Payload p in PayloadCollection.Current)
                                            {
                                                if (!req.IsAllowedPayload(p)) continue;
                                                yield return p.FullPath;
                                            }
                                        }
                                        break;
                                    }
                            }
                        }
                        break;
                    }
            }
        }
        public StringComparison ComparisonMethod { get { return StringComparison.InvariantCultureIgnoreCase; } }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public CommandListener(ICommandLayer command)
        {
            _Command = new CommandMenu(command, this);

            _Command.Add(new string[] { "banner" }, cmdBanner, Lang.Get("Man_Banner"));
            _Command.Add(new string[] { "clear" }, cmdClear, Lang.Get("Man_Clear"));
            _Command.Add(new string[] { "cd", "cd..", "cd\\", "cd/" }, cmdCD, "Un-use the current module");

            // Modules command
            _Command.Add(new string[] { "use" }, cmdUse, Lang.Get("Man_Use"));
            _Command.Add(new string[] { "show" }, cmdShow, Lang.Get("Man_Show"));


            _Command.Add(new string[] { "set" }, cmdSet, "set [variable] [value] for current exploit");
            _Command.Add(new string[] { "search" }, null, "set [variable] [value] for current exploit");
            _Command.Add(new string[] { "check" }, null, "check the current module");
            _Command.Add(new string[] { "exploit", "run" }, null, "run the current module");
        }
        public void cmdClear(string args) { _Command.IO.Clear(); }
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
        public void cmdSet(string args)
        {
            args = args.Trim();
            if (_Current == null)
            {
                _Command.IO.WriteLine(Lang.Get("Require_Module"));
                return;
            }
        }
        public void cmdShow(string args)
        {
            // Todo comentar en Res.res los comandos y acabar del set para abajo
            args = args.Trim().ToLowerInvariant();
            if (_Current == null)
            {
                _Command.IO.WriteLine(Lang.Get("Require_Module"));
                return;
            }

            switch (args)
            {
                case "":
                case "options":
                case "config":
                    {
                        CommandTable tb = new CommandTable();

                        tb.AddRow(Lang.Get("Name"), _Current.Name, "");
                        tb.AddRow("", "");

                        if (!string.IsNullOrEmpty(_Current.Author)) tb.AddRow(Lang.Get("Author"), _Current.Author, "");
                        if (!string.IsNullOrEmpty(_Current.Description)) tb.AddRow(Lang.Get("Description"), _Current.Description, "");
                        if (_Current.DisclosureDate != DateTime.MinValue) tb.AddRow(Lang.Get("DisclosureDate"), _Current.DisclosureDate, "");
                        tb.AddRow(Lang.Get("IsLocal"), _Current.IsLocal, "");
                        tb.AddRow(Lang.Get("IsRemote"), _Current.IsRemote, "");

                        if (_Current.References != null && _Current.References.Length > 0)
                        {
                            tb.AddRow("", "", "");

                            StringBuilder sb = new StringBuilder();
                            foreach (Reference r in _Current.References)
                            {
                                if (r == null) continue;
                                sb.AppendLine(r.Type.ToString() + " - " + r.Value);
                            }

                            tb.AddRow(Lang.Get("References"), sb.ToString(), "");
                        }

                        tb.AddRow("", "", "");
                        if (_Current.Target == null)
                            tb.AddRow(Lang.Get("Target"), "NULL", "")[1].ForeColor = ConsoleColor.Red;
                        else
                            tb.AddRow(Lang.Get("Target"), _Current.Target.Name, "");

                        if (_Current.Payload == null)
                            tb.AddRow(Lang.Get("Payload"), "NULL", "")[1].ForeColor = ConsoleColor.Red;
                        else
                            tb.AddRow(Lang.Get("Payload"), _Current.Payload.FullPath, "");

                        bool primera = true;
                        foreach (PropertyInfo pi in _Current.GetProperties(true, true, true))
                        {
                            if (primera)
                            {
                                tb.AddRow("", "", "");
                                primera = false;
                            }

                            object val = pi.GetValue(_Current);
                            if (val == null)
                            {
                                val = "NULL";
                                tb.AddRow(pi.Name, val.ToString(), "")[1].ForeColor = ConsoleColor.Red;
                            }
                            else
                            {
                                tb.AddRow(pi.Name, val.ToString(), "");
                            }
                        }

                        string separator = tb.Separator;
                        foreach (CommandTableRow row in tb)
                        {
                            foreach (CommandTableCol col in row)
                            {
                                if (col.Index == 0)
                                {
                                    _Command.IO.SetForeColor(ConsoleColor.DarkGray);
                                }
                                else _Command.IO.SetForeColor(col.ForeColor);

                                if (col.Index != 0) _Command.IO.Write(separator);
                                _Command.IO.Write(col.GetFormatedValue());
                            }
                            _Command.IO.WriteLine("");
                        }
                        break;
                    }
                case "payloads":
                    {
                        IPayloadRequirements req = _Current.PayloadRequirements;
                        if (req == null || req is NoPayloadRequired)
                            _Command.IO.WriteLine(Lang.Get("Nothing_To_Show"));
                        else
                        {
                            CommandTable tb = new CommandTable();

                            tb.AddRow(tb.AddRow(Lang.Get("Name"), Lang.Get("Description")).MakeSeparator());

                            foreach (Payload p in PayloadCollection.Current)
                            {
                                if (!req.IsAllowedPayload(p)) continue;
                                tb.AddRow(p.FullPath, p.Description);
                            }

                            _Command.IO.Write(tb.Output());
                        }
                        break;
                    }
                case "targets":
                    {

                        break;
                    }
                default:
                    {
                        // incorrect use
                        WriteError(Lang.Get("Incorrect_Command_Usage"));
                        _Command.IO.SetForeColor(ConsoleColor.Gray);
                        _Command.IO.AddInput("help show");
                        break;
                    }
            }
        }
        public void cmdUse(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                _Current = null;
                args = "";
            }
            else
            {
                args = args.Trim();
                _Current = ModuleCollection.Current.GetByFullPath(args);
            }

            if (_Current == null) WriteError(Lang.Get(string.IsNullOrEmpty(args) ? "Command_Incomplete" : "Module_Not_Found", args));
            else
            {
                _Command.PromptCharacter =  _Current.Name + "> ";
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