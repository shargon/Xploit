using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using XPloit.Core.Extensions;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;
using XPloit.Res;

namespace XPloit.Core.Command
{
    /// <summary>
    /// A single console menu item. It consists of a selector (keyword), a help text and the individual behavior.
    ///
    /// Also offers various ways to add, retrieve and use subitems.
    ///
    /// <example>
    /// Create a hellow world command:
    /// <code>
    /// var menuitem = new CommandMenuItem ("hello", s => Console.WriteLine ("Hello world!"));
    /// </code>
    /// </example>
    /// </summary>
    public class CommandMenuItem : IEnumerable<CommandMenuItem>
    {
        Dictionary<string, CommandMenuItem> _Menu = new Dictionary<string, CommandMenuItem>(StringComparer.InvariantCultureIgnoreCase);
        CommandMenuItem _Default = null;

        StringComparison? _StringComparison;

        internal ICommandLayer _IO;
        public ICommandLayer IO { get { return _IO; } }

        /// <summary>
        /// Parent of this item, if any.
        /// </summary>
        public CommandMenuItem Parent { get; private set; }

        /// <summary>
        /// This menu item.
        ///
        /// <remarks>
        /// This property can be used to combine object and collection initializers.
        /// <example>
        /// var m = new CommandMenuItem ("parent") {
        ///	HelpText = "help", // object initializer
        ///	MenuItem = { // collection initializer
        ///		new CommandMenuItem ("child 1"),
        ///		new CommandMenuItem ("child 2"),
        ///	}
        /// };
        /// </example>
        /// </remarks>
        /// </summary>
        public CommandMenuItem MenuItem { get { return this; } }

        /// <summary>
        /// Gets or sets how entered commands are compared.
        ///
        /// By default, the comparison is case insensitive and culture invariant.
        /// </summary>
        public virtual StringComparison StringComparison
        {
            get
            {
                if (_StringComparison.HasValue) return _StringComparison.Value;
                if (Parent != null) return Parent.StringComparison;
                return StringComparison.InvariantCultureIgnoreCase;
            }
            set
            {
                _StringComparison = value;
                _Menu = new Dictionary<string, CommandMenuItem>(_Menu, value.GetCorrespondingComparer());
            }
        }
        /// <summary>
        /// Keyword to select this item.
        /// </summary>
        public string[] Selector { get; set; }
        /// <summary>
        /// Descriptive help text.
        /// </summary>
        public string HelpText { get; set; }
        /// <summary>
        /// Behavior upon selection.
        ///
        /// By default, if present, this node's behavior will be executed.
        /// Else, execution will be delegated to the appropriate child.
        /// </summary>
        public virtual void Execute(string arg)
        {
            if (_Execute != null)
            {
                _Execute(arg);
                return;
            }

            if (this.Any()) ExecuteChild(arg);
        }

        private Action<string> _Execute;

        /// <summary>
        /// Creates a new CommandMenuItem from keyword, behavior and help text.
        /// </summary>
        /// <param name="selector">Keyword</param>
        /// <param name="execute">Behavior when selected.</param>
        /// <param name="help">Descriptive help text</param>
        public CommandMenuItem(string[] selector, Action<string> execute, string help = null)
        {
            Selector = selector;
            HelpText = help;
            SetAction(execute);
        }

        /// <summary>
        /// Creates a new CommandMenuItem from keyword.
        /// </summary>
        /// <param name="selector">Keyword</param>
        public CommandMenuItem(string[] selector) : this(selector, (Action<string>)null) { }

        /// <summary>
        /// Gets or sets the CommandMenuItem associated with the specified keyword.
        ///
        /// Use the null key to access the default item.
        /// </summary>
        /// <param name="key">
        /// Keyword of the CommandMenuItem. The selector must match perfectly (i.e. is not an abbreviation of the keyword).
        ///
        /// If the key is null, the value refers to the default item.
        /// </param>
        /// <value>
        /// The CommandMenuItem associated with the specified keyword, or null.
        /// </value>
        /// <returns>
        /// The menu item associated with the specified keyword.
        /// </returns>
        public CommandMenuItem this[string key]
        {
            get
            {
                if (key == null)
                {
                    return _Default;
                }
                CommandMenuItem it;
                _Menu.TryGetValue(key, out it);
                return it;
            }
            set
            {
                if (key == null) { _Default = value; }
                else { _Menu[key] = value; }
            }
        }

