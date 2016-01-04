using System;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;

namespace XPloit.Modules.Auxiliary.Local
{
    public class AuxiliaryNFC : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "NFC Restore system"; } }
        public override DateTime DisclosureDate { get { return DateTime.MinValue; } }
        public override bool IsLocal { get { return true; } }
        public override string Path { get { return "Auxiliary/Local"; } }
        public override string Name { get { return "NFC"; } }
        public override Reference[] References
        {
            get
            {
                return new Reference[] 
                { 
                    new Reference(EReferenceType.URL, "https://en.wikipedia.org/wiki/MIFARE") ,
                };
            }
        }
        public override Target[] Targets
        {
            get
            {
                return new Target[]
                {
                    new Target(EPlatform.RFID ,"Mifare-1K", new Variable("Sectors",12) )
                };
            }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Required = true, Description = "File for restore content")]
        public string File { get; set; }
        #endregion

        public override bool Run()
        {

            return false;
        }
    }
}