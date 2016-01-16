using System;
using System.IO;

namespace XPloit.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FileRequireExists : Attribute
    {
        /// <summary>
        /// Return if its valid the property
        /// </summary>
        /// <param name="val">Value</param>
        public bool IsValid(object val)
        {
            if (val == null) return true;
            if (val is FileInfo)
            {
                FileInfo fi = (FileInfo)val;
                fi.Refresh();
                return fi.Exists;
            }
            return false;
        }
    }
}