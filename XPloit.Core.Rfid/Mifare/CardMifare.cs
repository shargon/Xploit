using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using XPloit.Core.Rfid.Helpers;
using XPloit.Core.Rfid;
using Xploit.Core.Rfid.Mifare.Internal;
using Xploit.Core.Rfid.Enums;
using Xploit.Core.Rfid.Interfaces;

namespace Xploit.Core.Rfid.Mifare
{
    public class CardMifare : ICard
    {
        public enum EMifareType
        {
            Unknown,
            Classic1K,
            Classic4K,
            UltraLight,
            Mini
        }

        bool _Initialized;
        EMifareType _MifareType;
        Sector[] _Sectors;

        /// <summary>
        /// Id de la tarjeta
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Tipo de tarjeta
        /// </summary>
        public ECardType Type { get { return ECardType.Mifare; } }
        /// <summary>
        /// Tipo de Mifare
        /// </summary>
        public EMifareType MifareType { get { return _MifareType; } }
        /// <summary>
        /// Sectores
        /// </summary>
        public Sector[] Sectors { get { return _Sectors; } }
        /// <summary>
        /// ATR de la tarjeta
        /// </summary>
        public byte[] Atr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="atr">Atr de la tarjeta</param>
        public CardMifare(EMifareType type, byte[] atr) : this(type) { Atr = atr; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type</param>
        public CardMifare(EMifareType type)
        {
            _MifareType = type;
            _Sectors = null;
            _Initialized = false;
        }

        /// <summary>
        /// Inizializa la tarjeta como
        /// </summary>
        public void InitCard()
        {
            if (_Initialized) return;
            if (_MifareType == EMifareType.Unknown) return;

            switch (_MifareType)
            {
                case EMifareType.Classic1K:
                    {
                        int max = 16;   // 16 sectores
                        _Sectors = new Sector[max];

                        for (byte x = 0; x < max; x++)
                            _Sectors[x] = new Sector(this, x);

                        _Initialized = true;
                        break;
                    }
                case EMifareType.Classic4K:
                    {
                        int max = 40;   // 40 sectores 
                        _Sectors = new Sector[max];

                        for (byte x = 0; x < max; x++)
                            _Sectors[x] = new Sector(this, x);

                        _Initialized = true;
                        break;
                    }
            }

        }

        public class TrailingBlock : Block
        {
            public AccessConditions AccessConditions
            {
                get;
                private set;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="parent">Padre</param>
            /// <param name="blockNum">Número de bloque</param>
            public TrailingBlock(Sector parent, byte blockNum) : base(parent, blockNum) { }

            internal override void CopyFrom(byte[] data)
            {
                base.CopyFrom(data);
                AccessConditions = AccessBits.GetAccessConditions(data);
            }
        }

        public class Block
        {
            Sector _Parent;
            byte _BlockNum = 0;
            byte[] _Data = new byte[16];
            bool _IsReaded = false;

            /// <summary>
            /// Sector padre
            /// </summary>
            public Sector Parent { get { return _Parent; } }
            /// <summary>
            /// Bloque número
            /// </summary>
            public byte BlockNum { get { return _BlockNum; } }
            /// <summary>
            /// Datos de la tarjeta
            /// </summary>
            public byte[] Data { get { return _Data; } }
            /// <summary>
            /// Datos codificados como UTF-8
            /// </summary>
            public string Utf8Data { get { return Encoding.UTF8.GetString(_Data); } }
            /// <summary>
            /// Ha sido leido el bloque si o no
            /// </summary>
            public bool IsReaded { get { return _IsReaded; } }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="parent">Padre</param>
            /// <param name="blockNum">Número de bloque</param>
            public Block(Sector parent, byte blockNum)
            {
                _Parent = parent;
                _BlockNum = blockNum;
            }
            public override string ToString()
            {
                return "Block " + _BlockNum + " - " + _BlockNum.ToString("x2") + (_IsReaded ? " {" + NFCHelper.Buffer2Hex(Data) + "}" : "");
            }
            /// <summary>
            /// Copia el contenido del buffer al bloque
            /// </summary>
            /// <param name="data">Datos a copiar 0-16  bytes</param>
            virtual internal void CopyFrom(byte[] data)
            {
                Array.Copy(data, 0, _Data, 0, 16);
                _IsReaded = true;
            }
            /// <summary>
            /// Escribe el el bloque los datos especificados
            /// </summary>
            /// <param name="data">Datos a escribir</param>
            /// <returns>Devuelve true si es correcto</returns>
            public bool WriteInBlock(CardReader reader, ConfigMifareRead config, byte[] data)
            {
                int lg = 16;
                switch (_Parent.Parent._MifareType)
                {
                    case EMifareType.Classic1K:
                    case EMifareType.Classic4K: lg = 16; break;
                    case EMifareType.UltraLight: lg = 4; break;
                }

                if (data == null || data.Length != lg)
                    throw (new ArgumentException("Data for " + _Parent.Parent._MifareType.ToString() + " must be " + lg.ToString() + " bytes length", "data"));

                if (config != null)
                {
                    if (!(config is ConfigMifareRead))
                        throw (new ArgumentException("Config must be ConfigMifareRead for Mifare Reads", "config"));

                    ConfigMifareRead cfg = (ConfigMifareRead)config;
                    ConfigMifareReadSector scfg = cfg.ReadSectors[_Parent.SectorNum];

                    if (scfg != null && scfg.Login != null)
                    {
                        // Login al primer sector
                        byte[] ldata = CardReader.SendCmd(reader._hCard, new byte[] { 0xFF, 0x86, 0x00, 0x00, 0x05, 0x01, 0x00, _BlockNum,
                                           (byte)(scfg.Login.KeyType == ConfigMifareRead.EKeyType.A ? 0x60 : 0x61),
                                           (byte)(scfg.Login.KeyNum == ConfigMifareRead.EKeyNum.Zero ? 0x00 : 0x01) });

                        if (ldata == null || ldata.Length != 2 || ldata[0] != 0x90 || ldata[1] != 0x00)
                            return false;
                    }
                }

                byte[] r = CardReader.SendCmd(reader._hCard, new byte[] { 0xFF, 0xD6, 0x00, _BlockNum, (byte)(data.Length) }.Concat(data).ToArray());
                if (r != null && r.Length != 2)//&& r[0] == 0x90 && r[1] != 0x00)
                    return true;

                return false;
            }
        }

        public class Sector
        {
            byte _SectorNum;
            Block[] _DataBlocks;
            TrailingBlock _TrailingBlock;
            CardMifare _Parent;

            /// <summary>
            /// Padre del sector
            /// </summary>
            public CardMifare Parent { get { return _Parent; } }
            /// <summary>
            /// Sector número
            /// </summary>
            public byte SectorNum { get { return _SectorNum; } }
            /// <summary>
            /// Bloques de datos
            /// </summary>
            public Block[] DataBlocks { get { return _DataBlocks; } }
            /// <summary>
            /// Bloque de acceso
            /// </summary>
            public TrailingBlock TrailingBlock { get { return _TrailingBlock; } }
            /// <summary>
            /// Datos de la tarjeta
            /// </summary>
            public byte[] Data
            {
                get
                {
                    List<byte> lx = new List<byte>();

                    foreach (Block b in _DataBlocks)
                        if (b.IsReaded) lx.AddRange(b.Data);

                    return lx.ToArray();
                }
            }
            /// <summary>
            /// Datos codificados como UTF-8
            /// </summary>
            public string Utf8Data { get { return Encoding.UTF8.GetString(Data); } }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="parent">Padre del sector</param>
            /// <param name="sectorNum">Número de sector</param>
            public Sector(CardMifare parent, byte sectorNum)
            {
                _Parent = parent;
                _SectorNum = sectorNum;

                switch (parent._MifareType)
                {
                    case EMifareType.Classic1K:
                        {
                            // 3 bloques de datos
                            _DataBlocks = new Block[3];
                            for (int x = 0; x < 3; x++)
                                _DataBlocks[x] = new Block(this, (byte)((sectorNum * 4) + x));

                            // 1 bloque de acceso
                            _TrailingBlock = new TrailingBlock(this, (byte)((sectorNum * 4) + 3));
                            break;
                        }
                    case EMifareType.Classic4K:
                        {
                            if (sectorNum < 32) // 32 sectores de 4 bloques
                            {
                                // 3 bloques de datos
                                _DataBlocks = new Block[3];
                                for (int x = 0; x < 3; x++)
                                    _DataBlocks[x] = new Block(this, (byte)((sectorNum * 4) + x));

                                // 1 bloque de acceso
                                _TrailingBlock = new TrailingBlock(this, (byte)((sectorNum * 4) + 3));
                            }
                            else // 8 sectores de 16 bloques
                            {
                                int lastbBlock = 128;
                                int s = sectorNum - 32;

                                // 3 bloques de datos
                                _DataBlocks = new Block[15];
                                for (int x = 0; x < 15; x++)
                                    _DataBlocks[x] = new Block(this, (byte)((s * 16) + x + lastbBlock));

                                // 1 bloque de acceso
                                _TrailingBlock = new TrailingBlock(this, (byte)((s * 16) + 15 + lastbBlock));
                            }
                            break;
                        }
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                foreach (Block b in _DataBlocks)
                {
                    if (!b.IsReaded) continue;

                    if (sb.Length == 0) sb.Append(" - ");
                    sb.Append(NFCHelper.Buffer2Hex(b.Data));
                }

                return "Sector " + _SectorNum + " - " + _SectorNum.ToString("x2") + (sb.Length == 0 ? "" : " {" + sb.ToString() + "}");
            }
        }

        /// <summary>
        /// Obtiene toda la información de la tarjeta
        /// </summary>
        /// <returns></returns>
        public byte[] GetData(bool copyNonReaded)
        {
            if (_Sectors == null) return new byte[] { };

            List<byte> lx = new List<byte>();

            foreach (Sector s in _Sectors)
                foreach (Block b in s.DataBlocks)
                {
                    if (b == null || (!copyNonReaded && !b.IsReaded)) continue;

                    lx.AddRange(b.Data);
                }

            return lx.ToArray();
        }
    }
}