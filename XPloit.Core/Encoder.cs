using System;
using System.ComponentModel;
using System.Globalization;
using XPloit.Core.Collections;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;

namespace XPloit.Core
{
    [TypeConverter(typeof(Encoder.EncoderTypeConverter))]
    public class Encoder : IModule
    {
        /// <summary>
        /// ModuleType
        /// </summary>
        internal override EModuleType ModuleType { get { return EModuleType.Encoder; } }
        /// <summary>
        /// Runt
        /// </summary>
        /// <param name="payload">Payload</param>
        /// <returns>Return encoded payload</returns>
        public virtual object Run(Payload payload) { throw (new NotImplementedException()); }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        /// <param name="input">Input</param>
        public static implicit operator Encoder(string input)
        {
            return EncoderCollection.Current.GetByFullPath(input, true);
        }

        public class EncoderTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;

                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string)
                {
                    return (Encoder)value.ToString();
                }

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return ((Encoder)value).FullPath;

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}