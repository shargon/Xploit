﻿using System;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;

namespace Auxiliary.Local.Fuzzing
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Search pattern string for exploit development")]
    public class PatternSearch : Module
    {
        #region Configure
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
        [ConvertHexString()]
        public string Find { get; set; }
        #endregion

        public PatternSearch() { Length = 5000; }

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