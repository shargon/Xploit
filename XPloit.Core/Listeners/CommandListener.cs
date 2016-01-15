using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using XPloit.Core.Attributes;
using XPloit.Core.Collections;
using XPloit.Core.Command;
using XPloit.Core.Command.Interfaces;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;
using XPloit.Res;

namespace XPloit.Core.Listeners
{
    public class CommandListener : IListener, IAutoCompleteSource
    {
        ICommandLayer _IO = null;
        CommandMenu _Command = null;
        IModule _CurrentGlobal = null;
        IModule _Current = null;
        bool _IsStarted;

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
                case "man":
                case "help":
                    {
                        foreach (CommandMenuItem m in _Command)
                            foreach (string sep in m.Selector)
                                yield return sep;

                        break;
                    }
                case "use":
                    {
                        foreach (IModule e in ModuleCollection.Current) yield return e.FullPath;
                        foreach (IModule e in PayloadCollection.Current) yield return e.FullPath;
                        break;
                    }
                case "show":
                    {
                        if (arguments == null || arguments.Length <= 1)
                        {
                            foreach (string e in new string[] { "options", "config", "payloads", "targets", "info" }) yield return e;
                        }
                        break;
                    }
                case "set":
                case "gset":
                    {
                        if (_Current == null) break;

                        Module curM = _Current.ModuleType == EModuleType.Module ? (Module)_Current : null;

                        if (arguments == null || arguments.Length <= 1)
                        {
                            if (curM != null)
                            {
                                Target[] t = curM.Targets;
                                if (t != null && t.Length > 1) yield return "Target";
                                if (curM.PayloadRequirements != null)
                                    yield return "Payload";

                                if (curM.Payload != null)
                                    foreach (PropertyInfo pi in ReflectionHelper.GetProperties(curM.Payload, true, true, true)) yield return pi.Name;
                            }

                            foreach (PropertyInfo pi in ReflectionHelper.GetProperties(_Current, true, true, true)) yield return pi.Name;
                        }
                        else
                        {
                            string pname = arguments[0].ToLowerInvariant().Trim();
                            switch (pname)
                            {
                                case "target":
                                    {
                                        Target[] ts = curM == null ? null : curM.Targets;
                                        if (ts != null)
                                        {
                                            foreach (Target t in ts)
                                                yield return t.Id.ToString();
                                        }
                                        break;
                                    }
                                case "payload":
                                    {
                                        IPayloadRequirements req = curM == null ? null : curM.PayloadRequirements;
                                        if (req != null && req.ItsRequired())
                                        {
                                            foreach (Payload p in PayloadCollection.Current)
                                            {
                                                if (!req.IsAllowed(p)) continue;
                                                yield return p.FullPath;
                                            }
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        // By property value
                                        PropertyInfo[] pi = null;

                                        if (curM != null && curM.Payload != null) pi = ReflectionHelper.GetProperties(curM.Payload, pname);

                                        if (pi == null || pi.Length == 0) pi = ReflectionHelper.GetProperties(_Current, pname);
                                        if (pi != null && pi.Length > 0)
                                        {
                                            Type tp = pi[0].PropertyType;
                                            if (tp == typeof(bool))
                                            {
                                                yield return "true";
                                                yield return "false";
                                            }
                                            else
                                            {
                                                if (tp.IsEnum)
                                                {
                                                    foreach (string name in Enum.GetNames(pi[0].PropertyType))
                                                        yield return name;
                                                }
                                                else
                                                {
                                                    if (/*tp == typeof(string) ||*/ tp == typeof(FileInfo) || tp == typeof(DirectoryInfo))
                                                    {
                                                        string path = arguments[1];

                                                        if (!string.IsNullOrEmpty(path))
                                                        {
                                                            string pathl = path.ToLowerInvariant();

                                                            bool allowFolders =
                                                                tp == typeof(DirectoryInfo) /*||
                                                                pi[0].Name.ToLowerInvariant().Contains("folder") ||
                                                                pi[0].Name.ToLowerInvariant().Contains("directory") ||
                                                                pi[0].Name.ToLowerInvariant().Contains("path")*/;

                                                            bool allowFiles = !allowFolders;
                                                            if (allowFiles) allowFolders = true;

                                                            if (Directory.Exists(path))
                                                            {
                                                                if (allowFiles)
                                                                    foreach (string dir in Directory.GetFiles(path)) yield return dir;
                                                                if (allowFolders)
                                                                    foreach (string dir in Directory.GetDirectories(path)) yield return dir;
                                                            }
                                                            else
                                                            {
                                                                bool isWindows = SystemHelper.IsWindows;
                                                                string path2 = Path.GetDirectoryName(path);

                                                                if (!string.IsNullOrEmpty(path2) && Directory.Exists(path2))
                                                                {
                                                                    if (allowFiles)
                                                                        foreach (string dir in Directory.GetFiles(path2))
                                                                        {
                                                                            if (isWindows)
                                                                            {
                                                                                if (!dir.ToLowerInvariant().StartsWith(pathl)) continue;
                                                                            }
                                                                            else
                                                                            {
                                                                                if (!dir.StartsWith(path)) continue;
                                                                            }
                                                                            yield return dir;
                                                                        }
                                                                    foreach (string dir in Directory.GetDirectories(path2))
                                                                    {
                                                                        if (isWindows)
                                                                        {
                                                                            if (!dir.ToLowerInvariant().StartsWith(pathl)) continue;
                                                                        }
                                                                        else
                                                                        {
                                                                            if (!dir.StartsWith(path)) continue;
                                                                        }

                                                                        if (allowFolders)
                                                                            foreach (string d in Directory.GetDirectories(dir)) yield return d;
                                                                        if (allowFiles)
                                                                            foreach (string d in Directory.GetFiles(dir)) yield return d;

                                                                        if (allowFolders)
                                                                            yield return dir;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
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
            CommandMenu cmd = new CommandMenu(command, this, OnPrompt);
            _IO = command;

            cmd.Add(new string[] { "banner" }, cmdBanner, Lang.Get("Man_Banner"));
            cmd.Add(new string[] { "version" }, cmdVersion, Lang.Get("Man_Version"));
            cmd.Add(new string[] { "clear", "cls" }, cmdClear, Lang.Get("Man_Clear"));
            cmd.Add(new string[] { "cd", "cd..", "cd\\", "cd/", "back" }, cmdCD, Lang.Get("Man_Cd"));

            // Modules command
            cmd.Add(new string[] { "use" }, cmdUse, Lang.Get("Man_Use"));
            cmd.Add(new string[] { "show" }, cmdShow, Lang.Get("Man_Show"));
            cmd.Add(new string[] { "set" }, cmdSet, Lang.Get("Man_Set"));
            cmd.Add(new string[] { "gset" }, cmdSetG, Lang.Get("Man_Set_Global"));
            cmd.Add(new string[] { "check" }, cmdCheck, Lang.Get("Man_Check"));
            cmd.Add(new string[] { "exploit", "run" }, cmdRun, Lang.Get("Man_Run"));
            cmd.Add(new string[] { "reload" }, cmdReload, Lang.Get("Man_Reload"));
            cmd.Add(new string[] { "resource" }, cmdResource, Lang.Get("Man_Resource"));
            cmd.Add(new string[] { "kill" }, cmdKill, Lang.Get("Man_Kill"));
            cmd.Add(new string[] { "jobs" }, cmdJobs, Lang.Get("Man_Jobs"));
            cmd.Add(new string[] { "load" }, cmdLoad, Lang.Get("Man_Load"));

            cmd.Add(new string[] { "rerun", "rexploit" }, cmdReRun, Lang.Get("Man_ReRun"));
            cmd.Add(new string[] { "rcheck" }, cmdRCheck, Lang.Get("Man_RCheck"));
            cmd.Add(new string[] { "info" }, cmdInfo, Lang.Get("Man_Info"));

            cmd.Add(new string[] { "search" }, cmdSearch, Lang.Get("Man_Search"));
            cmd.Add(new string[] { "ifcheck" }, cmdIfCheck, Lang.Get("Man_IfCheck"));
            cmd.Add(new string[] { "ifnocheck" }, cmdIfNoCheck, Lang.Get("Man_IfNoCheck"));
            _Command = cmd;
        }
        void OnPrompt(ICommandLayer sender)
        {
            sender.SetForeColor(ConsoleColor.Green);

            if (_Current != null)
            {
                sender.SetForeColor(ConsoleColor.DarkGreen);
                sender.Write(Lang.Get(_Current.ModuleType.ToString()) + "(");
                sender.SetForeColor(ConsoleColor.Green);
                sender.Write(_Current.Name);
                sender.SetForeColor(ConsoleColor.DarkGreen);
                sender.Write(")");
                sender.SetForeColor(ConsoleColor.Green);
                sender.Write("> ");
            }
            else sender.Write("> ");

            sender.SetForeColor(ConsoleColor.White);
        }
        bool CheckModule(bool checkRequieredProperties, EModuleType expected)
        {
            if (_Current == null)
            {
                _IO.WriteError(Lang.Get("Require_Module"));
                return false;
            }

            if (expected != EModuleType.None)
            {
                if (_Current.ModuleType != expected)
                {
                    _IO.WriteError(Lang.Get("Require_Module_Type", expected.ToString()));
                    return false;
                }
            }

            string error;
            if (checkRequieredProperties && !_Current.CheckRequiredProperties(_IO, out error))
            {
                _Current.WriteInfo(error);
                return false;
            }
            return true;
        }

        #region Commands
        public void cmdClear(string args) { _IO.Clear(); }
        public void cmdCD(string args)
        {
            _Current = null;
            //_Command.PromptCharacter = "> ";
        }
        public void cmdJobs(string args)
        {
            if (JobCollection.Current.Count <= 0)
            {
                _IO.WriteInfo(Lang.Get("Nothing_To_Show"));
                return;
            }

            CommandTable tb = new CommandTable();
            _IO.WriteLine("");

            tb.AddRow(tb.AddRow(Lang.Get("Id"), Lang.Get("Status"), Lang.Get("Module")).MakeSeparator());

            foreach (Job j in JobCollection.Current)
            {
                CommandTableRow row;
                if (j.IsRunning)
                {
                    row = tb.AddRow(j.Id.ToString(), Lang.Get("Running"), j.FullPathModule);
                    row[0].Align = CommandTableCol.EAlign.Right;
                    row[1].ForeColor = ConsoleColor.Green;
                }
                else
                {
                    row = tb.AddRow(j.Id.ToString(), Lang.Get("Dead"), j.FullPathModule);
                    row[0].Align = CommandTableCol.EAlign.Right;
                    row[1].ForeColor = ConsoleColor.Red;
                }
            }

            tb.OutputColored(_IO);
            _IO.WriteLine("");
        }
        public void cmdVersion(string args)
        {
            _IO.WriteLine("");

            CommandTable tb = new CommandTable();

            _IO.WriteLine(Lang.Get("Version_Start"));
            _IO.WriteLine("");

            tb.AddRow(tb.AddRow(Lang.Get("File"), Lang.Get("Version")).MakeSeparator());

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GlobalAssemblyCache) continue;

                string file = "??";
                if (string.IsNullOrEmpty(asm.Location))
                    file = asm.ManifestModule.ScopeName;
                else file = Path.GetFileName(asm.Location);

                tb.AddRow(file, asm.ImageRuntimeVersion);
            }

            tb.AddRow("", "");

            tb.AddRow(tb.AddRow(Lang.Get("Module"), Lang.Get("Count")).MakeSeparator());
            tb.AddRow(Lang.Get("Modules"), ModuleCollection.Current.Count.ToString());
            tb.AddRow(Lang.Get("Encoders"), EncoderCollection.Current.Count.ToString());
            tb.AddRow(Lang.Get("Payloads"), PayloadCollection.Current.Count.ToString());
            tb.AddRow(Lang.Get("Nops"), NopCollection.Current.Count.ToString());

            _IO.WriteLine(tb.Output());
        }
        public void cmdBanner(string args)
        {
            _IO.WriteLine("");
            BannerHelper.GetRandomBanner(_IO);
            _IO.WriteLine("");
        }
        public void cmdRun(string args) { RunModule(); }
        public void cmdCheck(string args) { CheckModule(); }
        public void cmdIfCheck(string args) { if (CheckModule()) _IO.AddInput(args); }
        public void cmdIfNoCheck(string args) { if (!CheckModule()) _IO.AddInput(args); }
        public bool RunModule()
        {
            if (!CheckModule(true, EModuleType.Module)) return false;

            try
            {
                Thread th = new Thread(new ThreadStart(runModule));
                th.Name = "RUN " + _Current.FullPath;
                th.IsBackground = true;
                th.Start();
                th.Join();

                //if (((Module)_Current).Run()) return true;
                //_Current.WriteError(Lang.Get("Run_Error"));
            }
            catch (Exception e)
            {
                _Current.WriteError(e.Message);
            }
            return false;
        }
        void runModule()
        {
            try
            {
                if (((Module)_Current).Run()) return;
                _Current.WriteError(Lang.Get("Run_Error"));
            }
            catch (Exception e)
            {
                _Current.WriteError(e.Message);
            }
        }
        public bool CheckModule()
        {
            if (!CheckModule(true, EModuleType.Module)) return false;

            try
            {
                Module m = (Module)_Current;

                if (m.IsCheckIntrusive())
                {
                    _IO.WriteInfo(Lang.Get("Can_Be_Intrusive"));
                    string line = _IO.ReadLine(null, null);
                    if (!(bool)ConvertHelper.ConvertTo(line, typeof(bool)))
                        return false;
                }

                switch (m.Check())
                {
                    case ECheck.CantCheck: _Current.WriteInfo(Lang.Get("Check_CantCheck")); break;
                    case ECheck.Error: _Current.WriteInfo(Lang.Get("Check_Result"), Lang.Get("Error"), ConsoleColor.Red); break;
                    case ECheck.NotSure: _Current.WriteInfo(Lang.Get("Check_NotSure")); break;
                    case ECheck.Ok: _Current.WriteInfo(Lang.Get("Check_Result"), Lang.Get("Ok"), ConsoleColor.Green); return true;
                }
            }
            catch (Exception e)
            {
                _Current.WriteError(e.Message);
            }
            return false;
        }
        public void cmdKill(string args)
        {
            args = args.Trim();

            try
            {
                if (string.IsNullOrEmpty(args))
                {
                    _IO.WriteError(Lang.Get("Incorrect_Command_Usage"));
                    return;
                }

                int job = (int)ConvertHelper.ConvertTo(args, typeof(int));
                if (JobCollection.Current.Kill(job))
                {
                    _IO.WriteInfo(Lang.Get("Kill_Job"), Lang.Get("Ok"), ConsoleColor.Green);
                }
                else
                {
                    _IO.WriteInfo(Lang.Get("Kill_Job"), Lang.Get("Error"), ConsoleColor.Red);
                }
            }
            catch (Exception e)
            {
                _IO.WriteError(e.Message);
            }
        }
        public void cmdRCheck(string args)
        {
            if (!CheckModule(false, EModuleType.Module)) return;

            cmdReload(args);
            cmdCheck(args);
        }
        public void cmdReRun(string args)
        {
            if (!CheckModule(false, EModuleType.Module)) return;

            cmdReload(args);
            cmdRun(args);
        }
        public void cmdReload(string args)
        {
            // Todo comentar en Res.res los comandos y acabar del set para abajo
            if (!CheckModule(false, EModuleType.None)) return;

            _IO.AddInput("use " + _Current.FullPath);
            _IO.WriteInfo(Lang.Get("Reloaded_Module", _Current.FullPath), Lang.Get("Ok"), ConsoleColor.Green);
        }
        public void cmdLoad(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                _IO.WriteError(Lang.Get("Incorrect_Command_Usage"));
                return;
            }
            args = args.Trim();
            if (!File.Exists(args))
            {
                _IO.WriteError(Lang.Get("File_Not_Exists", args));
                return;
            }

            try
            {
                _IO.SetForeColor(ConsoleColor.Gray);
                _IO.Write(Lang.Get("Reading_File", args));

                Assembly.Load(File.ReadAllBytes(args));
                //Assembly.LoadFile(args);

                _IO.SetForeColor(ConsoleColor.Green);
                _IO.WriteLine(Lang.Get("Ok").ToUpperInvariant());
            }
            catch
            {
                _IO.SetForeColor(ConsoleColor.Red);
                _IO.WriteLine(Lang.Get("Error").ToUpperInvariant());
            }
            CommandTable tb = new CommandTable();

            _IO.WriteLine("");

            tb.AddRow(tb.AddRow(Lang.Get("Type"), Lang.Get("Count")).MakeSeparator());

            tb.AddRow(Lang.Get("Modules"), ModuleCollection.Current.Load().ToString())[1].Align = CommandTableCol.EAlign.Right;
            tb.AddRow(Lang.Get("Payloads"), PayloadCollection.Current.Load().ToString())[1].Align = CommandTableCol.EAlign.Right;
            tb.AddRow(Lang.Get("Encoders"), EncoderCollection.Current.Load().ToString())[1].Align = CommandTableCol.EAlign.Right;
            tb.AddRow(Lang.Get("Nops"), NopCollection.Current.Load().ToString())[1].Align = CommandTableCol.EAlign.Right;

            tb.OutputColored(_IO);
            _IO.WriteLine("");
        }
        public void cmdResource(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                _IO.WriteError(Lang.Get("Incorrect_Command_Usage"));
                return;
            }
            args = args.Trim();
            if (!File.Exists(args))
            {
                _IO.WriteError(Lang.Get("File_Not_Exists", args));
                return;
            }
            try
            {
                _IO.SetForeColor(ConsoleColor.Gray);
                _IO.Write(Lang.Get("Reading_File", args));

                foreach (string line in File.ReadAllLines(args))
                {
                    string ap = line.Trim();
                    if (string.IsNullOrEmpty(ap)) continue;
                    _IO.AddInput(ap);
                }

                _IO.SetForeColor(ConsoleColor.Green);
                _IO.WriteLine(Lang.Get("Ok").ToUpperInvariant());
            }
            catch
            {
                _IO.SetForeColor(ConsoleColor.Red);
                _IO.WriteLine(Lang.Get("Error").ToUpperInvariant());
            }
        }
        public void cmdSet(string args) { cmdSet(args, false); }
        public void cmdSetG(string args) { cmdSet(args, true); }
        public void cmdSet(string args, bool global)
        {
            if (!CheckModule(false, EModuleType.None)) return;
            args = args.Trim();

            string[] prop = ArgumentHelper.ArrayFromCommandLine(args);
            if (prop == null || (prop.Length != 2 && prop.Length != 1))
            {
                _IO.WriteError(Lang.Get("Incorrect_Command_Usage"));
                return;
            }

            object val = prop.Length == 2 ? prop[1] : null;

            if (!_Current.SetProperty(prop[0], val))
            {
                _IO.WriteError(Lang.Get("Error_Converting_Value"));
            }
            else
            {
                if (global) _CurrentGlobal.SetProperty(prop[0], val);
            }
        }
        public void cmdInfo(string args)
        {
            if (!CheckModule(false, EModuleType.None)) return;
            args = args.Trim().ToLowerInvariant();

            Module curM = _Current.ModuleType == EModuleType.Module ? (Module)_Current : null;

            CommandTable tb = new CommandTable();

            tb.AddRow(Lang.Get("Path"), _Current.Path, "")[0].ForeColor = ConsoleColor.DarkGray;
            tb.AddRow(Lang.Get("Name"), _Current.Name, "")[0].ForeColor = ConsoleColor.DarkGray;

            tb.AddRow("", "", "");

            if (!string.IsNullOrEmpty(_Current.Author))
            {
                CommandTableRow row = tb.AddRow(1, Lang.Get("Author"), _Current.Author, "");
                row[0].ForeColor = ConsoleColor.DarkGray;
                row[1].Align = CommandTableCol.EAlign.None;
                row[2].Align = CommandTableCol.EAlign.None;
            }

            if (curM != null)
            {
                if (curM.DisclosureDate != DateTime.MinValue) tb.AddRow(Lang.Get("DisclosureDate"), curM.DisclosureDate.ToString(), "")[0].ForeColor = ConsoleColor.DarkGray;

                if (curM.References != null && curM.References.Length > 0)
                {
                    tb.AddRow("", "", "");

                    StringBuilder sb = new StringBuilder();
                    foreach (Reference r in curM.References)
                    {
                        if (r == null) continue;
                        sb.AppendLine(r.Type.ToString() + " - " + r.Value);
                    }

                    foreach (CommandTableRow row in tb.AddSplitRow(1, Lang.Get("References"), sb.ToString(), ""))
                    {
                        row[0].ForeColor = ConsoleColor.DarkGray;
                        row[1].Align = CommandTableCol.EAlign.None;
                        row[2].Align = CommandTableCol.EAlign.None;
                    }
                }
            }

            if (!string.IsNullOrEmpty(_Current.Description))
            {
                foreach (CommandTableRow row in tb.AddSplitRow(1, Lang.Get("Description"), _Current.Description, ""))
                {
                    row[0].ForeColor = ConsoleColor.DarkGray;
                    row[1].Align = CommandTableCol.EAlign.None;
                    row[2].Align = CommandTableCol.EAlign.None;
                }
            }
            tb.OutputColored(_IO);
        }
        public void cmdSearch(string args)
        {
            args = args.Trim().ToLowerInvariant();


            string[] pars = ArgumentHelper.ArrayFromCommandLine(args);
            if (pars != null && pars.Length > 0)
            {
                CommandTable tb = new CommandTable();

                bool primera1 = false;
                foreach (Module m in ModuleCollection.Current.Search(pars))
                {
                    if (!primera1)
                    {
                        if (tb.Count > 0) tb.AddRow("", "", "");
                        CommandTableRow row = tb.AddRow(Lang.Get("Type"), Lang.Get("Path"), Lang.Get("Disclosure"));
                        tb.AddRow(row.MakeSeparator());
                        tb.AddRow("", "", "");
                        primera1 = true;
                    }

                    tb.AddRow(Lang.Get("Modules"), m.FullPath, m.DisclosureDate.ToShortDateString());
                }
                bool primera2 = false;
                foreach (IModule m in PayloadCollection.Current.Search(pars))
                {
                    if (!primera2)
                    {
                        if (tb.Count > 0) tb.AddRow("", "", "");
                        CommandTableRow row = tb.AddRow(Lang.Get("Type"), Lang.Get("Path"), "");
                        tb.AddRow(row.MakeSeparator());
                        tb.AddRow("", "", "");
                        primera2 = true;
                    }

                    tb.AddRow(Lang.Get("Payload"), m.FullPath, "");
                }

                if (primera1 || primera2)
                    tb.OutputColored(_IO);
                else
                    _IO.WriteInfo(Lang.Get("Nothing_To_Show"));
            }
            else
            {
                _IO.WriteInfo(Lang.Get("Be_More_Specific"));
            }
        }
        public void cmdShow(string args)
        {
            if (!CheckModule(false, EModuleType.None)) return;
            args = args.Trim().ToLowerInvariant();

            Module curM = _Current.ModuleType == EModuleType.Module ? (Module)_Current : null;

            switch (args)
            {
                case "":
                case "info": { cmdInfo(args); break; }
                case "options":
                case "config":
                    {
                        // set target id
                        Target[] ps = null;

                        if (curM != null)
                        {
                            ps = curM.Targets;
                            if (ps != null)
                            {
                                int ix = 0;
                                foreach (Target t in ps) { t.Id = ix; ix++; }
                            }
                        }

                        CommandTable tb = new CommandTable();

                        string title = "";
                        for (int x = 0; x <= 2; x++)
                        {
                            PropertyInfo[] pis = null;

                            object pv = _Current;
                            switch (x)
                            {
                                case 0:
                                    {
                                        if (_Current.ModuleType == EModuleType.Payload)
                                            title = Lang.Get("Payload_Options", _Current.FullPath);
                                        else
                                            title = Lang.Get("Module_Options", _Current.FullPath);

                                        pis = ReflectionHelper.GetProperties(_Current, true, true, true);
                                        break;
                                    }
                                case 1:
                                    {
                                        title = Lang.Get("Current_Target");
                                        if (ps != null && ps.Length > 1)
                                            pis = ReflectionHelper.GetProperties(_Current, "Target");
                                        break;
                                    }
                                case 2:
                                    {
                                        if (curM != null)
                                        {
                                            if (curM.Payload != null)
                                            {
                                                pv = curM.Payload;
                                                pis = ReflectionHelper.GetProperties(curM.Payload, true, true, true);
                                                title = Lang.Get("Payload_Options", curM.Payload.FullPath);
                                            }
                                            else
                                            {
                                                if (curM.PayloadRequirements != null && curM.PayloadRequirements.ItsRequired())
                                                {
                                                    pis = ReflectionHelper.GetProperties(_Current, "Payload");
                                                    title = Lang.Get("Selected_Payload");
                                                }
                                            }
                                        }
                                        break;
                                    }
                            }

                            if (pis != null)
                            {
                                bool primera = true;// (x != 1 || !hasX0);
                                foreach (PropertyInfo pi in pis)
                                {
                                    ConfigurableProperty c = pi.GetCustomAttribute<ConfigurableProperty>();
                                    if (c == null)
                                        continue;

                                    if (primera)
                                    {
                                        if (tb.Count > 0) tb.AddRow("", "", "");
                                        CommandTableRow row = tb.AddRow(0, title, "", "");
                                        tb.AddRow("", "", "");

                                        row[0].Align = CommandTableCol.EAlign.None;
                                        row[1].Align = CommandTableCol.EAlign.None;
                                        row[2].Align = CommandTableCol.EAlign.None;
                                        primera = false;
                                    }

                                    object val = pi.GetValue(pv);
                                    if (val == null)
                                    {
                                        val = "NULL";
                                        CommandTableRow row = tb.AddRow(pi.Name, val.ToString(), c.Description);
                                        if (c.Required) row[1].ForeColor = ConsoleColor.Red;
                                        else
                                        {
                                            if (x == 0 || x == 3)
                                                row[1].ForeColor = ConsoleColor.Cyan;
                                        }
                                    }
                                    else
                                    {
                                        CommandTableRow row = tb.AddRow(pi.Name, val.ToString(), c.Description);
                                        if (x == 0 || x == 3)
                                            row[1].ForeColor = ConsoleColor.Cyan;
                                    }
                                }
                            }
                        }

                        string separator = tb.Separator;
                        foreach (CommandTableRow row in tb)
                        {
                            foreach (CommandTableCol col in row)
                            {
                                if (col.ReplicatedChar == '\0')
                                {
                                    switch (col.Index)
                                    {
                                        case 0: _IO.SetForeColor(ConsoleColor.DarkGray); break;
                                        case 1: _IO.SetForeColor(col.ForeColor); break;
                                        case 2: _IO.SetForeColor(ConsoleColor.Yellow); break;
                                    }
                                }
                                else _IO.SetForeColor(col.ForeColor);

                                if (col.Index != 0) _IO.Write(separator);
                                _IO.Write(col.GetFormatedValue());
                            }
                            _IO.WriteLine("");
                        }
                        break;
                    }
                case "payloads":
                    {
                        Payload[] ps = curM == null ? null : PayloadCollection.Current.GetPayloadAvailables(curM.PayloadRequirements);
                        if (ps == null || ps.Length == 0)
                            _IO.WriteInfo(Lang.Get("Nothing_To_Show"));
                        else
                        {
                            CommandTable tb = new CommandTable();

                            tb.AddRow(tb.AddRow(Lang.Get("Name"), Lang.Get("Description")).MakeSeparator());

                            foreach (Payload p in ps)
                                tb.AddRow(p.FullPath, p.Description);

                            _IO.Write(tb.Output());
                        }
                        break;
                    }
                case "targets":
                    {
                        Target[] ps = curM == null ? null : curM.Targets;
                        if (ps == null || ps.Length <= 1)
                            _IO.WriteInfo(Lang.Get("Nothing_To_Show"));
                        else
                        {
                            CommandTable tb = new CommandTable();

                            tb.AddRow(tb.AddRow(Lang.Get("Name"), Lang.Get("Description")).MakeSeparator());

                            int ix = 0;
                            foreach (Target p in ps)
                            {
                                p.Id = ix; ix++;
                                tb.AddRow(p.Id.ToString(), p.Name);
                            }

                            _IO.Write(tb.Output());
                        }
                        break;
                    }
                default:
                    {
                        // incorrect use
                        _IO.WriteError(Lang.Get("Incorrect_Command_Usage"));
                        _IO.AddInput("help show");
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

                _CurrentGlobal = null;
                if (_CurrentGlobal == null) _CurrentGlobal = ModuleCollection.Current.GetByFullPath(args, false);
                if (_CurrentGlobal == null) _CurrentGlobal = PayloadCollection.Current.GetByFullPath(args, false);

                if (_CurrentGlobal != null)
                {
                    _Current = (IModule)ReflectionHelper.Clone(_CurrentGlobal, true);

                    if (_Current != null && _Current is Module)
                    {
                        ((Module)_Current).Prepare(_IO);
                    }
                }
                else
                {
                    _Current = null;
                }
            }

            if (_Current == null) _IO.WriteError(Lang.Get(string.IsNullOrEmpty(args) ? "Command_Incomplete" : "Module_Not_Found", args));
            //else
            //{
            //    _Command.PromptCharacter = _Current.Name + "> ";
            //}
        }
        #endregion

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