using System;

namespace Xploit.Core.Rfid.Mifare.Internal
{
    /// <summary>
    /// Key to use to login into the sector
    /// </summary>
    public enum KeyTypeEnum
    {
        KeyA,
        KeyB,
        KeyDefaultF
    }

    /// <summary>
    /// Possible card types that have been approached to the reader. Only MiFARE_1K and MiFARE_4K are currently supported
    /// </summary>
    public enum CardTypeEnum
    {
        Unknown,
        MiFARE_Light,
        MiFARE_1K,
        MiFARE_4K,
        MiFARE_DESFire,
        MiFARE_UltraLight
    };

    /// <summary>
    /// interface for a generic card reader
    /// </summary>
    public interface ICardReader
    {
        /// <summary>
        /// returns the type of the card in the reader
        /// </summary>
        /// <param name="cardType">the type of the card</param>
        /// <returns>returns true on succes, false on error or no card present</returns>
        bool GetCardType(out CardTypeEnum cardType);

        /// <summary>
        /// Login into the given sector using the given key
        /// </summary>
        /// <param name="sector">sector to login into</param>
        /// <param name="key">key to use</param>
        /// <returns>tru on success, false otherwise</returns>
        bool Login(int sector, KeyTypeEnum key);

        /// <summary>
        /// read a datablock from a sector 
        /// </summary>
        /// <param name="sector">sector to read</param>
        /// <param name="datablock">datablock to read</param>
        /// <param name="data">data read out from the datablock</param>
        /// <returns>true on success, false otherwise</returns>
        bool Read(int sector, int datablock, out byte[] data);

        /// <summary>
        /// write data in a datablock
        /// </summary>
        /// <param name="sector">sector to write</param>
        /// <param name="datablock">datablock to write</param>
        /// <param name="data">data to write. this is a 16-bytes array</param>
        /// <returns>true on success, false otherwise</returns>
        bool Write(int sector, int datablock, byte[] data);
    }

    /// <summary>
    /// interface for handling datablocks as values 
    /// </summary>
    public interface ICardValueReader
    {
        bool ReadValue(int sector, int datablock, out int value);
        bool WriteValue(int sector, int datablock, int value);
        bool IncValue(int sector, int datablock, int delta, out int value);
        bool DecValue(int sector, int datablock, int delta, out int value);
        bool CopyValue(int sector, int srcBlock, int dstBlock, out int value);
    }
}
