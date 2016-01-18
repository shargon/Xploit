using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Xploit.Core.Rfid.Mifare.Internal
{
    /// <summary>
    /// Handle access condition for a generic datablock of a sector
    /// </summary>
    public class DataAreaAccessCondition
    {
        #region Private fields
        /// <summary>
        /// Dictionary that associate an AccessConditionsSet to a bit array of C1-C2-C3 bits
        /// (see MiFare specs for the meaning of C1-C2-C3)
        /// </summary>
        private static Dictionary<DataAreaAccessCondition, BitArray> _Templates = null;
        #endregion

        public DataAreaAccessCondition()
        {
            Read = ConditionEnum.KeyAOrB;
            Write = ConditionEnum.KeyAOrB;
            Increment = ConditionEnum.KeyAOrB;
            Decrement = ConditionEnum.KeyAOrB;
        }

        #region Properties

        /// <summary>
        /// List of access conditions that may apply to each operation (read, write, inc, dec)
        /// </summary>
        public enum ConditionEnum
        {
            Never,
            KeyA,
            KeyB,
            KeyAOrB
        }

        /// <summary>
        /// Access condition for read operations on the data block
        /// </summary>
        public ConditionEnum Read;

        /// <summary>
        /// Access condition for write operations on the data block
        /// </summary>
        public ConditionEnum Write;

        /// Access condition for increment operations on the data block
        public ConditionEnum Increment;

        /// Access condition for decrement operations on the data block
        public ConditionEnum Decrement;

        #endregion

        #region Public functions

        public override bool Equals(object obj)
        {
            DataAreaAccessCondition daac = obj as DataAreaAccessCondition;
            if (daac == null)
                return false;

            return ((daac.Read == Read) && (daac.Write == Write) && (daac.Increment == Increment) && (daac.Decrement == Decrement));
        }

        public override string ToString()
        {
            BitArray bits = GetBits();
            if (bits == null)
                return "Invalid";

            return bits.ToString();
        }

        #endregion

        #region Private fields

        private void InitTemplates()
        {
            if (_Templates != null)
                return;

            _Templates = new Dictionary<DataAreaAccessCondition, BitArray>();

            _Templates.Add(new DataAreaAccessCondition()
            {
                Read = ConditionEnum.KeyAOrB,
                Write = ConditionEnum.KeyAOrB,
                Increment = ConditionEnum.KeyAOrB,
                Decrement = ConditionEnum.KeyAOrB
            },
            new BitArray(new bool[] { false, false, false }));

            _Templates.Add(new DataAreaAccessCondition()
            {
                Read = ConditionEnum.KeyAOrB,
                Write = ConditionEnum.Never,
                Increment = ConditionEnum.Never,
                Decrement = ConditionEnum.Never
            },
            new BitArray(new bool[] { false, true, false }));

            _Templates.Add(new DataAreaAccessCondition()
            {
                Read = ConditionEnum.KeyAOrB,
                Write = ConditionEnum.KeyB,
                Increment = ConditionEnum.Never,
                Decrement = ConditionEnum.Never
            },
            new BitArray(new bool[] { true, false, false }));

            _Templates.Add(new DataAreaAccessCondition()
            {
                Read = ConditionEnum.KeyAOrB,
                Write = ConditionEnum.KeyB,
                Increment = ConditionEnum.KeyB,
                Decrement = ConditionEnum.KeyAOrB
            },
            new BitArray(new bool[] { true, true, false }));

            _Templates.Add(new DataAreaAccessCondition()
            {
                Read = ConditionEnum.KeyAOrB,
                Write = ConditionEnum.Never,
                Increment = ConditionEnum.Never,
                Decrement = ConditionEnum.KeyAOrB
            },
            new BitArray(new bool[] { false, false, true }));

            _Templates.Add(new DataAreaAccessCondition()
            {
                Read = ConditionEnum.KeyB,
                Write = ConditionEnum.KeyB,
                Increment = ConditionEnum.Never,
                Decrement = ConditionEnum.Never
            },
            new BitArray(new bool[] { false, true, true }));

            _Templates.Add(new DataAreaAccessCondition()
            {
                Read = ConditionEnum.KeyB,
                Write = ConditionEnum.Never,
                Increment = ConditionEnum.Never,
                Decrement = ConditionEnum.Never
            },
            new BitArray(new bool[] { true, false, true }));

            _Templates.Add(new DataAreaAccessCondition()
            {
                Read = ConditionEnum.Never,
                Write = ConditionEnum.Never,
                Increment = ConditionEnum.Never,
                Decrement = ConditionEnum.Never
            },
            new BitArray(new bool[] { true, true, true }));
        }

        /// <summary>
        /// convert the object to the corresponding C1-C2-C3 bits
        /// </summary>
        /// <returns>a 3-elements bit array</returns>
        internal BitArray GetBits()
        {
            InitTemplates();

            foreach (KeyValuePair<DataAreaAccessCondition, BitArray> kvp in _Templates)
            {
                if (kvp.Key.Equals(this))
                    return kvp.Value;
            }

            return _Templates.ElementAt(0).Value;
        }

        /// <summary>
        /// Initialize the object based on a DataAreaAccessCondition
        /// </summary>
        /// <param name="access">the DataAreaAccessCondition to clone</param>
        internal void Initialize(DataAreaAccessCondition access)
        {
            Read = access.Read;
            Write = access.Write;
            Increment = access.Increment;
            Decrement = access.Decrement;
        }

        /// <summary>
        /// Initialize object based on a bit array of C1-C2-C3
        /// </summary>
        /// <param name="bits">C1-C2-C3 bit array</param>
        /// <returns></returns>
        internal bool Initialize(BitArray bits)
        {
            InitTemplates();

            foreach (KeyValuePair<DataAreaAccessCondition, BitArray> kvp in _Templates)
            {
                if (kvp.Value.IsEqual(bits))
                {
                    Initialize(kvp.Key);
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
