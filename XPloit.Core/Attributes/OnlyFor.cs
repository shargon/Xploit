using System;
using System.IO;

namespace XPloit.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OnlyFor : Attribute
    {
        public bool Windows { get; set; }
        public bool Linux { get; set; }
        public bool Mac { get; set; }
    }
}