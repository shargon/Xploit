using System;
using Xploit.Core.Rfid.Enums;
using Xploit.Core.Rfid.Interfaces;
using Xploit.Core.Rfid.Mifare;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Rfid;

namespace Auxiliary.Local.NFC
{
    public class MifareSetId : Module
    {
        Target[] _Readers = null;

        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description
        {
            get
            {
                return "Mifare Id Setter. Require a valid card";
            }
        }
        public override Reference[] References
        {
            get { return new Reference[] { new Reference(EReferenceType.URL, "https://en.wikipedia.org/wiki/MIFARE"), }; }
        }
        public override Target[] Targets
        {
            get
            {
                if (_Readers == null) _Readers = MifareRestoreClone.LoadReaders();
                return _Readers;
            }
        }
        #endregion

        #region Properties
        [FileRequireExists()]
        [ConfigurableProperty(Required = true, Description = "Id for set (Require valid card)")]
        public byte[] Id { get; set; }
        [ConfigurableProperty(Required = true, Description = "Password for Write Sector")]
        public byte[] Password { get; set; }
        [ConfigurableProperty(Required = true, Description = "Key for type for login")]
        public ConfigMifareRead.EKeyType KeyType { get; set; }
        #endregion

        public override ECheck Check()
        {
            if (InternalRun(true)) return ECheck.Ok;
            return ECheck.Error;
        }
        public override bool Run() { return InternalRun(false); }

        /// <summary>
        /// Constructor
        /// </summary>
        public MifareSetId()
        {
            KeyType = ConfigMifareRead.EKeyType.A;
            Id = new byte[] { 0, 0, 0, 0 };
            Password = new byte[] { 0, 0, 0, 0, 0, 0 };
        }

        bool InternalRun(bool check)
        {
            using (CardReaderCollection cardReaderCollections = new CardReaderCollection())
            using (CardReader reader = cardReaderCollections[Target.Name])
            {
                switch (reader.Connect())
                {
                    case EConnection.Error:
                        {
                            WriteError("Error while connect to " + reader.Name);
                            return false;
                        }
                    case EConnection.NotCard:
                        {
                            WriteError("Card not present, please insert a card");
                            return false;
                        }
                }

                ICard ic;
                if (!reader.GetCard(out ic, null))
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

                CardMifare card = (CardMifare)ic;
                WriteInfo("Mifare type ......", card.MifareType.ToString(), ConsoleColor.Cyan);
                card.InitCard();

                switch (card.MifareType)
                {
                    case CardMifare.EMifareType.Classic1K:
                        {
                            if (Id == null || Id.Length != 4)
                            {
                                WriteError("Id must be 4 byte length (00:01:02:03:04:05:06:07)");
                                return false;
                            }

                            if (!check)
                            {
                                ConfigMifareRead cfg = new ConfigMifareRead();
                                cfg.KeysOne = Password;
                                cfg.ReadSectors[0].Login = new LoginMifareMethod()
                                {
                                    KeyNum = ConfigMifareRead.EKeyNum.One,
                                    KeyType = KeyType
                                };

                                cfg.ReadSectors[0].ReadDataBlockStart = ConfigMifareReadSector.EBlockRange.DataBlock01;
                                cfg.ReadSectors[0].ReadDataBlockEnd = ConfigMifareReadSector.EBlockRange.DataBlock03;
                                cfg.ReadSectors[0].ReadTrailBlock = true;

                                if (reader.GetCard(out ic, cfg))
                                {
                                    card = (CardMifare)ic;

                                    Array.Copy(Id, 0, card.Sectors[0].DataBlocks[0].Data, 0, Id.Length);
                                    if (card.Sectors[0].DataBlocks[0].WriteInBlock(reader, cfg, card.Sectors[0].DataBlocks[0].Data))
                                    {
                                        WriteInfo("Sucessfull write!");
                                        return true;
                                    }
                                    else
                                    {
                                        WriteError("Error while writting sector, check the password");
                                    }
                                }

                                return false;
                            }

                            return true;
                        }
                    default:
                        {
                            WriteError("Card " + card.MifareType.ToString() + " not implemented");
                            return false;
                        }
                }
            }
        }
    }
}