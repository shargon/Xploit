using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using XPloit.Core.Collections;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;

namespace XPloit.Core
{
    [TypeConverter(typeof(Nop.NopTypeConverter))]
    public class Nop : IModule
    {
        /// <summary>
        /// ModuleType
        /// </summary>
        internal override EModuleType ModuleType { get { return EModuleType.Nop; } }
        /// <summary>
        /// Encoding value
        /// </summary>
        public virtual Encoding Encoding { get { return Encoding.UTF8; } }
        /// <summary>
        /// Nop value
        /// </summary>
        public virtual byte[] Get(int size) { return null; }
        /// <summary>
        /// String value
        /// </summary>
        public string GetString(int size)
        {
            byte[] va = Get(size);
            if (va == null) return null;
            return this.Encoding.GetString(va);
        }
        /// <summary>
        /// References
        /// </summary>
        public virtual Reference[] References { get { return null; } }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        /// <param name="input">Input</param>
        public static implicit operator Nop(string input)
        {
            return NopCollection.Current.GetByFullPath(input, true);
        }

        public class NopTypeConverter : TypeConverter
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
                    return (Nop)value.ToString();
                }

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return ((Nop)value).FullPath;

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}