using System;
using System.Linq;

namespace XPloit.Core.Command.DefaultItems
{
    public class CommandMenuHelp : CommandMenuItem
    {
        private readonly CommandMenu _Menu;

        public CommandMenuHelp(CommandMenu menu)
            : base(new string[] { "help", "man" })
        {
            if (menu == null)
            {
                throw new ArgumentNullException("menu");
            }

            _Menu = menu;

            HelpText = ""
                + "help [command]\n"
                + "Displays a help text for the specified command, or\n"
                + "Displays a list of all available commands.";
        }

        public override void Execute(string arg)
        {
            DisplayHelp(arg, _Menu, false);
        }

        private static void DisplayHelp(string arg, CommandMenuItem context, bool isInner)
        {
            if (arg == null)
            {
                throw new ArgumentNullException("arg");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (string.IsNullOrEmpty(arg))
            {
                if (!DisplayItemHelp(context, !context.Any()))
                {
                    DisplayAvailableCommands(context, isInner);
                }
                return;
            }

            string cmd = arg;
            CommandMenuItem inner = context.GetMenuItem(ref cmd, out arg, false, false, true);
            if (inner != null)
            {
                DisplayHelp(arg, inner, true);
                return;
            }

            context.IO.WriteLine("Could not find inner command \"" + cmd + "\".");
            if (context.Selector != null)
            {
                context.IO.WriteLine("Help for " + context.Selector + ":");
            }
            DisplayItemHelp(context, true);
        }

        private static bool DisplayItemHelp(CommandMenuItem item, bool force)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (item.HelpText == null)
            {
                if (force)
                {
                    item.IO.WriteLine("No help available for " + item.Selector);
                }
                return false;
            }
            else
            {
                item.IO.WriteLine(item.HelpText);
                return true;
            }
        }

        private static void DisplayAvailableCommands(CommandMenuItem menu, bool inner)
        {
            if (menu == null)
            {
                throw new ArgumentNullException("menu");
            }

            if (!inner)
            {
                menu.IO.WriteLine("Available commands:");
            }
            var abbreviations = menu.CommandAbbreviations().OrderBy(it => it.Key);
            foreach (var ab in abbreviations)
            {
                if (ab.Value == null)
                {
                    menu.IO.Write("      ");
                }
                else
                {
                    menu.IO.Write(ab.Value.PadRight(3) + " | ");
                }
                menu.IO.WriteLine(ab.Key);
            }
            if (!inner)
            {
                menu.IO.WriteLine("Type \"help <command>\" for individual command help.");
            }
        }
    }
}
