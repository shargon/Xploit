using System;

namespace Xploit.Core.Rfid.Mifare.Internal
{
    /// <summary>
    /// Helper for MiFare card handling
    /// </summary>
    public class MiFARECard
    {
        #region Private fields
        private const int MAX_SECTORS = 40;

        private ICardReader _Reader;
        private ICardValueReader _ValueReader;
        private Sector[] _Sectors;
        private MAD _MAD;
        private MAD2 _MAD2;
        #endregion

        #region Constructor
        public MiFARECard(ICardReader reader)
        {
            _Reader = reader;
            _ValueReader = null;

            Initialize();
        }

        public MiFARECard(ICardReader reader, ICardValueReader valueReader)
        {
            _Reader = reader;
            _ValueReader = valueReader;

            Initialize();
        }
        #endregion
        
        #region Properties

        /// <summary>
        /// the sector actually active (logged in)
        /// </summary>
        internal int ActiveSector { get; set; }

        /// <summary>
        /// allows access to the ICardReader interface from Sector objects
        /// </summary>
        internal ICardReader Reader { get { return _Reader; } }

        /// <summary>
        /// allows access to the ICardValueReader interface from Sector objects
        /// </summary>
        internal ICardValueReader ValueReader { get { return _ValueReader; } }

        #endregion

        #region Public functions

        #region GetSector
        /// <summary>
        /// returns the Sector object at given position
        /// </summary>
        /// <param name="sector">index of the sector (0..39)</param>
        /// <returns>the sector object</returns>
        /// <remarks>may throw CardLoginException or CardReadException</remarks>
        public Sector GetSector(int sector)
        {
            Sector s = _Sectors[sector];
            if (s == null)
            {
                s = LoadSector(sector);
                _Sectors[sector] = s;
            }

            return s;
        }
        #endregion

        #region GetAppSectors
        /// <summary>
        /// return the list of sectors associated to the given applicaition id
        /// </summary>
        /// <param name="appId">id of the application</param>
        /// <returns>list of sectors reserved to the application</returns>
        /// <remarks>may throw CardLoginException and CardReadException</remarks>
        public int[] GetAppSectors(int appId)
        {
            int[] sector1 = null;
            int[] sector2 = null;

            InitMAD();
            if (_MAD != null)
                sector1 = _MAD.GetAppSectors(appId);

            InitMAD2();
            if (_MAD2 != null)
                sector2 = _MAD2.GetAppSectors(appId);

            int numSectors = 0;
            if (sector1 != null)
                numSectors += sector1.Length;
            if (sector2 != null)
                numSectors += sector2.Length;

            int[] sectors = new int[numSectors];
            int idx = 0;
            if (sector1 != null)
            {
                Array.Copy(sector1, sectors, sector1.Length);
                idx = sector1.Length;
            }

            if (sector2 != null)
                Array.Copy(sector2, 0, sectors, idx, sector2.Length);

            return sectors;
        }
        #endregion

        #region AddAppId
        public enum ApplicationMADEnum
        {
            Any,
            MAD,
            MAD2
        };

        /// <summary>
        /// reserve a new sector to the application. look for free sector in MAD and MAD2 if available
        /// </summary>
        /// <param name="appId">id of the application</param>
        /// <returns>index of the reserved sector or -1 if no sectors found</returns>
        /// <remarks>may throw CardLoginException and CardReadException</remarks>
        public int AddAppId(int appId)
        {
            return AddAppId(appId, ApplicationMADEnum.Any);
        }

        /// <summary>
        /// reserve a new sector to the application. look for free sector in the given MAD 
        /// </summary>
        /// <param name="appId">id of the application</param>
        /// <param name="whichMAD">MAD that will be scanned
        /// Any = scan MAD and MAD2 if available
        /// MAD = scan MAD only
        /// MAD2 = scan MAD2 only
        /// </param>
        /// <returns>index of the reserved sector or -1 if no sectors found</returns>
        /// <remarks>may throw CardLoginException and CardReadException</remarks>
        public int AddAppId(int appId, ApplicationMADEnum whichMAD)
        {
            int sector = -1;

            if ((whichMAD == ApplicationMADEnum.Any) || (whichMAD == ApplicationMADEnum.MAD))
            {
                InitMAD();
                if (_MAD != null)
                {
                    sector = _MAD.AddAppId(appId);
                    if (sector != -1)
                        return sector;
                }
            }

            if ((whichMAD == ApplicationMADEnum.Any) || (whichMAD == ApplicationMADEnum.MAD2))
            {
                InitMAD2();
                if (_MAD2 != null)
                {
                    sector = _MAD2.AddAppId(appId);
                    if (sector != -1)
                        return sector;
                }
            }

            return -1;
        }
        #endregion

