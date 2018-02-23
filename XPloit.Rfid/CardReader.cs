using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Xploit.Core.Rfid.Enums;
using Xploit.Core.Rfid.Interfaces;
using Xploit.Core.Rfid.Mifare;
using XPloit.Rfid.Asn1;
using XPloit.Rfid.DniE;
using XPloit.Rfid.Helpers;

namespace XPloit.Rfid
{
    public class CardReader : IDisposable
    {
        #region Estaticas
        public static class API
        {
            public const int SCARD_PROTOCOL_T0 = 1;
            public const int SCARD_PROTOCOL_T1 = 2;
            public const int SCARD_PROTOCOL_RAW = 4;

            public const int SCARD_SHARE_SHARED = 2;

            public const int SCARD_SCOPE_USER = 0;
            public const int SCARD_SCOPE_TERMINAL = 1;
            public const int SCARD_SCOPE_SYSTEM = 2;

            public const int SCARD_PCI_T0 = 1;
            public const int SCARD_PCI_T1 = 2;

            public const int SCARD_LEAVE_CARD = 0;
            public const int SCARD_RESET_CARD = 1;
            public const int SCARD_UNPOWER_CARD = 2;

            public enum SCardFunctionReturnCodes : uint
            {
                SCARD_S_SUCCESS = 0x0,
                //'Errors
                SCARD_E_CANCELLED = 0x80100002,
                SCARD_E_INVALID_HANDLE = 0x80100003,
                SCARD_E_INVALID_PARAMETER = 0x80100004,
                SCARD_E_INVALID_TARGET = 0x80100005,
                SCARD_E_NO_MEMORY = 0x80100006,
                SCARD_F_WAITED_TOO_LONG = 0x80100007,
                SCARD_E_INSUFFICIENT_BUFFER = 0x80100008,
                SCARD_E_UNKNOWN_READER = 0x80100009,
                SCARD_E_TIMEOUT = 0x8010000A,
                SCARD_E_SHARING_VIOLATION = 0x8010000B,
                SCARD_E_NO_SMARTCARD = 0x8010000C,
                SCARD_E_UNKNOWN_CARD = 0x8010000D,
                SCARD_E_CANT_DISPOSE = 0x8010000E,
                SCARD_E_PROTO_MISMATCH = 0x8010000F,
                SCARD_E_NOT_READY = 0x80100010,
                SCARD_E_INVALID_VALUE = 0x80100011,
                SCARD_E_SYSTEM_CANCELLED = 0x80100012,
                SCARD_E_INVALID_ATR = 0x80100015,
                SCARD_E_NOT_TRANSACTED = 0x80100016,
                SCARD_E_READER_UNAVAILABLE = 0x80100017,
                SCARD_E_PCI_TOO_SMALL = 0x80100019,
                SCARD_E_READER_UNSUPPORTED = 0x8010001A,
                SCARD_E_DUPLICATE_READER = 0x8010001B,
                SCARD_E_CARD_UNSUPPORTED = 0x8010001C,
                SCARD_E_NO_SERVICE = 0x8010001D,
                SCARD_E_SERVICE_STOPPED = 0x8010001E,
                SCARD_E_UNEXPECTED = 0x8010001F,
                SCARD_E_ICC_INSTALLATION = 0x80100020,
                SCARD_E_ICC_CREATEORDER = 0x80100021,
                SCARD_E_DIR_NOT_FOUND = 0x80100023,
                SCARD_E_FILE_NOT_FOUND = 0x80100024,
                SCARD_E_NO_DIR = 0x80100025,
                SCARD_E_NO_FILE = 0x80100026,
                SCARD_E_NO_ACCESS = 0x80100027,
                SCARD_E_WRITE_TOO_MANY = 0x80100028,
                SCARD_E_BAD_SEEK = 0x80100029,
                SCARD_E_INVALID_CHV = 0x8010002A,
                SCARD_E_UNKNOWN_RES_MNG = 0x8010002B,
                SCARD_E_NO_SUCH_CERTIFICATE = 0x8010002C,
                SCARD_E_CERTIFICATE_UNAVAILABLE = 0x8010002D,
                SCARD_E_NO_READERS_AVAILABLE = 0x8010002E,
                SCARD_E_COMM_DATA_LOST = 0x8010002F,
                SCARD_E_NO_KEY_CONTAINER = 0x80100030,
                SCARD_E_SERVER_TOO_BUSY = 0x80100031,
                SCARD_E_UNSUPPORTED_FEATURE = 0x8010001F,
                //'The F...not sure
                SCARD_F_INTERNAL_ERROR = 0x80100001,
                SCARD_F_COMM_ERROR = 0x80100013,
                SCARD_F_UNKNOWN_ERROR = 0x80100014,
                //'The W...not sure
                SCARD_W_UNSUPPORTED_CARD = 0x80100065,
                SCARD_W_UNRESPONSIVE_CARD = 0x80100066,
                SCARD_W_UNPOWERED_CARD = 0x80100067,
                SCARD_W_RESET_CARD = 0x80100068,
                SCARD_W_REMOVED_CARD = 0x80100069,
                SCARD_W_SECURITY_VIOLATION = 0x8010006A,
                SCARD_W_WRONG_CHV = 0x8010006B,
                SCARD_W_CHV_BLOCKED = 0x8010006C,
                SCARD_W_EOF = 0x8010006D,
                SCARD_W_CANCELLED_BY_USER = 0x8010006E,
                SCARD_W_CARD_NOT_AUTHENTICATED = 0x8010006F,
                SCARD_W_INSERTED_CARD = 0x8010006A,
            }

