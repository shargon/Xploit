using System;
using System.Linq;
using XPloit.Res;

namespace XPloit.Core.Command.DefaultItems
{
    public class CommandMenuHelp : CommandMenuItem
    {
        private readonly CommandMenu _Menu;

        public CommandMenuHelp(CommandMenu menu)
            : base(new string[] { "help", "man" })
        {
            if (menu == null) throw new ArgumentNullException("menu");

            _Menu = menu;

            HelpText = Lang.Get("Man_Help");
        }

        public override void Execute(string arg)
        {
            DisplayHelp(arg, _Menu, false);
        }

        static void DisplayHelp(string arg, CommandMenuItem context, bool isInner)
        {
            if (arg == null) throw new ArgumentNullException("arg");
            if (context == null) throw new ArgumentNullException("context");

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

            context.IO.WriteLine(Lang.Get("Could_Not_Find_Inner_Command", cmd));

            if (context.Selector != null)
                context.IO.WriteLine(Lang.Get("Help_For", string.Join(", ", context.Selector) + ":"));

            DisplayItemHelp(context, true);
        }

        static bool DisplayItemHelp(CommandMenuItem item, bool force)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (string.IsNullOrEmpty(item.HelpText))
            {
                if (force && item.Selector != null)
                    item.IO.WriteLine(Lang.Get("No_Help_Available", string.Join(", ", item.Selector)));

                return false;
            }
            else
            {
                item.IO.WriteLine(item.HelpText);
                return true;
            }
        }

        static void DisplayAvailableCommands(CommandMenuItem menu, bool inner)
        {
            if (menu == null) throw new ArgumentNullException("menu");

            CommandTable tb = new CommandTable();

            if (!inner)
            {
                menu.IO.WriteLine(Lang.Get("Available_Commands") + ":");
                menu.IO.WriteLine("");

                tb.AddRow(tb.AddRow(Lang.Get("Short"), Lang.Get("Command")).MakeSeparator());
            }

            bool entra = false;
            var abbreviations = menu.CommandAbbreviations().OrderBy(it => it.Key);
            foreach (var ab in abbreviations)
            {
                //if (ab.Value == null)
                //{
                //    menu.IO.Write("      ");
                //}
                //else
                //{
                //    menu.IO.Write(ab.Value.PadRight(3) + " | ");
                //}

                tb.AddRow(ab.Value, ab.Key)[0].Align = CommandTableCol.EAlign.Right;
                //menu.IO.WriteLine(ab.Key);
                entra = true;
            }

            if (entra) menu.IO.WriteLine(tb.Output());

            if (!inner) menu.IO.WriteLine(Lang.Get("Type_Help"));
        }
    }
}
