using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace XPloit.Core.Helpers
{
    public class HashHelper
    {
        public enum EHashType { Md5, Sha1, Sha256, Sha384, Sha512 };

        byte[] _Raw;
        string _Hex;

        public HashHelper(EHashType type, string cad) : this(type, Encoding.UTF8, cad) { }
        public HashHelper(EHashType type, Encoding codec, string cad)
        {
            if (codec != null && !string.IsNullOrEmpty(cad))
            {
                _Raw = HashRaw(type, codec.GetBytes(cad));
                _Hex = HexHelper.Buffer2Hex(_Raw);
            }
        }
        public HashHelper(EHashType type, byte[] bx)
        {
            _Raw = HashRaw(type, bx);
            _Hex = HexHelper.Buffer2Hex(_Raw);
        }

        public byte[] Raw { get { return _Raw; } }
        public string Hex { get { return _Hex; } }

        public static string HashHex(EHashType type, string cad) { return HashHex(type, Encoding.UTF8, cad); }
        public static string HashHex(EHashType type, Encoding codec, string cad)
        {
            if (string.IsNullOrEmpty(cad)) return null;

            byte[] raw = HashRaw(type, codec.GetBytes(cad));
            return HexHelper.Buffer2Hex(raw);
        }
        public static string HashHex(EHashType type, byte[] bs) { return HexHelper.Buffer2Hex(HashRaw(type, bs)); }
        public static byte[] HashRaw(EHashType type, byte[] bs)
        {
            if (bs == null) return null;

            HashAlgorithm cmd5 = null;
            switch (type)
            {
                case EHashType.Md5: cmd5 = new MD5CryptoServiceProvider(); break;
                case EHashType.Sha1: cmd5 = new SHA1CryptoServiceProvider(); break;
                case EHashType.Sha256: cmd5 = new SHA256CryptoServiceProvider(); break;
                case EHashType.Sha384: cmd5 = new SHA384CryptoServiceProvider(); break;
                case EHashType.Sha512: cmd5 = new SHA512CryptoServiceProvider(); break;
            }

            bs = cmd5.ComputeHash(bs);
            cmd5.Dispose();
            return bs;
        }
        public static string HashHex(EHashType type, Stream bs, bool seekBegin) { return HexHelper.Buffer2Hex(HashRaw(type, bs, seekBegin)); }
        public static byte[] HashRaw(EHashType type, Stream bs, bool seekBegin)
        {
            if (bs == null) return null;

            HashAlgorithm cmd5 = null;
            switch (type)
            {
                case EHashType.Md5: cmd5 = new MD5CryptoServiceProvider(); break;
                case EHashType.Sha1: cmd5 = new SHA1CryptoServiceProvider(); break;
                case EHashType.Sha256: cmd5 = new SHA256CryptoServiceProvider(); break;
                case EHashType.Sha384: cmd5 = new SHA384CryptoServiceProvider(); break;
                case EHashType.Sha512: cmd5 = new SHA512CryptoServiceProvider(); break;
            }

            if (seekBegin) bs.Seek(0, SeekOrigin.Begin);

            byte[] bsh = cmd5.ComputeHash(bs);
            cmd5.Dispose();
            return bsh;
        }

        public static string HashHexFile(EHashType type, string file)
        {
            try
            {
                using (FileStream fs = File.OpenRead(file))
                    return HashHex(type, fs, false);
            }
            catch { }
            return null;
        }

        public static byte[] HashRawFile(EHashType type, string file)
        {
            try
            {
                using (FileStream fs = File.OpenRead(file))
                    return HashRaw(type, fs, false);
            }
            catch { }
            return null;
        }
    }
}