            public enum CardState
            {
                //Unaware
                None = 0,
                Ignore = 1,
                Changed = 2,
                Unknown = 4,
                Unavailable = 8,
                Empty = 16,
                Present = 32,
                AttributeMatch = 64,
                Exclusive = 128,
                InUse = 256,
                Mute = 512,
                Unpowered = 1024
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct SCARD_READERSTATE
            {
                #region Member Fields
                //Points to the name of the reader being monitored.
                [MarshalAs(UnmanagedType.LPWStr)]
                private string _reader;
                //Not used by the smart card subsystem but is used by the application.
                private IntPtr _userData;
                //Current state of reader at time of call
                private CardState _currentState;
                //State of reader after state change
                private CardState _eventState;
                //Number of bytes in the returned ATR
                [MarshalAs(UnmanagedType.U4)]
                private int _attribute;
                //ATR of inserted card, with extra alignment bytes.
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
                private byte[] _rgbAtr;
                #endregion

                #region Methods
                public byte[] RGBAttribute()
                {
                    return this._rgbAtr;
                }
                #endregion

                #region "Properties"
                public string Reader
                {
                    get { return this._reader; }
                    set { this._reader = value; }
                }

                public IntPtr UserData
                {
                    get { return this._userData; }
                    set { this._userData = value; }
                }

                public CardState CurrentState
                {
                    get { return this._currentState; }
                    set { this._currentState = value; }
                }

                public CardState EventState
                {
                    get { return this._eventState; }
                }
                #endregion
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SCARD_IO_REQUEST
            {
                public int dwProtocol;
                public int cbPciLength;
            }

            [DllImport("WINSCARD.DLL", EntryPoint = "SCardGetStatusChange", CharSet = CharSet.Unicode,
            SetLastError = true)]
            public static extern uint GetStatusChange(IntPtr context, int timeout, [In(), Out()] SCARD_READERSTATE[] states, int count);


            [DllImport("winscard.dll")]
            public static extern int SCardTransmit(IntPtr hCard, ref SCARD_IO_REQUEST pioSendRequest, ref byte SendBuff, int SendBuffLen, ref SCARD_IO_REQUEST pioRecvRequest, ref byte RecvBuff, ref int RecvBuffLen);


            [DllImport("winscard.dll", EntryPoint = "SCardListReadersA", CharSet = CharSet.Ansi)]
            public static extern int SCardListReaders(IntPtr hContext, IntPtr mszGroups, byte[] mszReaders, ref UInt32 pcchReaders);

            [DllImport("WinScard.dll")]
            public static extern int SCardEstablishContext(uint dwScope, IntPtr notUsed1, IntPtr notUsed2, out IntPtr phContext);

            [DllImport("WinScard.dll")]
            public static extern int SCardReleaseContext(IntPtr phContext);

            [DllImport("WinScard.dll")]
            public static extern int SCardConnect(IntPtr hContext, string cReaderName, uint dwShareMode, uint dwPrefProtocol, ref IntPtr phCard, ref IntPtr ActiveProtocol);

            [DllImport("WinScard.dll")]
            public static extern int SCardDisconnect(IntPtr hCard, int Disposition);

            [DllImport("winscard.dll")]
            public static extern int SCardStatus(IntPtr hCard, IntPtr szReaderName, ref int pcchReaderLen, ref int pdwState, ref uint pdwProtocol, byte[] pbAtr, ref int pcbAtrLen);
            [DllImport("winscard.dll")]
            public static extern int SCardBeginTransaction(IntPtr hCard);
            [DllImport("winscard.dll")]
            public static extern int SCardEndTransaction(IntPtr hCard, int dwDisposition);
        }

