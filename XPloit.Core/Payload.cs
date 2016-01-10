using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using XPloit.Core.Attributes;
using XPloit.Core.Collections;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;

namespace XPloit.Core
{
    [TypeConverter(typeof(Payload.PayloadTypeConverter))]
    public class Payload : IModule
    {
        /// <summary>
        /// ModuleType
        /// </summary>
        internal override EModuleType ModuleType { get { return EModuleType.Payload; } }
        /// <summary>
        /// Platform
        /// </summary>
        public virtual EPlatform Platform { get { return EPlatform.None; } }
        /// <summary>
        /// Arquitecture
        /// </summary>
        public virtual EArquitecture Arquitecture { get { return EArquitecture.None; } }
        /// <summary>
        /// Payload value
        /// </summary>
        /// <param name="target">Target</param>
        public virtual byte[] GetValue(Target target) { return null; }
        /// <summary>
        /// References
        /// </summary>
        public virtual Reference[] References { get { return null; } }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        /// <param name="input">Input</param>
        public static implicit operator Payload(string input)
        {
            return PayloadCollection.Current.GetByFullPath(input, true);
        }

        public class PayloadTypeConverter : TypeConverter
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
                    return (Payload)value.ToString();
                }

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return ((Payload)value).FullPath;

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}