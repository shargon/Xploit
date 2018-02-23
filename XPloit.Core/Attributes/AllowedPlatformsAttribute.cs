using System;

namespace XPloit.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AllowedPlatformsAttribute : Attribute
    {
        public bool Windows { get; set; }
        public bool Linux { get; set; }
        public bool Mac { get; set; }
    }
}