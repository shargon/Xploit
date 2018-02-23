using XPloit.Core;
using XPloit.Core.Attributes;

namespace Encoders.Xor
{
    [ModuleInfo(Author = "Teeknofil", Description = "Encode byte[] to Xor bytes")]
    public class XorEncoder : XPloit.Core.Encoder
    {
        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="strKey"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
       //TODO: fix Target String
        public static byte[] Run(byte[] bytes, string strKey, Payload payload)
        {
            if (payload == null) return null;

            int amount = 350;
            byte[] key = System.Text.Encoding.ASCII.GetBytes(strKey);
            for (int i = 0; i < bytes.Length; i++) bytes[i] ^= (byte)(key[i % key.Length] >> (i + amount + key.Length) & 255);
            return bytes;
        }

    }
}
