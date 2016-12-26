using System;
using System.Collections.Generic;

namespace Xploit.Core.Rfid.Mifare.Internal
{
    /// <summary>
    /// handle the MiFare application directory
    /// </summary>
    internal class MAD2
    {
        #region Private fields
        private Byte[] _Block1;
        private Byte[] _Block2;
        private Byte[] _Block3;

        private int[] _AppDir;
        #endregion

        #region Constructor
        public MAD2(Byte[] block1, Byte[] block2, Byte[] block3)
        {
            _Block1 = block1;
            _Block2 = block2;
            _Block3 = block3;

            InitDirectory();
        }
        #endregion

        #region Public functions

        #region GetAppSectors
        /// <summary>
        /// return the list of sectors reserved for the given application id
        /// </summary>
        /// <param name="appId">application id to look for</param>
        /// <returns>the indexes of the sectors reserved for the application</returns>
        public int[] GetAppSectors(int appId)
        {
            List<int> result = new List<int>();

            for (int sector=1; sector<_AppDir.Length; sector++)
            {
                if (_AppDir[sector] == appId)
                    result.Add(sector);
            }
            return result.ToArray();
        }
        #endregion

        #region AddAppId
        /// <summary>
        /// reserver a new sector for the application
        /// </summary>
        /// <param name="appId">id of the application</param>
        /// <returns>the index of the reserved sector or -1 if no sectors available</returns>
        public int AddAppId(int appId)
        {
            for (int sector = 1; sector < _AppDir.Length; sector++)
            {
                if (_AppDir[sector] == 0)
                {
                    SaveAppId(sector, appId);
                    return sector;
                }
            }

            return -1;
        }
        #endregion

        #endregion

        #region Private functions

        #region InitDirectory
        private void InitDirectory()
        {
            _AppDir = new int[24];

            int idx = 1;
            for (int byteIdx = 2; byteIdx < 16; byteIdx += 2)
            {
                int appId = (_Block1[byteIdx + 1] * 256) + _Block1[byteIdx];
                _AppDir[idx] = appId;

                idx++;
            }

            for (int byteIdx = 0; byteIdx < 16; byteIdx += 2)
            {
                int appId = (_Block2[byteIdx + 1] * 256) + _Block2[byteIdx];
                _AppDir[idx] = appId;

                idx++;
            }

            for (int byteIdx = 0; byteIdx < 16; byteIdx += 2)
            {
                int appId = (_Block3[byteIdx + 1] * 256) + _Block3[byteIdx];
                _AppDir[idx] = appId;

                idx++;
            }

            Byte crc = CalculateCRC();
            if (_Block1[0] != crc)
                crc = _Block1[0];
        }
        #endregion

        #region SaveAppId
        private void SaveAppId(int sector, int appId)
        {
            if (sector < 8)
            {
                _Block1[(sector * 2) + 0] = (Byte)(appId % 256);
                _Block1[(sector * 2) + 1] = (Byte)(appId / 256);
            }
            else if (sector < 16)
            {
                _Block2[((sector - 8) * 2) + 0] = (Byte)(appId % 256);
                _Block2[((sector-8) * 2) + 1] = (Byte)(appId / 256);
            }
            else 
            {
                _Block3[((sector - 16) * 2) + 0] = (Byte)(appId % 256);
                _Block3[((sector - 16) * 2) + 1] = (Byte)(appId / 256);
            }

            _AppDir[sector] = appId;

            _Block1[0] = CalculateCRC();
        }
        #endregion

        #region CalculareCRC
        private void Crc(ref Byte crc, Byte value)
        {
            /* x^8 + x^4 + x^3 + x^2 + 1 => 0x11d */
            const Byte poly = 0x1d;

            for (int current_bit = 7; current_bit >= 0; current_bit--)
            {
                int bit_out = crc & 0x80;
                crc = (Byte)((crc << 1) | ((value >> (current_bit)) & 0x01));

                if (bit_out != 0)
                    crc ^= poly;
            }
        }

        private Byte CalculateCRC()
        {
            Byte crc = 0x67;

            for(int i=1; i<16;i++)
                Crc(ref crc, _Block1[i]);

            for(int i=0; i<16;i++)
                Crc(ref crc, _Block2[i]);

            for (int i = 0; i < 16; i++)
                Crc(ref crc, _Block3[i]);

            Crc(ref crc, 0x00);

            return crc;
        }
        #endregion

        #endregion
    }
}