        /// <summary>
        /// Add new command.
        ///
        /// The menu's internal structure and abbreviations are updated automatically.
        /// </summary>
        /// <param name="it">Command to add.</param>
        /// <returns>The added CommandMenuItem</returns>
        public virtual CommandMenuItem Add(CommandMenuItem it)
        {
            if (it == null) throw new ArgumentNullException("it");
            if (it.Parent != null) throw new ArgumentException("Menuitem already has a parent.", "it");

            if (it.Selector != null)
            {
                foreach (string sel in it.Selector)
                    _Menu.Add(sel, it);
            }
            else
            {
                if (_Default != null) throw new ArgumentException("The default item was already set.", "it");
                _Default = it;
            }

            if (_IO != null && it._IO == null)
                it._IO = _IO;
            it.Parent = this;

            return it;
        }

        /// <summary>
        /// Adds a new command from keyword and help.
        /// </summary>
        /// <param name="selector">Keyword</param>
        /// <param name="help">Descriptive help text</param>
        /// <returns>The added CommandMenuItem</returns>
        public CommandMenuItem Add(string[] selector, string help)
        {
            return Add(selector, (Action<string>)null, help);
        }

        /// <summary>
        /// Adds a new command from keyword.
        /// </summary>
        /// <param name="selector">Keyword</param>
        /// <returns>The added CommandMenuItem</returns>
        public CommandMenuItem Add(string[] selector)
        {
            return Add(selector, null);
        }

        /// <summary>
        /// Creates a new CommandMenuItem from keyword, behavior and help text.
        /// </summary>
        /// <param name="selector">Keyword</param>
        /// <param name="execute">Behavior when selected.</param>
        /// <param name="help">Descriptive help text</param>
        /// <returns>The added CommandMenuItem</returns>
        public CommandMenuItem Add(string[] selector, Action<string> execute, string help = null)
        {
            return Add(new CommandMenuItem(selector, execute, help));
        }

        /// <summary>
        /// Returns the commands equal, or starting with, the specified cmd.
        ///
        /// Does not return the default menu item.
        /// </summary>
        private CommandMenuItem[] GetCommands(string cmd, out string[] selectors, StringComparison comparison, bool useCache)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");

            selectors = null;

            if (useCache)
            {
                CommandMenuItem mi;
                _Menu.TryGetValue(cmd, out mi);
                if (mi != null) return new[] { mi };
            }

            List<string> search = new List<string>();
            List<CommandMenuItem> its = new List<CommandMenuItem>();

            foreach (string sel in _Menu.Keys)
            {
                if (sel.StartsWith(cmd, comparison))
                {
                    search.Add(sel);
                    its.Add(_Menu[sel]);
                    //break;
                }
            }

            selectors = search.ToArray();
            return its.ToArray();
            //CommandMenuItem[] its = _Menu.Values
            //    .Where(it => it.Selector.StartsWith(cmd, comparison))
            //    .OrderBy(it => it.Selector)
            //    .ToArray();
            //return its;
        }

        /// <summary>
        /// Retrieves the IMenuItem associated with the specified keyword.
        ///
        /// If no single item matches perfectly, the search will broaden to all items starting with the keyword.
        ///
        /// In case sensitive mode, missing match which could be solved by different casing will re reported if complain is specified.
        ///
        /// If <c>useDefault</c> is set and a default item is present, it will be returned and no complaint will be generated.
        /// </summary>
        /// <param name="cmd">
        /// In: The command, possibly with arguments, from which the keyword is extracted which uniquely identifies the searched menu item.
        /// Out: The keyword uniquely identifying a menu item, or null if no such menu item was found.
        /// </param>
        /// <param name="args">
        /// Out: The arguments which were supplied in addition to a keyword.
        /// </param>
        /// <param name="complain">
        /// If true, clarifications about missing or superfluous matches will be written to stdout.
        /// </param>
        /// <param name="useDefault">
        /// The single closest matching menu item, or the default item if no better fit was found, or null in case of 0 or multiple matches.
        /// </param>
        /// <returns></returns>
        public CommandMenuItem GetMenuItem(ref string cmd, out string args, bool complain, bool useDefault, bool useCache)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");

