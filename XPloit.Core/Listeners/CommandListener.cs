using System;
using System.Collections.Generic;
using XPloit.Core.Interfaces;
using XPloit.Core.Menus;
using XPloit.Core.Menus.Main;
using XPloit.Core.Multi;
using XPloit.Res;

namespace XPloit.Core.Listeners
{
    public class CommandListener : IListener
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

        /// <summary>
        /// Constructor
        /// </summary>
        public CommandListener(ICommandLayer command)
        {
            _Command = command;
            if (command != null && command is ConsoleCommand)
            {
                ConsoleCommand cmd = (ConsoleCommand)command;
                cmd.OnAutoComplete += cmd_OnAutoComplete;
            }
        }
        /// <summary>
        /// Fill autocomplete source
        /// </summary>
        /// <param name="search">Search word</param>
        /// <param name="availables">Avaiables</param>
        void cmd_OnAutoComplete(string search, out string[] availables)
        {
            availables = null;

            search = search.ToLowerInvariant();

            // Search by name
            if (search.StartsWith("cd ")) search = search.Substring(3);

            Menu[] currentMenus = _Current == null ? menus : _Current.Childs;

            if (currentMenus != null)
            {
                List<string> ret = new List<string>();

                foreach (Menu m in currentMenus)
                {
                    if (m.Name.ToLowerInvariant().StartsWith(search))
                        ret.Add(m.Name);
                }

                availables = ret.ToArray();
            }
        }
        public override bool IsStarted { get { return _IsStarted; } }
        public override bool Start()
        {
            _IsStarted = true;

            _Command.SetForeColor(_ServerGoodColor);
            _Command.Write(Lang.Get("Wellcome"), true);

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