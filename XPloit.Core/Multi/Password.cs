using System;
using System.ComponentModel;

namespace XPloit.Core.Multi
{
    [TypeConverter(typeof(Password.PasswordTypeConverter))]
    public class Password
    {
        /// <summary>
        /// Password
        /// </summary>
        public string RawPassword { get; set; }


        public override string ToString()
        {
            return (RawPassword == null ? "Raw[NULL]" : "Raw=" + RawPassword);
        }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        /// <param name="input">Input</param>
        public static implicit operator Password(string input) { return new Password() { RawPassword = input }; }

        public class PasswordTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;

                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string)
                    return new Password() { RawPassword = Convert.ToString(value) };

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return ((Password)value).RawPassword;

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}