        #region Mifare
        internal static byte[] SendCmd(IntPtr card, byte[] sendBytes)
        {
            byte[] receivedUID = new byte[255];

            API.SCARD_IO_REQUEST request = new API.SCARD_IO_REQUEST();
            request.dwProtocol = API.SCARD_PCI_T1;
            request.cbPciLength = Marshal.SizeOf(typeof(API.SCARD_IO_REQUEST));

            int outBytes = receivedUID.Length;
            API.SCardFunctionReturnCodes status =
                (API.SCardFunctionReturnCodes)API.SCardTransmit(card, ref request, ref sendBytes[0], sendBytes.Length, ref request, ref receivedUID[0], ref outBytes);
            if (status == API.SCardFunctionReturnCodes.SCARD_S_SUCCESS)
            {
                return receivedUID.Take(outBytes).ToArray();
            }
            return null;
        }
        #endregion

        #region Dnie
        static bool SCardReadFile(IntPtr card, byte[] path, Stream tStream)
        {
            byte[] buffer = new byte[260];
            int l = 260;

            if (!SendDnieCmd(card, new byte[] { 0x00, 0xA4, 0x00, 0x00, 0x02, 0x50, 0x15 }, buffer, ref l)) return false;

            if (!SendDnieCmd(card, new byte[] { 0x00, 0xA4, 0x00, 0x00, 0x02, path[0], path[1] }, buffer, ref l)) return false;

            l = 260;

            // GET RESPONSE
            if (!SendDnieCmd(card, new byte[] { 0x00, 0xC0, 0x00, 0x00, 0x0E }, buffer, ref l)) return false;
            if (l != 0x10 || buffer[0] != 0x6F) return false;

            // Obtenemos el tamaño
            ushort size = NFCHelper.ToUInt16(new byte[] { buffer[8], buffer[7] }, 0);

            for (ushort offset = 0; offset < size;)
            {
                byte[] s = NFCHelper.GetBytesUInt16(offset);
                byte lee = (byte)Math.Min(size - offset, 0xff);

                l = 258;
                if (!SendDnieCmd(card, new byte[] { 0x00, 0xB0, s[1], s[0], lee }, buffer, ref l))
                    return false;
                offset += lee;

                if (l > 2)
                {
                    tStream.Write(buffer, 0, l - 2);
                    //tStream.Write(buffer, 2, l - 2 /*- 2*/);
                }
            }

            return true;
        }
        static bool SendDnieCmd(IntPtr card, byte[] cmd, byte[] buffer, ref int l)
        {
            API.SCARD_IO_REQUEST request = new API.SCARD_IO_REQUEST();
            request.dwProtocol = API.SCARD_PCI_T0;
            request.cbPciLength = Marshal.SizeOf(typeof(API.SCARD_IO_REQUEST));

            API.SCardFunctionReturnCodes status = (API.SCardFunctionReturnCodes)API.SCardTransmit(card, ref request, ref cmd[0], cmd.Length, ref request, ref buffer[0], ref l);
            if (status == API.SCardFunctionReturnCodes.SCARD_S_SUCCESS)
            {
                if (l == 2)
                {
                    bool result = (buffer)[0] == 0x61 || ((buffer)[0] == 0x90 && (buffer)[1] == 0x00);
                    return result;
                }
                return true;
            }
            else return false;
        }
        #endregion

