using System.ComponentModel;
using System.Text;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Attributes
{
    public class ConvertHexString : IPreSetVariable
    {
        /// <summary>
        /// Codificación a utilizar
        /// </summary>
        [Description("Encoding")]
        public Encoding Encoding { get; set; }
        /// <summary>
        /// Invierte la cadena antes de establecerla
        /// </summary>
        [Description("Reverse string")]
        public bool Reverse { get; set; }

        /// <summary>
        /// Edit the variable before set
        /// </summary>
        /// <param name="value">Value</param>
        public override string PreSetVariable(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.StartsWith("0x"))
            {
                try
                {
                    value = Encoding.GetString(HexHelper.FromHexString(value));
                }
                catch { }
            }
            return value;
        }

        public ConvertHexString()
        {
            Encoding = Encoding.ASCII;
        }
    }
}