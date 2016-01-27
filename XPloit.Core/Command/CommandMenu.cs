using System;
using XPloit.Core.Command.DefaultItems;
using XPloit.Core.Command.Interfaces;
using XPloit.Core.Interfaces;
using XPloit.Core.Listeners.Layer;

namespace XPloit.Core.Command
{
    /// <summary>
    /// A console menu structure, comprised of various menu items.
    ///
    /// <example>
    /// Create a menu which can display the time:
    /// <code>
    /// var menu = new CMenu ();
    /// menu.Add ("time", s => Console.WriteLine (DateTime.UtcNow));
    /// menu.Run ();
    /// </code>
    /// </example>
    /// </summary>
    public class CommandMenu : CommandMenuItem
    {
        bool _Quit;
        PromptDelegate Prompt = null;
        IAutoCompleteSource _AutoCompleteSource = null;

        //ConsoleColor _ServerColor = ConsoleColor.DarkGray;
        ConsoleColor _PromptColor = ConsoleColor.Green;
        ConsoleColor _ClientColor = ConsoleColor.White;

        /// <summary>
        /// PromptCharacter
        /// </summary>
        public string PromptCharacter { get; set; }

        /// <summary>
        /// Create a new CMenu.
        /// <para>
        /// The menu will initially contain the following commands:
        /// <list type="bullet">
        /// <item>help</item>
        /// <item>quit</item>
        /// </list>
        /// </para>
        /// </summary>
        public CommandMenu(CommandLayer io, IAutoCompleteSource autoCompleteSource, PromptDelegate prompt, string[] selector = null)
            : base(selector)
        {
            _IO = io;
            _AutoCompleteSource = autoCompleteSource;

            if (selector == null)
            {
                Add(new CommandMenuQuit(this));
                Add(new CommandMenuHelp(this));
            }

            PromptCharacter = "> ";
            Prompt = prompt == null ? OnPrompt : prompt;
        }
        void OnPrompt(CommandLayer sender)
        {
            sender.SetForeColor(_PromptColor);
            sender.Write(PromptCharacter);
            sender.SetForeColor(_ClientColor);
        }
        public override CommandMenuItem Add(CommandMenuItem it)
        {
            CommandMenuItem t = base.Add(it);
            t._IO = this.IO;
            // TODO: If have children io dosent copy?
            return t;
        }
        /// <summary>
        /// Start console promting and processing.
        /// </summary>
        public void Run()
        {
            _Quit = false;
            while (!_Quit)
            {
                ExecuteChild(ReadLine());
            }
        }
        string ReadLine()
        {
            return IO.ReadLine(Prompt, _AutoCompleteSource);
        }
        public void Quit() { _Quit = true; }
    }
}
