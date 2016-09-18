using System;
using System.ComponentModel;
using System.Globalization;
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
        /// References
        /// </summary>
        public virtual Reference[] References { get { return null; } }


        /// <summary>
        /// Fill the nop
        /// </summary>
        /// <param name="buffer">Data</param>
        /// <param name="index">Index</param>
        /// <param name="length">Length</param>
        /// <returns>Return true if nop its filled</returns>
        public virtual bool Fill(byte[] buffer, int index, int length)
        {
            return false;
        }

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
        /// <summary>
        /// Generate nops
        /// </summary>
        /// <param name="nopCount">Nop count</param>
        public byte[] CreateJob(int nopCount)
        {
            byte[] nops = new byte[nopCount];

            if (nopCount > 0)
                Fill(nops, 0, nopCount);

            return nops;
        }
    }
}