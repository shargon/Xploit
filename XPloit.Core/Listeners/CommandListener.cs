using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;
using XPloit.Core.Menus;
using XPloit.Core.Menus.Main;
using XPloit.Core.Multi;
using XPloit.Res;

namespace XPloit.Core.Listeners
{
    public class CommandListener : IListener, IGetAutocompleteCommand
    {
        const string CommandStart = " > ";

        ConsoleColor _ClientColor = ConsoleColor.White;
        ConsoleColor _ServerColor = ConsoleColor.DarkGray;
        ConsoleColor _ServerErrorColor = ConsoleColor.Red;
        ConsoleColor _ServerGoodColor = ConsoleColor.Green;

        ICommandLayer _Command;
        bool _IsStarted;

        string[] _LastList = null;
        Menu _Current = null;
        Menu[] menus = new Menu[]
            {
                new MenuVersion(),
                new MenuSystem()
            };

        #region AutoComplete commands
        /// <summary>
        /// Get Available commands
        /// </summary>
        public string[] AvailableCommands()
        {
            List<string> av = new List<string>();
            av.Add("cd");
            av.Add("ls");
            av.Add("cls");
            av.Add("clear");
            av.Add("banner");
            av.Add("set");
            av.Add("use");
            av.Add("dir");
            av.Add("exit");

            Menu[] currentMenus = _Current == null ? menus : _Current.Childs;

            if (currentMenus != null)
            {
                foreach (Menu m in currentMenus)
                    foreach (string a in m.AllowedNames)
                        av.Add(a);
            }

            return av.ToArray();
        }
        public string[] AvailableCommandOptions(string command)
        {
            List<string> av = new List<string>();

            Menu[] currentMenus = _Current == null ? menus : _Current.Childs;
            if (currentMenus != null)
            {
                switch (command)
                {
                    case "use":
                    case "cd":
                        {
                            foreach (Menu m in currentMenus)
                                foreach (string a in m.AllowedNames)
                                    av.Add(a);
                            break;
                        }
                    default:
                        {
                            // Search by name
                            Menu m = Menu.SearchByName(currentMenus, command);
                            if (m != null)
                            {

                            }
                            break;
                        }
                }
            }

            return av.ToArray();// "arg0", "arg1", "arg2", "arg3", "arg4", "arg5" };
        }
        public EAllowAutocompleteCommand AllowAutocompleteMaths { get { return EAllowAutocompleteCommand.OnlyWhenEmpty; } }
        public EAllowAutocompleteCommand AllowAutocompleteFiles { get { return EAllowAutocompleteCommand.Yes; } }
        public EAllowAutocompleteCommand AllowAutocompleteFolders { get { return EAllowAutocompleteCommand.Yes; } }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public CommandListener(ICommandLayer command)
        {
            _Command = command;
            if (command != null && command is ConsoleCommand)
            {
                ConsoleCommand cmd = (ConsoleCommand)command;
                cmd.AutoCompleteSource = this;
            }
        }
        public override bool IsStarted { get { return _IsStarted; } }
        public override bool Start()
        {
            _IsStarted = true;

            _Command.Write("", true);
            BannerHelper.GetRandomBanner(_Command);
            _Command.Write("", true);

            string read;
            string readl;
            do
            {
                _Command.SetForeColor(_ClientColor);
                _Command.Write(CommandStart, false);

                if (_Current != null)
                {
                    _Command.Write(_Current.GetPath("/"), false);
                    _Command.Write(CommandStart, false);
                }

                read = _Command.ReadLine();
                if (read == null) continue;


                read = read.Trim();
                if (read == string.Empty) continue;

                readl = read.ToLowerInvariant();

                Menu[] currentMenus = _Current == null ? menus : _Current.Childs;

                switch (readl)
                {
                    case ".":
                    case "cd..":
                    case "cd ..":
                        {
                            // Down one level
                            if (_Current != null)
                            {
                                _Current = _Current.Parent;
                            }
                            break;
                        }
                    case "cls":
                    case "clear":
                        {
                            _Command.Clear();
                            break;
                        }
                    case "banner":
                        {
                            _Command.Write("", true);
                            BannerHelper.GetRandomBanner(_Command);
                            _Command.Write("", true);
                            break;
                        }
                    case "..":
                    case "cd/":
                    case "cd\\":
                    case "cd /":
                    case "cd \\":
                        {
                            // Down to main
                            if (_Current != null)
                            {
                                _Current = null;
                            }
                            break;
                        }
                    case "ls":
                    case "dir":
                        {
                            _Command.SetForeColor(_ServerColor);
                            if (currentMenus != null)
                            {
                                List<string> ls = new List<string>();
                                foreach (Menu m in currentMenus)
                                    ls.Add(m.Name);

                                _LastList = ls.ToArray();
                                PrintList();
                            }
                            else
                            {
                                _Command.Write(Lang.Get("Nothing_To_Show"), true);
                            }
                            break;
                        }
                    case "exit": { _IsStarted = false; break; }
                    default:
                        {
                            if (readl.StartsWith("cd ")) readl = readl.Substring(3);
                            else if (readl.StartsWith("use ")) readl = readl.Substring(4);

                            readl = readl.Trim('/', '\\');

                            if (_LastList != null)
                            {
                                int ix = 0;
                                if (int.TryParse(readl, out ix) && _LastList.Length > ix)
                                {
                                    readl = _LastList[ix];
                                }
                            }

                            if (currentMenus != null)
                            {
                                // Search by name
                                Menu m = Menu.SearchByName(currentMenus, readl);
                                if (m != null)
                                {
                                    _Current = m;
                                    _LastList = null;
                                    continue;
                                }
                                else
                                {
                                    double c = MathHelper.Calc(readl);
                                    if (!double.IsNaN(c))
                                    {
                                        _Command.SetForeColor(_ServerColor);
                                        _Command.Write(c.ToString(CultureInfo.InvariantCulture), true);
                                        continue;
                                    }
                                }
                            }

                            _Command.SetForeColor(_ServerErrorColor);
                            _Command.Write(Lang.Get("Unknown_Command"), true);
                            break;
                        }
                }
            }
            while (_IsStarted);
            return _IsStarted;
        }
        void PrintList()
        {
            if (_LastList == null) return;

            int ix = -1;
            foreach (string l in _LastList)
            {
                ix++;
                _Command.Write(ix.ToString().PadLeft(3, ' ') + " - " + l, true);
            }
        }
        public override bool Stop()
        {
            _IsStarted = false;
            return true;
        }
    }
}