using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Xploit.Core.Rfid.Mifare.Internal
{
    public class TrailerAccessCondition
    {
        #region Private fields
        private static Dictionary<TrailerAccessCondition, BitArray> _Templates = null;
        #endregion

        #region Properties

        public enum ConditionEnum
        {
            Never,
            KeyA,
            KeyB,
            KeyAOrB
        }

        public ConditionEnum KeyARead;

        public ConditionEnum KeyAWrite;

        public ConditionEnum KeyBRead;

        public ConditionEnum KeyBWrite;

        public ConditionEnum AccessBitsRead;

        public ConditionEnum AccessBitsWrite;

        #endregion

        #region Public functions

        public override bool Equals(object obj)
        {
            TrailerAccessCondition tac = obj as TrailerAccessCondition;
            if (tac == null)
                return false;

            return ((tac.KeyARead == KeyARead) && (tac.KeyAWrite == KeyAWrite) &&
                (tac.KeyBRead == KeyBRead) && (tac.KeyBWrite == KeyBWrite) &&
                (tac.AccessBitsRead == AccessBitsRead) && (tac.AccessBitsWrite == AccessBitsWrite));
        }

        public override String ToString()
        {
            BitArray bits = GetBits();
            if (bits == null)
                return "Invalid";

            return bits.ToString();
        }

        #region GetBits
        public BitArray GetBits()
        {
            InitTemplates();

            foreach (KeyValuePair<TrailerAccessCondition, BitArray> kvp in _Templates)
            {
                if (kvp.Key.Equals(this))
                    return kvp.Value;
            }

            return _Templates.ElementAt(4).Value;
        }
        #endregion

        #region Initialize
        public void Initialize(TrailerAccessCondition access)
        {
            KeyARead = access.KeyARead;
            KeyAWrite = access.KeyAWrite;
            KeyBRead = access.KeyBRead;
            KeyBWrite = access.KeyBWrite;
            AccessBitsRead = access.AccessBitsRead;
            AccessBitsWrite = access.AccessBitsWrite;
        }

        public bool Initialize(BitArray bits)
        {
            InitTemplates();

            foreach (KeyValuePair<TrailerAccessCondition, BitArray> kvp in _Templates)
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

        #endregion

        #region Private functions

        private void InitTemplates()
        {
            if (_Templates != null)
                return;

            _Templates = new Dictionary<TrailerAccessCondition, BitArray>();

            _Templates.Add(new TrailerAccessCondition()
            {
                KeyARead = ConditionEnum.Never,
                KeyAWrite = ConditionEnum.KeyA,
                AccessBitsRead = ConditionEnum.KeyA,
                AccessBitsWrite = ConditionEnum.Never,
                KeyBRead = ConditionEnum.KeyA,
                KeyBWrite = ConditionEnum.KeyA
            },
            new BitArray(new bool[] { false, false, false }));

            _Templates.Add(new TrailerAccessCondition()
            {
                KeyARead = ConditionEnum.Never,
                KeyAWrite = ConditionEnum.Never,
                AccessBitsRead = ConditionEnum.KeyA,
                AccessBitsWrite = ConditionEnum.Never,
                KeyBRead = ConditionEnum.KeyA,
                KeyBWrite = ConditionEnum.Never
            },
            new BitArray(new bool[] { false, true, false }));

            _Templates.Add(new TrailerAccessCondition()
            {
                KeyARead = ConditionEnum.Never,
                KeyAWrite = ConditionEnum.KeyB,
                AccessBitsRead = ConditionEnum.KeyAOrB,
                AccessBitsWrite = ConditionEnum.Never,
                KeyBRead = ConditionEnum.Never,
                KeyBWrite = ConditionEnum.KeyB
            },
            new BitArray(new bool[] { true, false, false }));

            _Templates.Add(new TrailerAccessCondition()
            {
                KeyARead = ConditionEnum.Never,
                KeyAWrite = ConditionEnum.Never,
                AccessBitsRead = ConditionEnum.KeyAOrB,
                AccessBitsWrite = ConditionEnum.Never,
                KeyBRead = ConditionEnum.Never,
                KeyBWrite = ConditionEnum.Never
            },
            new BitArray(new bool[] { true, true, false }));

            _Templates.Add(new TrailerAccessCondition()
            {
                KeyARead = ConditionEnum.Never,
                KeyAWrite = ConditionEnum.KeyA,
                AccessBitsRead = ConditionEnum.KeyA,
                AccessBitsWrite = ConditionEnum.KeyA,
                KeyBRead = ConditionEnum.KeyA,
                KeyBWrite = ConditionEnum.KeyA
            },
            new BitArray(new bool[] { false, false, true }));

            _Templates.Add(new TrailerAccessCondition()
            {
                KeyARead = ConditionEnum.Never,
                KeyAWrite = ConditionEnum.KeyB,
                AccessBitsRead = ConditionEnum.KeyAOrB,
                AccessBitsWrite = ConditionEnum.KeyB,
                KeyBRead = ConditionEnum.Never,
                KeyBWrite = ConditionEnum.KeyB
            },
            new BitArray(new bool[] { false, true, true }));

            _Templates.Add(new TrailerAccessCondition()
            {
                KeyARead = ConditionEnum.Never,
                KeyAWrite = ConditionEnum.Never,
                AccessBitsRead = ConditionEnum.KeyAOrB,
                AccessBitsWrite = ConditionEnum.KeyB,
                KeyBRead = ConditionEnum.Never,
                KeyBWrite = ConditionEnum.Never
            },
            new BitArray(new bool[] { true, false, true }));

            _Templates.Add(new TrailerAccessCondition()
            {
                KeyARead = ConditionEnum.Never,
                KeyAWrite = ConditionEnum.Never,
                AccessBitsRead = ConditionEnum.KeyAOrB,
                AccessBitsWrite = ConditionEnum.Never,
                KeyBRead = ConditionEnum.Never,
                KeyBWrite = ConditionEnum.Never
            },
            new BitArray(new bool[] { true, true, true }));
        }

        #endregion
    }
}