        static byte[] GetAtr(IntPtr card, string nameReader, uint pw)
        {
            int dwAtrLen = 0;
            byte[] bAtr = null;

            IntPtr theIntPtr = Marshal.StringToHGlobalUni(nameReader);
            int rl = nameReader.Length + 2; // Dos \0 al final mas
            int st = 0;

            API.SCardFunctionReturnCodes status = (API.SCardFunctionReturnCodes)API.SCardStatus(card, theIntPtr, ref rl, ref st, ref pw, bAtr, ref dwAtrLen);

            // Se repite y ya va por que devuelve la primera vez el tamaño
            if (status == API.SCardFunctionReturnCodes.SCARD_E_INSUFFICIENT_BUFFER)
                status = (API.SCardFunctionReturnCodes)API.SCardStatus(card, theIntPtr, ref rl, ref st, ref pw, bAtr, ref dwAtrLen);

            if (status != API.SCardFunctionReturnCodes.SCARD_S_SUCCESS || dwAtrLen != 20)
            {
                Marshal.Release(theIntPtr);
                return null;
            }

            bAtr = new byte[dwAtrLen];
            status = (API.SCardFunctionReturnCodes)API.SCardStatus(card, theIntPtr, ref rl, ref st, ref pw, bAtr, ref dwAtrLen);

            Marshal.Release(theIntPtr);

            if (status != API.SCardFunctionReturnCodes.SCARD_S_SUCCESS) return null;

            return bAtr;
        }

        #endregion

        string _Name;
        internal IntPtr _hCard = IntPtr.Zero;
        IntPtr _hContext = IntPtr.Zero;
        IntPtr _ActiveProtocol = IntPtr.Zero;
        internal bool _IsCardInserted = false;

        /// <summary>
        /// Nombre del lector
        /// </summary>
        public string Name { get { return _Name; } }
        /// <summary>
        /// Devuelve si está o no la tarjeta insertada
        /// </summary>
        public bool IsCardInserted { get { return _IsCardInserted; } }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Nombre del lector</param>
        internal CardReader(string name, IntPtr context)
        {
            _Name = name;
            _hContext = context;
        }

