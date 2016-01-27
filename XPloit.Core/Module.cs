using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using XPloit.Core.Attributes;
using XPloit.Core.Collections;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
using XPloit.Core.Listeners.Layer;
using XPloit.Core.Interfaces;
using XPloit.Core.Requirements.Payloads;

namespace XPloit.Core
{
    [TypeConverter(typeof(Module.ModuleTypeConverter))]
    public class Module : IModule
    {
        IModule _Parent = null;

        /// <summary>
        /// ModuleType
        /// </summary>
        internal override EModuleType ModuleType { get { return EModuleType.Module; } }
        /// <summary>
        /// References
        /// </summary>
        public virtual Reference[] References { get { return null; } }
        /// <summary>
        /// DisclosureDate
        /// </summary>
        public virtual DateTime DisclosureDate { get { return DateTime.MinValue; } }
        /// <summary>
        /// Targets
        /// </summary>
        public virtual Target[] Targets { get { return null; } }
        /// <summary>
        /// Target
        /// </summary>
        [ConfigurableProperty(Required = true, Description = "Especify the Target")]
        public Target Target { get; set; }
        /// <summary>
        /// Payload
        /// </summary>
        [ConfigurableProperty(Required = true, Description = "Especify the Payload")]
        public Payload Payload { get; set; }
        /// <summary>
        /// Payload Requirements
        /// </summary>
        public virtual IPayloadRequirements PayloadRequirements { get { return new SpecifyPlatformRequired(this); } }
        /// <summary>
        /// Run Method
        /// </summary>
        /// <param name="cmd">Command</param>
        public virtual bool Run() { return false; }
        /// <summary>
        /// Check Method
        /// </summary>
        /// <param name="cmd">Command</param>
        public virtual ECheck Check() { return ECheck.CantCheck; }
        /// <summary>
        /// Prepare the current module
        /// </summary>
        /// <param name="module">Module</param>
        internal void Prepare(IModule module)
        {
            if (module == null) return;

            _Parent = module;
            Prepare(module.IO);
        }
        /// <summary>
        /// Copy selected properties to parent
        /// </summary>
        /// <param name="props">Props</param>
        public bool CopyPropertiesToActiveModule(params string[] props)
        {
            if (_Parent == null) return false;

            ReflectionHelper.CopyProperties(this, _Parent, props);
            return true;
        }
        /// <summary>
        /// Prepare the current module
        /// </summary>
        internal void Prepare(CommandLayer io)
        {
            SetIO(io);

            if (Target == null)
            {
                Target[] t = Targets;
                if (t != null && t.Length > 0)
                    Target = t[0];
            }
            if (Payload == null)
            {
                Payload[] payloads = PayloadCollection.Current.GetPayloadAvailables(PayloadRequirements);
                if (payloads != null && payloads.Length == 1)
                {
                    //Payload = payloads[0];
                    SetProperty("Payload", payloads[0]);
                    //Payload.SetIO(io);
                }
            }
        }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        /// <param name="input">Input</param>
        public static implicit operator Module(string input)
        {
            return ModuleCollection.Current.GetByFullPath(input, true);
        }

        public class ModuleTypeConverter : TypeConverter
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
                    return (Module)value.ToString();
                }

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return ((Module)value).FullPath;

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        /// <summary>
        /// Return true if Check is Intrusive
        /// </summary>
        public bool IsCheckIntrusive()
        {
            return GetType().GetMethod("Check").GetCustomAttribute<IntrusiveCheck>() != null;
        }
    }
}