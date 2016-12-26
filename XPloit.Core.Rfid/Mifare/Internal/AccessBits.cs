using System;
using System.Text;
using System.Collections;

namespace Xploit.Core.Rfid.Mifare.Internal
{
    /// <summary>
    /// Internal class for encoding/decoding the 4 control bytes in the trailer datablock of each sector
    /// </summary>
    internal class AccessBits
    {
        #region Public functions

        /// <summary>
        /// Calculate the 4 control bytes in the trailer datablock of each sector according to the given AccessConditions
        /// </summary>
        /// <param name="access">AccessConditions to encode</param>
        /// <returns>a 4-bytes array</returns>
        public static Byte[] CalculateAccessBits(AccessConditions access)
        {
            BitArray[] bitConds = new BitArray[4];
            bitConds[0] = access.DataAreas[0].GetBits();
            bitConds[1] = access.DataAreas[1].GetBits();
            bitConds[2] = access.DataAreas[2].GetBits();
            bitConds[3] = access.Trailer.GetBits();

            PrintValues(bitConds[0], 8);
            PrintValues(bitConds[1], 8);
            PrintValues(bitConds[2], 8);
            PrintValues(bitConds[3], 8);

            // build a bit array for the first byte (byte 6 of trailer datablock)
            BitArray byte6 = new BitArray(8);
            byte6.Set(0, !bitConds[0].Get(0)); // ! C1-0 
            byte6.Set(1, !bitConds[1].Get(0)); // ! C1-1
            byte6.Set(2, !bitConds[2].Get(0)); // ! C1-2
            byte6.Set(3, !bitConds[3].Get(0)); // ! C1-3
            byte6.Set(4, !bitConds[0].Get(1)); // ! C2-0
            byte6.Set(5, !bitConds[1].Get(1)); // ! C2-1
            byte6.Set(6, !bitConds[2].Get(1)); // ! C2-2
            byte6.Set(7, !bitConds[3].Get(1)); // ! C2-3

            // build a bit array for the second byte (byte 7 of trailer datablock)
            BitArray byte7 = new BitArray(8);
            byte7.Set(0, !bitConds[0].Get(2)); // ! C3-0 
            byte7.Set(1, !bitConds[1].Get(2)); // ! C3-1
            byte7.Set(2, !bitConds[2].Get(2)); // ! C3-2
            byte7.Set(3, !bitConds[3].Get(2)); // ! C3-3
            byte7.Set(4, bitConds[0].Get(0)); // C1-0
            byte7.Set(5, bitConds[1].Get(0)); // C1-1
            byte7.Set(6, bitConds[2].Get(0)); // C1-2
            byte7.Set(7, bitConds[3].Get(0)); // C1-3

            // build a bit array for the third byte (byte 8 of trailer datablock)
            BitArray byte8 = new BitArray(8);
            byte8.Set(0, bitConds[0].Get(1)); // C2-0 
            byte8.Set(1, bitConds[1].Get(1)); // C2-1
            byte8.Set(2, bitConds[2].Get(1)); // C2-2
            byte8.Set(3, bitConds[3].Get(1)); // C2-3
            byte8.Set(4, bitConds[0].Get(2)); // C3-0
            byte8.Set(5, bitConds[1].Get(2)); // C3-1
            byte8.Set(6, bitConds[2].Get(2)); // C3-2
            byte8.Set(7, bitConds[3].Get(2)); // C3-3

            // build GPB byte
            BitArray byte9 = new BitArray(8);
            if (access.MADVersion == AccessConditions.MADVersionEnum.Version1)
            {
                byte9.Set(0, true);
                byte9.Set(1, false);
                byte9.Set(7, true);
            }
            else if (access.MADVersion == AccessConditions.MADVersionEnum.Version2)
            {
                byte9.Set(0, false);
                byte9.Set(1, true);
                byte9.Set(7, true);
            }

            byte9.Set(6, access.MultiApplicationCard);

            Byte[] bits = new Byte[4];
            byte6.CopyTo(bits, 0);
            byte7.CopyTo(bits, 1);
            byte8.CopyTo(bits, 2);
            byte9.CopyTo(bits, 3);

            return bits;
        }

        /// <summary>
        /// Decode the 4 access control bytes
        /// </summary>
        /// <param name="data">a 4-bytes array to decode</param>
        /// <returns>an initialized AccessConditions object</returns>
        public static AccessConditions GetAccessConditions(Byte[] data)
        {
            BitArray byte6 = new BitArray(new Byte[] { 0xFF });
            BitArray byte7 = new BitArray(new Byte[] { 0x07 });
            BitArray byte8 = new BitArray(new Byte[] { 0x80 });
            BitArray byte9 = new BitArray(new Byte[] { 0x69 });

            if (data != null)
            {
                byte6 = new BitArray(new Byte[] { data[6] });
                byte7 = new BitArray(new Byte[] { data[7] });
                byte8 = new BitArray(new Byte[] { data[8] });
                byte9 = new BitArray(new Byte[] { data[9] });
            }

            BitArray[] condBits = new BitArray[4];

            condBits[0] = new BitArray(new bool[] {
                            byte7.Get(4), // C1-0
                            byte8.Get(0), // C2-0
                            byte8.Get(4)  // C3-0
            });

            condBits[1] = new BitArray(new bool[]{
                            byte7.Get(5), // C1-1
                            byte8.Get(1), // C2-1
                            byte8.Get(5)  // C3-1
            });

            condBits[2] = new BitArray(new bool[]{
                            byte7.Get(6), // C1-2
                            byte8.Get(2), // C2-2
                            byte8.Get(6)  // C3-2
            });

            condBits[3] = new BitArray(new bool[] {
                            byte7.Get(7), // C1-3
                            byte8.Get(3), // C2-3
                            byte8.Get(7)  // C3-3
            });

            AccessConditions access = new AccessConditions();
            access.DataAreas[0].Initialize(condBits[0]);
            access.DataAreas[1].Initialize(condBits[1]);
            access.DataAreas[2].Initialize(condBits[2]);
            access.Trailer.Initialize(condBits[3]);

            access.MADVersion = AccessConditions.MADVersionEnum.NoMAD;
            if (byte9.Get(7))
            {
                if (byte9.Get(0))
                    access.MADVersion = AccessConditions.MADVersionEnum.Version1;
                if (byte9.Get(1))
                    access.MADVersion = AccessConditions.MADVersionEnum.Version2;
            }

            access.MultiApplicationCard = byte9.Get(6);

            //byte[] bx = CalculateAccessBits(access);
            return access;
        }

        #endregion

        /// <summary>
        /// Helper function for debug
        /// </summary>
        /// <param name="ba"></param>
        /// <param name="myWidth"></param>
        static void PrintValues(BitArray ba, int myWidth)
        {
            int i = myWidth;
            StringBuilder sb = new StringBuilder();

            for (int bit = ba.Length - 1; bit >= 0; bit--)
            {
                sb.Append(ba.Get(bit) ? "1" : "0");
            }

            System.Diagnostics.Debug.WriteLine(sb);
        }
    }
}
