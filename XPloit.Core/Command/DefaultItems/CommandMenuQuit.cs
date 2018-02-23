using XPloit.Res;

namespace XPloit.Core.Command.DefaultItems
{
    public class CommandMenuQuit : CommandMenuItem
    {
        private readonly CommandMenu _Menu;

        public CommandMenuQuit(CommandMenu menu)
            : base(new string[] { "quit", "exit" })
        {
            _Menu = menu;
            HelpText = Lang.Get("Man_Exit");
        }

        public override void Execute(string arg) { _Menu.Quit(); }
    }
}