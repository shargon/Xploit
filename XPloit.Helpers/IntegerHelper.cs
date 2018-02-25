using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace XPloit.Helpers
{
    public class IntegerHelper
    {
        static Random Rand = new Random();
        /// <summary>
        /// Ramdom numbers with custom seed
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int RandomInterger(int max)
        {
            return Rand.Next(max);
        }

        /// <summary>
        /// Ramdom numbers with min and max value
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int RandomInterger(int min, int max)
        {
            if (max > min)
            {
                return Rand.Next(min, max);
            }
            else
            {
                return Rand.Next(max, min);
            }
        }

        /// <summary>
        /// Ramdom big numbers with custom value
        /// </summary>
        /// <param name="max"></param>      
        /// <returns></returns>
        public static BigInteger RandomBigInteger(int seedNumber)
        {
            if (seedNumber > 9999)
            {
                seedNumber = 9999;
            }

            char[] chArray1 = new char[62];
            char[] chArray2 = "0123456789".ToCharArray();
            chArray2 = chArray2.OrderBy(x => Rand.Next()).ToArray();
            byte[] data = new byte[1];

            using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                cryptoServiceProvider.GetNonZeroBytes(data);
                data = new byte[seedNumber];
                cryptoServiceProvider.GetNonZeroBytes(data);
            }

            StringBuilder stringBuilder = new StringBuilder(seedNumber);
            foreach (byte num in data)
                stringBuilder.Append(chArray2[num % chArray2.Length]);

            return BigInteger.Parse(stringBuilder.ToString());
        }
    }
}