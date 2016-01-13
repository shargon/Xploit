using System;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;

namespace Auxiliary.Local.NFC
{
    public class NFCDump : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "NFC Restore system"; } }
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
                    new Target(EPlatform.RFID ,EArquitecture.None, "Mifare-1K", new Variable("Sectors",12) )
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