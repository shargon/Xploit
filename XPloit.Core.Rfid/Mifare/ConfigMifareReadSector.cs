namespace Xploit.Core.Rfid.Mifare
{
    public class ConfigMifareReadSector
    {
        public enum EBlockRange : int
        {
            /// <summary>
            /// No leer ningún bloque
            /// </summary>
            NoRead = -1,
            /// <summary>
            /// Leer el primer bloque del sector
            /// </summary>
            DataBlock01 = 0,
            /// <summary>
            /// Leeer el segundo bloque del sector
            /// </summary>
            DataBlock02 = 1,
            /// <summary>
            /// Leer el tercer bloque del sector
            /// </summary>
            DataBlock03 = 2,
            /// <summary>
            /// Leer el bloque 4 del sector (solo 4k sectores superiores al 32)
            /// </summary>
            DataBlock04 = 3,
            /// <summary>
            /// Leer el bloque 5 del sector (solo 4k sectores superiores al 32)
            /// </summary>
            DataBlock05 = 4,
            /// <summary>
            /// Leer el bloque 6 del sector (solo 4k sectores superiores al 32)
            /// </summary>
            DataBlock06 = 5,
            /// <summary>
            /// Leer el bloque 7 del sector (solo 4k sectores superiores al 32)
            /// </summary>
            DataBlock07 = 6,
            /// <summary>
            /// Leer el bloque 8 del sector (solo 4k sectores superiores al 32)
            /// </summary>
            DataBlock08 = 7,
            /// <summary>
            /// Leer el bloque 9 del sector (solo 4k sectores superiores al 32)
            /// </summary>
            DataBlock09 = 8,
            /// <summary>
            /// Leer el bloque 10 del sector (solo 4k sectores superiores al 32)
            /// </summary>
            DataBlock10 = 9,
            /// <summary>
            /// Leer el bloque 11 del sector (solo 4k sectores superiores al 32)
            /// </summary>
            DataBlock11 = 10,
            /// <summary>
            /// Leer el bloque 12 del sector (solo 4k sectores superiores al 32)
            /// </summary>
            DataBlock12 = 11,
            /// <summary>
            /// Leer el bloque 13 del sector (solo 4k sectores superiores al 32)
            /// </summary>
            DataBlock13 = 12,
            /// <summary>
            /// Leer el bloque 14 del sector (solo 4k sectores superiores al 32)
            /// </summary>
            DataBlock14 = 13,
            /// <summary>
            /// Leer el bloque 15 del sector (solo 4k sectores superiores al 32)
            /// </summary>
            DataBlock15 = 14,
            /// <summary>
            /// Leer el bloque 16 del sector (solo 4k sectores superiores al 32)
            /// </summary>
            DataBlock16 = 15,
        }

        bool _ReadTrailBlock = false;
        EBlockRange _ReadDataBlockStart = EBlockRange.NoRead, _ReadDataBlockEnd = EBlockRange.NoRead;

        /// <summary>
        /// Bloque del sector a leer (DESDE) (1-3) (no número de bloque general)
        /// </summary>
        public EBlockRange ReadDataBlockStart { get { return _ReadDataBlockStart; } set { _ReadDataBlockStart = value; } }
        /// <summary>
        /// Bloque del sector a leer (HASTA) (1-3) (no número de bloque general)
        /// </summary>
        public EBlockRange ReadDataBlockEnd { get { return _ReadDataBlockEnd; } set { _ReadDataBlockEnd = value; } }

        /// <summary>
        /// Requiere leer el bloque Trail
        /// </summary>
        public bool ReadTrailBlock { get { return _ReadTrailBlock; } set { _ReadTrailBlock = value; } }
        /// <summary>
        /// Requiere la lectura de algún bloque de datos
        /// </summary>
        public bool ReadDataBlocks
        {
            get
            {
                return (_ReadDataBlockStart != EBlockRange.NoRead && _ReadDataBlockEnd != EBlockRange.NoRead);
            }
        }
        /// <summary>
        /// Login en el sector
        /// </summary>
        public LoginMifareMethod Login { get; set; }

        /// <summary>
        /// Devuelve si tiene que leer algún bloque
        /// </summary>
        public bool ReadSomething { get { return ReadTrailBlock || ReadDataBlocks; } }

        public override string ToString()
        {
            return ReadSomething ? "[Read]" : "[NonRead]";
        }
    }
}