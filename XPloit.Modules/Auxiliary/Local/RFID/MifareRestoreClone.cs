using System;
using System.Collections.Generic;
using System.IO;
using Xploit.Core.Rfid.Enums;
using Xploit.Core.Rfid.Interfaces;
using Xploit.Core.Rfid.Mifare;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;
using XPloit.Rfid;

namespace Auxiliary.Local.NFC
{
    [AllowedPlatforms(Windows = true)]
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Mifare Restore clone (dont touch Trailing blocks)")]
    public class MifareRestoreClone : Module
    {
        Target[] _Readers = null;

        #region Configure
        public override Reference[] References
        {
            get { return new Reference[] { new Reference(EReferenceType.URL, "https://en.wikipedia.org/wiki/MIFARE"), }; }
        }
        public override Target[] Targets
        {
            get
            {
                if (_Readers == null) _Readers = LoadReaders();
                return _Readers;
            }
        }
        #endregion

        #region Properties
        [RequireExists()]
        [ConfigurableProperty(Description = "File for restore content")]
        public FileInfo File { get; set; }
        [ConfigurableProperty(Description = "Key for type for login")]
        public ConfigMifareRead.EKeyType KeyType { get; set; }

        #region Optional
        [ConfigurableProperty(Optional = true, Description = "Only this sectors")]
        public List<int> OnlySectors { get; set; }
        #endregion
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
        public MifareRestoreClone()
        {
            KeyType = ConfigMifareRead.EKeyType.A;
        }

        public static Target[] LoadReaders()
        {
            // Load readers
            using (CardReaderCollection readers = new CardReaderCollection())
            {
                int x = 0;
                Target[] ret = new Target[readers.Count];
                foreach (CardReader reader in readers)
                {
                    ret[x] = new Target(EPlatform.RFID, EArquitecture.None, reader.Name);
                    x++;
                }
                return ret;
            }
        }
        bool InternalRun(bool check)
        {
            if (Target == null)
            {
                WriteError("Target not found ");
                return false;
            }

            byte[] file = System.IO.File.ReadAllBytes(File.FullName);

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
                            int dSec = card.Sectors[0].DataBlocks[0].Data.Length;
                            int length = card.Sectors.Length * (dSec * 4);   //6 key a / 6 keyb / 4 rights

                            if (file.Length != length)
                            {
                                WriteError("File length must be " + StringHelper.Convert2KbWithBytes(length));
                                return false;
                            }
                            WriteInfo("Total sectors ....", card.Sectors.Length.ToString(), ConsoleColor.Cyan);

                            length = 0;
                            foreach (CardMifare.Sector sector in card.Sectors)
                            {
                                if (OnlySectors != null && OnlySectors.Count > 0 && !OnlySectors.Contains(sector.SectorNum))
                                {
                                    length += dSec * (sector.DataBlocks.Length + 1);
                                    WriteInfo("Checking sector " + sector.SectorNum.ToString().PadLeft(2, '0'), "Passed", ConsoleColor.Yellow);
                                    continue;
                                }

                                byte[] keyA = new byte[6], keyB = new byte[6];

                                Array.Copy(file, length + (dSec * 3), keyA, 0, 6);
                                Array.Copy(file, length + (dSec * 3) + 10, keyB, 0, 6);

                                ConfigMifareRead cfg = new ConfigMifareRead() { KeysOne = keyA, KeysZero = keyB };

                                ConfigMifareReadSector sec = cfg.ReadSectors[sector.SectorNum];
                                sec.Login = new LoginMifareMethod()
                                {
                                    KeyNum = KeyType == ConfigMifareRead.EKeyType.A ? ConfigMifareRead.EKeyNum.One : ConfigMifareRead.EKeyNum.Zero,
                                    KeyType = KeyType
                                };
                                sec.ReadDataBlockEnd = ConfigMifareReadSector.EBlockRange.DataBlock03;
                                sec.ReadDataBlockStart = ConfigMifareReadSector.EBlockRange.DataBlock01;
                                sec.ReadTrailBlock = false;

                                ICard c2;
                                if (!reader.GetCard(out c2, cfg))
                                {
                                    WriteError("Error reading sector " + sector.SectorNum.ToString() + " Blocks 1-3");
                                    return false;
                                }

                                CardMifare card2 = (CardMifare)c2;
                                CardMifare.Sector sector2 = card2.Sectors[sector.SectorNum];

                                bool equal = true;
                                foreach (CardMifare.Block block in sector2.DataBlocks)
                                {
                                    // Check block
                                    bool blockEqual = true;
                                    for (int x = 0, m = block.Data.Length; x < m; x++)
                                        if (block.Data[x] != file[x + length]) { blockEqual = false; break; }

                                    if (!blockEqual)
                                    {
                                        equal = false;
                                        if (!check)
                                        {
                                            // Write block
                                            byte[] data = new byte[block.Data.Length];
                                            Array.Copy(file, length, data, 0, data.Length);
                                            if (!block.WriteInBlock(reader, cfg, data))
                                                WriteError("Error writting sector " + sector2.SectorNum.ToString() + " block " + block.BlockNum.ToString());
                                        }
                                    }
                                    length += block.Data.Length;
                                }

                                // Info
                                if (!equal) WriteInfo("Checking sector " + sector.SectorNum.ToString().PadLeft(2, '0'), check ? "Different" : "Modified", ConsoleColor.Red);
                                else WriteInfo("Checking sector " + sector.SectorNum.ToString().PadLeft(2, '0'), "Equal", ConsoleColor.Green);

                                length += dSec;   //6 key a / 6 keyb / 4 rights
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