        /// <summary>
        /// Obtiene el UID de una tarjeta
        /// </summary>
        /// <param name="card">Tarjeta obtenida</param>
        /// <param name="config">Configuración de lectura de la tarjeta</param>
        /// <returns>Devuelve el tipo de retorno de la lectura</returns>
        public bool GetCard(out ICard card, ICardReadConfig config)
        {
            return GetCard(out card, null, config);
        }
        /// <summary>
        /// Obtiene el UID de una tarjeta
        /// </summary>
        /// <param name="card">Tarjeta obtenida</param>
        /// <param name="bAtr">ATR de la tarjeta</param>
        /// <param name="config">Configuración de lectura de la tarjeta</param>
        /// <returns>Devuelve el tipo de retorno de la lectura</returns>
        public bool GetCard(out ICard card, byte[] bAtr, ICardReadConfig config)
        {
            card = null;

            if (bAtr == null)
            {
                bAtr = GetAtr(_hCard, _Name, API.SCARD_PCI_T0 | API.SCARD_PCI_T1);
                if (bAtr == null) return false;
            }
            if (IsDniE(bAtr))
            {
                // Leer el Contenido del DNIe

                //http://delphi.jmrds.com/node/78
                //https://social.msdn.microsoft.com/Forums/es-ES/044965fe-5a3c-4ec9-8d30-3880cc6d420a/traducir-cdigo-c-as-vbnet?forum=vcses
                //https://social.msdn.microsoft.com/Forums/es-ES/24a95872-8207-499c-b158-167991d00343/lector-de-tarjetas?forum=vbes

                using (MemoryStream ms = new MemoryStream())
                {
                    if (SCardReadFile(_hCard, new byte[] { 0x60, 0x04 }, ms))
                    {
                        Asn1Parser a = new Asn1Parser();
                        a.LoadData(ms);

                        Asn1Node pais = a.GetNodeByPath("/2/0/1/0/0/1");
                        Asn1Node niff = a.GetNodeByPath("/2/0/1/1/0/1");
                        Asn1Node fame = a.GetNodeByPath("/2/0/1/2/0/1");
                        Asn1Node xame = a.GetNodeByPath("/2/0/1/3/0/1");
                        Asn1Node name = a.GetNodeByPath("/2/0/1/4/0/1");

                        string snif = Encoding.UTF8.GetString(niff.Data);
                        string spais = Encoding.UTF8.GetString(pais.Data);
                        string ssname = Encoding.UTF8.GetString(xame.Data);
                        string fname = Encoding.UTF8.GetString(fame.Data);
                        string sname = Encoding.UTF8.GetString(name.Data).Replace("(AUTENTICACIÓN)", "").Trim();

                        card = new CardDnie(bAtr) { Id = snif, Country = spais, CompleteName = sname, Name = ssname, Surname1 = fname };
                    }
                }

                // Leemos el IDESP con la ruta 0006
                // Leemos la version con la ruta 2F03
                // Leemos el CDF con la ruta 5015 6004
            }
            else
            {
                // Buscar tipo de tarjeta Mifare según el ATR

                CardMifare.EMifareType type = CardMifare.EMifareType.Unknown;
                if (bAtr.Length >= 18)
                {
                    if (bAtr[0] == 0x3B)
                    {
                        // http://downloads.acs.com.hk/drivers/en/API-ACR122U-2.02.pdf

                        // Mifare
                        if (bAtr[13] == 0)
                        {
                            switch (bAtr[14])
                            {
                                case 0x01: type = CardMifare.EMifareType.Classic1K; break;
                                case 0x02: type = CardMifare.EMifareType.Classic4K; break;
                                case 0x03: type = CardMifare.EMifareType.UltraLight; break;
                                case 0x26: type = CardMifare.EMifareType.Mini; break;
                            }
                        }
                    }
                }

                ConfigMifareRead cfg = null;
                if (config != null)
                {
                    if (!(config is ConfigMifareRead))
                        throw (new ArgumentException("Config must be ConfigMifareRead for Mifare Reads", "config"));

                    cfg = (ConfigMifareRead)config;
                }

                // Get UID command for Mifare cards
                byte[] receivedUID = SendCmd(_hCard, new byte[] { 0xFF, 0xCA, 0x00, 0x00, 0x00 });
                if (receivedUID == null) return false;

                card = new CardMifare(type, bAtr) { Id = NFCHelper.Buffer2Hex(receivedUID, 0, receivedUID.Length - 2).ToUpperInvariant(), };

                if (cfg != null && cfg.RequireReadSomething)
                {
                    CardMifare xcard = (CardMifare)card;

                    // Creamos la tarjeta según su tipo
                    xcard.InitCard();

                    byte[] data;

                    // Establecemos las claves en el lector
                    if (cfg.KeysZero != null)
                    {
                        // Cargar la K0
                        data = SendCmd(_hCard, new byte[] { 0xFF, 0x82, 0x00, 0x00, 0x06, }.Concat(cfg.KeysZero).ToArray());
                        if (data == null || data.Length != 2) return false;
                    }

                    if (cfg.KeysOne != null)
                    {
                        // Cargar la K1
                        data = SendCmd(_hCard, new byte[] { 0xFF, 0x82, 0x00, 0x01, 0x06, }.Concat(cfg.KeysOne).ToArray());
                        if (data == null || data.Length != 2) return false;
                    }

                    // Leer los sectores que se precisan
                    //byte blockNum;
                    ConfigMifareReadSector[] readSectors = cfg.ReadSectors;

                    foreach (CardMifare.Sector sector in xcard.Sectors)
                    {
                        ConfigMifareReadSector readCfg = readSectors[sector.SectorNum];
                        if (readCfg == null || !readCfg.ReadSomething) continue;

                        // General Authenticate - Peer sector

                        if (readCfg.Login != null)
                        {
                            // Login al primer sector
                            data = SendCmd(_hCard, new byte[] { 0xFF, 0x86, 0x00, 0x00, 0x05, 0x01, 0x00, sector.DataBlocks[0].BlockNum,
                                           (byte)(readCfg.Login.KeyType == ConfigMifareRead.EKeyType.A ? 0x60 : 0x61),
                                           (byte)(readCfg.Login.KeyNum == ConfigMifareRead.EKeyNum.Zero ? 0x00 : 0x01) });
                        }

                        List<CardMifare.Block> bRead = new List<CardMifare.Block>();

                        if (readCfg.ReadDataBlocks)
                        {
                            for (int x = (int)readCfg.ReadDataBlockStart, m = (int)readCfg.ReadDataBlockEnd; x <= m; x++)
                                bRead.Add(sector.DataBlocks[x]);
                        }
                        if (readCfg.ReadTrailBlock) bRead.Add(sector.TrailingBlock);

                        //if (data != null && data.Length == 2) 
                        foreach (CardMifare.Block block in bRead)
                        {
                            //blockNum = block.BlockNum;// byte.Parse(block.BlockNum.ToString().PadLeft(2, '0'), NumberStyles.HexNumber);

                            //if (cfg.Login != null)
                            //{
                            //    data = SendCmd(_hCard, new byte[] { 0xFF, 0x86, 0x00, 0x00, 0x05, 0x01, 0x00, block.BlockNum, 
                            //       (byte)(cfg.Login.KeyType == ConfigMifareRead.EKeyType.A ? 0x60 : 0x61),
                            //       (byte)(cfg.Login.KeyNum == ConfigMifareRead.EKeyNum.Zero ? 0x00 : 0x01) });

                            //    if (data == null || data.Length != 2) continue;
                            //}

                            // Read Binary
                            data = SendCmd(_hCard, new byte[] { 0xFF, 0xB0, 0x00, block.BlockNum, 0x10 });
                            if (data != null && data.Length == 18)
                            {
                                block.CopyFrom(data);
                            }
                        }
                    }
                }
            }

            // Eliminar ids vacios
            if (card != null && card.Id.Trim('0') == "") card = null;

            return card != null;
        }