        #region GetData
        /// <summary>
        /// read a block of data of any size
        /// </summary>
        /// <param name="sector">index of the sector</param>
        /// <param name="dataBlock">index of the datablock</param>
        /// <param name="length">n umber of bytes to read</param>
        /// <returns>the data read</returns>
        /// <remarks>may throw CardLoginException and CardReadException</remarks>
        public Byte[] GetData(int sector, int dataBlock, int length)
        {
            Byte[] result = new Byte[length];
            Array.Clear(result, 0, length);

            int bytesRead = 0;
            int currSector = sector;
            int currDataBlock = dataBlock;
            while (bytesRead < length)
            {
                Sector s = GetSector(currSector);

                Byte[] data = s.GetData(currDataBlock);
                if (data != null)
                    Array.Copy(data, 0, result, bytesRead, Math.Min(length-bytesRead, data.Length));

                bytesRead += DataBlock.Length;

                GetNextSectorAndDataBlock(ref currSector, ref currDataBlock);
            }

            return result;
        }
        #endregion

        #region SetData
        /// <summary>
        /// write data on card. data is stored internally, not actually written on card. use Flush to write changes on the card
        /// </summary>
        /// <param name="sector">index of the sector</param>
        /// <param name="dataBlock">index of the datablock</param>
        /// <param name="data">data to write</param>
        /// <remarks>may throw CardLoginException and CardWriteException</remarks>
        public void SetData(int sector, int dataBlock, Byte[] data)
        {
            int bytesWritten = 0;
            int currSector = sector;
            int currDataBlock = dataBlock;
            while (bytesWritten < data.Length)
            {
                Sector s = GetSector(currSector);

                Byte[] block = new Byte[DataBlock.Length];
                Array.Copy(data, bytesWritten, block, 0, Math.Min(data.Length-bytesWritten, block.Length));

                s.SetData(block, currDataBlock);

                bytesWritten += DataBlock.Length;

                GetNextSectorAndDataBlock(ref currSector, ref currDataBlock);
            }
        }
        #endregion

        #region Abort
        /// <summary>
        /// reinitialize the object
        /// </summary>
        public void Abort()
        {
            Initialize();
        }
        #endregion

        #region Flush
        /// <summary>
        /// write all changed datblock on the card
        /// </summary>
        /// <remarks>may throw CardLoginException and CardWriteException</remarks>
        public void Flush()
        {
            foreach (Sector s in _Sectors)
            {
                if (s == null)
                    continue;

                s.Flush();
            }

            Initialize();
        }
        #endregion

        #region Value access functions

        #region ReadValue
        public int ReadValue(int sector, int datablock)
        {
            Sector s = GetSector(sector);
            return s.ReadValue(datablock);
        }
        #endregion

        #region WriteValue
        public void WriteValue(int sector, int datablock, int value)
        {
            Sector s = GetSector(sector);
            s.WriteValue(datablock, value);
        }
        #endregion

        #region IncValue
        public int IncValue(int sector, int datablock, int delta)
        {
            Sector s = GetSector(sector);
            return s.IncValue(datablock, delta);
        }
        #endregion

        #region DecValue
        public int DecValue(int sector, int datablock, int delta)
        {
            Sector s = GetSector(sector);
            return s.DecValue(datablock, delta);
        }
        #endregion

        #region CopyValue
        public int CopyValue(int sector, int srcBlock, int dstBlock)
        {
            Sector s = GetSector(sector);
            return s.CopyValue(srcBlock, dstBlock);
        }
        #endregion

        #endregion

        #endregion

        #region Private functions

        #region InitMAD
        private void InitMAD()
        {
            if (_MAD != null)
                return;

            // load sector 1, block 2 and 3
            Sector sector0 = GetSector(0);
            if (sector0.Access.MADVersion == AccessConditions.MADVersionEnum.NoMAD)
                return;

            Byte[] dataBlock1 = sector0.GetData(1);
            Byte[] dataBlock2 = sector0.GetData(2);

            _MAD = new MAD(dataBlock1, dataBlock2);
        }
        #endregion

        #region InitMAD2
        private void InitMAD2()
        {
            if (_MAD2 != null)
                return;

            // load sector 1, block 2 and 3
            Sector sector0 = GetSector(0);
            if (sector0.Access.MADVersion != AccessConditions.MADVersionEnum.Version2)
                return;

            Sector sector16 = GetSector(16);

            Byte[] dataBlock1 = sector16.GetData(0);
            Byte[] dataBlock2 = sector16.GetData(1);
            Byte[] dataBlock3 = sector16.GetData(2);

            _MAD2 = new MAD2(dataBlock1, dataBlock2, dataBlock3);
        }
        #endregion

        #region LoadSector
        private Sector LoadSector(int index)
        {
            Sector s = new Sector(this, index);
            return s;
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            _Sectors = new Sector[MAX_SECTORS];
            _MAD = null;
            _MAD2 = null;

            ActiveSector = -1;
        }
        #endregion

        #region GetNextSectorAndDataBlock
        private void GetNextSectorAndDataBlock(ref int sector, ref int dataBlock)
        {
            dataBlock++;

            Sector s = GetSector(sector);
            if (dataBlock >= s.NumDataBlocks)
            {
                sector++;
                dataBlock = 0;
            }
        }
        #endregion

        #endregion
    }
}