using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xploit.Core.Rfid.Mifare.Internal
{
    /// <summary>
    /// Class that handles the datablocks in a sector
    /// </summary>
    internal class DataBlock
    {
        #region Private fields
        protected Byte[] _Data;
        protected int _Number;
        protected bool _IsTrailer;

        protected Byte[] _OrigData;
        #endregion

        #region Constructor
        public DataBlock(int number, Byte[] data)
        {
            _Number = number;
            _Data = data;
            _IsTrailer = false;

            _OrigData = new Byte[_Data.Length];
            Array.Copy(_Data, _OrigData, _Data.Length);
        }

        public DataBlock(int number, Byte[] data, bool isTrailer)
        {
            _Number = number;
            _Data = data;
            _IsTrailer = isTrailer;

            _OrigData = new Byte[_Data.Length];
            Array.Copy(_Data, _OrigData, _Data.Length);
        }
        #endregion

        #region Properties

        #region Length
        public const int Length = 16;
        #endregion

        #region Number
        public int Number { get { return _Number; } }
        #endregion

        #region Data
        public Byte[] Data
        {
            get { return _Data; }
        }
        #endregion

        #region IsTrailer
        public bool IsTrailer
        {
            get { return _IsTrailer; }
        }
        #endregion

        #region IsChanged
        public bool IsChanged
        {
            get
            {
                return (!_Data.Equals(_OrigData));
            }
        }
        #endregion

        #endregion

        #region Public functions
        #endregion
    }
}