        /// <summary>
        /// Comprobar si es DNIe (ATR)
        /// </summary>
        /// <param name="bAtr">ATR de la tarjeta</param>
        bool IsDniE(byte[] bAtr)
        {
            if (bAtr == null || bAtr.Length < 20) return false;

            if (!(bAtr[0] == 0x3B && bAtr[1] == 0x7F && bAtr[3] == 0x00 && bAtr[4] == 0x00 && bAtr[5] == 0x00 && bAtr[6] == 0x6A && bAtr[7] == 0x44 &&
                bAtr[8] == 0x4E && bAtr[9] == 0x49 && bAtr[10] == 0x65 && bAtr[18] == 0x90 && bAtr[19] == 0x00))
                return false;

            return true;
        }
        /// <summary>
        /// Libera los recursos
        /// </summary>
        public void Dispose() { Disconnect(); }
        /// <summary>
        /// Conecta el lector
        /// </summary>
        /// <param name="type">Tipo de tarjetas que se leeran</param>
        public EConnection Connect()
        {
            try
            {
                Disconnect();
                _ActiveProtocol = new IntPtr(1);//.Zero;

                API.SCardFunctionReturnCodes result;

                // Establish context
                //result = (API.SCardFunctionReturnCodes)API.SCardEstablishContext(API.SCARD_SCOPE_SYSTEM/*SCARD_SCOPE_USER*/, IntPtr.Zero, IntPtr.Zero, out _hContext);

                // using mode =2

                uint dwProtocol = API.SCARD_PROTOCOL_T0 | API.SCARD_PROTOCOL_T1;
                //                (uint)(type == ECardType.DniE ? API.SCARD_PROTOCOL_T0 : API.SCARD_PROTOCOL_T1);
                result = (API.SCardFunctionReturnCodes)API.SCardConnect(_hContext, _Name, API.SCARD_SHARE_SHARED, dwProtocol, ref _hCard, ref _ActiveProtocol);

                switch (result)
                {
                    case API.SCardFunctionReturnCodes.SCARD_W_REMOVED_CARD: return EConnection.NotCard;
                    case API.SCardFunctionReturnCodes.SCARD_S_SUCCESS: return EConnection.Ok;
                    default: return EConnection.Error;
                }
            }
            catch
            {
                Disconnect();
            }

            return EConnection.Error;
        }

        /// <summary>
        /// Desconecta el lector
        /// </summary>
        public void Disconnect()
        {
            if (_hCard != IntPtr.Zero)
            {
                API.SCardDisconnect(_hCard, API.SCARD_RESET_CARD);
                _hCard = IntPtr.Zero;
            }
            //if (_hContext != IntPtr.Zero)
            //{
            //    int ret = API.SCardReleaseContext(_hContext);
            //    _hContext = IntPtr.Zero;
            //}
        }
    }
}