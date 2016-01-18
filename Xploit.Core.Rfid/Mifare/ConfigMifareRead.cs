using System;
using Xploit.Core.Rfid.Interfaces;

namespace Xploit.Core.Rfid.Mifare
{
    public class ConfigMifareRead : ICardReadConfig
    {
        public enum EKeyType { A = 0, B = 1 };
        public enum EKeyNum { Zero = 0, One = 1 };

        byte[] _KeysZero = null;
        byte[] _KeysOne = null;

        ConfigMifareReadSector[] _ReadSectors = new ConfigMifareReadSector[40];

        /// <summary>
        /// Clave de K0
        /// </summary>
        public byte[] KeysZero
        {
            get { return _KeysZero; }
            set
            {
                if (value == null || value.Length != 6) throw (new Exception("K0 Password Error"));
                _KeysZero = value;
            }
        }
        /// <summary>
        /// Clave de K1
        /// </summary>
        public byte[] KeysOne
        {
            get { return _KeysOne; }
            set
            {
                if (value == null || value.Length != 6) throw (new Exception("K1 Password Error"));
                _KeysOne = value;
            }
        }
        /// <summary>
        /// Devuelve los sectores que se quiere leer
        /// </summary>
        public ConfigMifareReadSector[] ReadSectors { get { return _ReadSectors; } }
        /// <summary>
        /// Requiere leer algun sector, SI o NO
        /// </summary>
        public bool RequireReadSomething
        {
            get
            {
                foreach (ConfigMifareReadSector b in _ReadSectors)
                    if (b != null && b.ReadSomething) return true;

                return false;
            }
        }
        /// <summary>
        /// Devuelve o establece el sector a leer
        /// </summary>
        /// <param name="sectorNum">Número del sector</param>
        public ConfigMifareReadSector this[byte sectorNum]
        {
            get { return _ReadSectors[sectorNum]; }
            set { _ReadSectors[sectorNum] = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ConfigMifareRead()
        {
            for (int x = 0, m = _ReadSectors.Length; x < m; x++)
                _ReadSectors[x] = new ConfigMifareReadSector();
        }
    }
}