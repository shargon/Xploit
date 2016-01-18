using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Rfid;

namespace Auxiliary.Local.NFC
{
    public class MifareRestoreClone : Module
    {
        Target[] _Readers = null;

        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Mifare Restore clone"; } }
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
        public override Target[] Targets { get { return _Readers; } }
        #endregion

        #region Properties
        [ConfigurableProperty(Required = true, Description = "File for restore content")]
        public string File { get; set; }
        #endregion

        public MifareRestoreClone()
        {
            // Load readers
            using (CardReaderCollection readers = new CardReaderCollection())
            {
                int x = 0;
                _Readers = new Target[readers.Count];
                foreach (CardReader reader in readers)
                {
                    _Readers[x] = new Target(EPlatform.RFID, EArquitecture.None, reader.Name);
                    x++;
                }
            }
        }

        public override bool Run()
        {

            return false;
        }
    }
}