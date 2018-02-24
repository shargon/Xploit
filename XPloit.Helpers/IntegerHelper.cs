using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XPloit.Helpers
{
    public class IntegerHelper
    {
        /// <summary>
        /// Ramdom numbers with custom seed
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>

        public static int RandomInterger(int max)
        {
            Thread.Sleep(1);
            Random rdm = new Random(DateTime.Now.Millisecond - DateTime.Now.Second - DateTime.Now.Minute * max * max++);
            return rdm.Next(max);
        }

        /// <summary>
        /// Ramdom numbers with min and max value
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int RandomInterger(int min, int max)
        {
            int i = 0;
            Thread.Sleep(1);
            Random rdm = new Random(DateTime.Now.Millisecond - DateTime.Now.Second - DateTime.Now.Minute * max * max++);

            if (max > min)
            {
                i = rdm.Next(min, max);
            }
            else
            {
                i = rdm.Next(max, min);
            }

            return i;
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
            Random rnd = new Random(DateTime.Now.Millisecond);
            chArray2 = chArray2.OrderBy(x => rnd.Next()).ToArray();
            byte[] data = new byte[1];

            using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                cryptoServiceProvider.GetNonZeroBytes(data);
                data = new byte[seedNumber];
                cryptoServiceProvider.GetNonZeroBytes(data);
            }

            StringBuilder stringBuilder = new StringBuilder(seedNumber);
            foreach (byte num in data)
                stringBuilder.Append(chArray2[(int)num % chArray2.Length]);

            return BigInteger.Parse(stringBuilder.ToString());
        }

    }
}
