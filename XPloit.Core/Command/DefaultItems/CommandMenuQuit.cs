namespace XPloit.Core.Command.DefaultItems
{
    public class CommandMenuQuit : CommandMenuItem
    {
        private readonly CommandMenu _Menu;

        public CommandMenuQuit(CommandMenu menu)
            : base(new string[] { "quit", "exit" })
        {
            _Menu = menu;

            HelpText = ""
                + "quit\n"
                + "Quits menu processing.";
        }

        public override void Execute(string arg) { _Menu.Quit(); }
    }
}