            // Is there a fitting child menu?
            string original = cmd;
            args = cmd;
            cmd = StringHelper.SplitFirstWord(ref args);

            string[] selectors;
            CommandMenuItem[] its = GetCommands(cmd, out selectors, StringComparison, useCache);

            if (its.Length == 1) return its[0];

            if (its.Length > 1)
            {
                if (complain)
                {
                    IO.SetForeColor(ConsoleColor.Red);
                    string s = cmd == ""
                        ? Lang.Get("Command_Incomplete")
                        : Lang.Get("Command_Not_Unique", cmd);

                    IO.WriteLine(s + " Candidates: " + string.Join(", ", selectors));
                }
                return null;
            }

            // Is there a fallback?
            CommandMenuItem def = this[null];
            if (def != null)
            {
                cmd = null;
                args = original;
                return def;
            }

            // We found nothing. Display this failure?
            if (complain)
            {
                IO.SetForeColor(ConsoleColor.Red);
                IO.WriteLine(Lang.Get("Unknown_Command", cmd));

                if (StringComparison.IsCaseSensitive())
                {
                    /*  CommandMenuItem[] suggestions =*/
                    GetCommands(cmd, out selectors, StringComparison.InvariantCultureIgnoreCase, useCache);
                    if (selectors != null && selectors.Length > 0)
                    {
                        if (selectors.Length == 1)
                        {
                            IO.WriteLine("Did you mean \"" + selectors[0] + "\"?");
                        }
                        else if (selectors.Length <= 5)
                        {
                            IO.Write("Did you mean ");
                            IO.Write(string.Join(", ", selectors.Take(selectors.Length - 1).Select(sug => "\"" + sug + "\"")));
                            IO.Write(" or \"" + selectors.Last() + "\"?");
                            IO.WriteLine("");
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Executes the specified command using only children (instead of this node's own behavior).
        /// </summary>
        /// <param name="args">Command to execute using contained commands.</param>
        public void ExecuteChild(string args)
        {
            string cmd = args;
            CommandMenuItem it = GetMenuItem(ref cmd, out args, true, true, true);
            if (it != null) it.Execute(args);
        }

        /// <summary>
        /// Returns an enumerator over all menu items contained in this item.
        ///
        /// The default item will not be enumerated.
        /// </summary>
        public IEnumerator<CommandMenuItem> GetEnumerator() { return _Menu.Values.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        /// <summary>
        /// Returns a dictionary containing all contained menu items and their corresponding abbreviation.
        ///
        /// The abbreviations will be updated if commands are added, changed or removed.
        ///
        /// The default menu item will not be returned.
        /// </summary>
        public IDictionary<string, string> CommandAbbreviations()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (string sel in _Menu.Keys)
            {
                string ab = GetAbbreviation(sel);
                if (ab.Length >= sel.Length - 1) ab = null;
                dict.Add(sel, ab);
            }

            return dict;
        }

        private string GetAbbreviation(string cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");

            for (int i = 1, m = cmd.Length; i <= m; i++)
            {
                string sub = cmd.Substring(0, i), dummy;
                if (GetMenuItem(ref sub, out dummy, false, false, false) != null)
                    return sub;
            }
            return cmd;
        }

        /// <summary>
        /// Sets the behavior upon selection
        /// </summary>
        /// <param name="action">
        /// Behavior when selected.
        /// </param>
        public void SetAction(Action<string> action) { _Execute = action; }

        public override string ToString() { return "[" + string.Join(",", Selector) + "]"; }
    }
}
