using System;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;

namespace Auxiliary.Local.Fuzzing
{
    public class PatternSearch : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Search pattern string for exploit development"; } }
        public override Reference[] References
        {
            get { return new Reference[] { new Reference(EReferenceType.URL, "http://unlogic.co.uk/2014/07/16/exploit-pattern-generator/") }; }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Length of pattern")]
        public int Length { get; set; }
        [ConfigurableProperty(Description = "Start to search in (default 0)")]
        public int Start { get; set; }
        [ConfigurableProperty(Description = "String to find (ex. Aa0A)")]
        public string Find { get; set; }
        #endregion

        public PatternSearch() { Length = 1000; }

        public override bool Run()
        {
            int ix = PatternHelper.Search(Length, (Find), Start);

            if (ix == -1)
            {
                WriteError("'" + Find + "' not found in pattern with " + Length.ToString() + " length" + (Start > 0 ? " starting from " + Start.ToString() : ""));
                return false;
            }

            WriteInfo("Pattern '" + Find + "' first occurrence" +
                (Start > 0 ? " from " + Start.ToString() : "") +
                " at ", ix.ToString(), ConsoleColor.Green);

            return true;
        }
    }
}