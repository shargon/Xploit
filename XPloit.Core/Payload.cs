using System;
using System.ComponentModel;
using System.Text;
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
        /// Encoding value
        /// </summary>
        public virtual Encoding Encoding { get { return Encoding.UTF8; } }
        /// <summary>
        /// Payload value
        /// </summary>
        public virtual byte[] Value { get { return null; } }
        /// <summary>
        /// PayloadString value
        /// </summary>
        public string StringValue { get { return this.Encoding.GetString(Value); } }
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
            return PayloadCollection.Current.GetByFullPath(input);
        }

        public class PayloadTypeConverter : TypeConverter
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
                    return PayloadCollection.Current.GetByFullPath(value.ToString());

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return ((Payload)value).FullPath;

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}