using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace XPloit.Core.Helpers.Crypt
{
    public class AESHelper
    {
        RijndaelManaged symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC };
        ICryptoTransform encryptor, decryptor;

        public interface IAESConfig
        {
            string AesIV { get; }
            string AesPassword { get; }
            int AesIterations { get; }
            AESHelper.EKeyLength AesKeyLength { get; }
            string AesRGBSalt { get; }
        }

        public enum EKeyLength
        {
            Length_128 = 128,
            Length_192 = 192,
            Length_256 = 256
        }
        public AESHelper(IAESConfig config) : this(config.AesPassword, config.AesRGBSalt, config.AesIterations, config.AesIV, config.AesKeyLength) { }
        /// <summary>
        /// Clase de encriptación
        /// </summary>
        /// <param name="password">Texto que se usará para generar el algoritmo de cifrado</param>
        /// <param name="valorRGBSalt">una cadena de texto cualquiera</param>
        /// <param name="iteraciones">número de iteraciones</param>
        /// <param name="vectorInicial">Un texto o número de 16 bytes (16 caracteres)</param>
        /// <param name="keyLength">Tamaño clave: puede ser 128, 192 o 256</param>
        public AESHelper(string password, string valorRGBSalt, int iteraciones, string vectorInicial, EKeyLength keyLength)
        {
            if (vectorInicial == null) vectorInicial = "";
            if (vectorInicial.Length != 16)
            {
                if (vectorInicial.Length < 16) vectorInicial = vectorInicial.PadLeft(16, '0');
                else vectorInicial = vectorInicial.Substring(0, 16);
            }

            byte[] InitialVectorBytes = Encoding.ASCII.GetBytes(vectorInicial);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(valorRGBSalt);

            PasswordDeriveBytes pd = new PasswordDeriveBytes(password, saltValueBytes, "SHA1"  // Algoritmo de cifrado: puede ser MD5 ó SHA1
                , iteraciones);
            byte[] keyBytes = pd.GetBytes((int)keyLength / 8);

            encryptor = symmetricKey.CreateEncryptor(keyBytes, InitialVectorBytes);
            decryptor = symmetricKey.CreateDecryptor(keyBytes, InitialVectorBytes);
        }
        ///// <summary>
        ///// Clase de encriptación compatible con PHP
        ///// </summary>
        ///// <param name="keyString">Clave</param>
        ///// <param name="vectorInicial">Un texto o número de 16 bytes (16 caracteres)</param>
        //public AESHelper(string keyString, string vectorInicial, EKeyLength keyLength)
        //{
        //    //if (keyString.Length != (int)keyLength / 8)
        //    //{
        //    //    int t = (int)keyLength / 8;
        //    //    if (keyString.Length < t) keyString = keyString.PadLeft(t, '0');
        //    //    else keyString = keyString.Substring(0, t);
        //    //}

        //    //if (vectorInicial.Length != 16)
        //    //{
        //    //    if (vectorInicial.Length < 16) vectorInicial = vectorInicial.PadLeft(16, '0');
        //    //    else vectorInicial = vectorInicial.Substring(0, 16);
        //    //}

        //    //Encoding encoding = Encoding.ASCII;
        //    Encoding encoding = new UTF8Encoding();
        //    byte[] Key = encoding.GetBytes(keyString);
        //    byte[] IV = encoding.GetBytes(vectorInicial);

        //    symmetricKey.Padding = PaddingMode.PKCS7;
        //    symmetricKey.Mode = CipherMode.CBC;
        //    symmetricKey.KeySize = (int)keyLength;
        //    symmetricKey.BlockSize = (int)keyLength;
        //    symmetricKey.Key = Key;
        //    symmetricKey.IV = IV;
        //    symmetricKey.Padding = PaddingMode.PKCS7;

        //    encryptor = symmetricKey.CreateEncryptor(Key, IV);
        //    decryptor = symmetricKey.CreateDecryptor(Key, IV);
        //}

        public byte[] Encrypt(byte[] data) { return Encrypt(data, 0, data.Length, null); }
        public byte[] Encrypt(byte[] data, byte[] header) { return Encrypt(data, 0, data.Length, header); }
        public byte[] Encrypt(byte[] data, int index, int length, byte[] header)
        {
            byte[] cipherTextBytes = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    if (header != null)
                        ms.Write(header, 0, header.Length);

                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, index, length);
                        cs.FlushFinalBlock();
                    }

                    cipherTextBytes = ms.ToArray();
                }
            }
            catch { }
            return cipherTextBytes;
        }
        public byte[] Encrypt(Stream data, bool seekBegin) { return Encrypt(data, seekBegin, null); }
        public byte[] Encrypt(Stream data, bool seekBegin, byte[] header)
        {
            byte[] cipherTextBytes = null;
            try
            {
                if (seekBegin) data.Seek(0, SeekOrigin.Begin);

                byte[] readb = new byte[1024 * 10];

                using (MemoryStream ms = new MemoryStream())
                {
                    if (header != null) ms.Write(header, 0, header.Length);

                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        int lee = 0;
                        while ((lee = data.Read(readb, 0, readb.Length)) > 0)
                            cs.Write(readb, 0, lee);
                        cs.FlushFinalBlock();
                    }

                    cipherTextBytes = ms.ToArray();
                }
            }
            catch { }
            return cipherTextBytes;
        }

        public byte[] Decrypt(byte[] data)
        {
            if (data == null) return null;
            return Decrypt(data, 0, data.Length);
        }
        /// <summary>
        /// Desencripta el array de bytes
        /// </summary>
        /// <param name="data">Array a desencriptar</param>
        /// <param name="index">Posición inicial</param>
        /// <param name="length">Longitud del array</param>
        /// <returns>Devuelve el array desencriptado</returns>
        public byte[] Decrypt(byte[] data, int index, int length)
        {
            byte[] result = null;
            try
            {
                using (MemoryStream ms = new MemoryStream(data, index, length))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    result = new byte[length];
                    int result_length = cs.Read(result, 0, length);
                    Array.Resize(ref result, result_length);
                }
            }
            catch { result = null; }
            return result;
        }
        /// <summary>
        /// Desencripta el array de bytes
        /// </summary>
        /// <param name="data">Array a desencriptar</param>
        /// <param name="index">Posición inicial</param>
        /// <param name="length">Longitud del array, que se modifica por la longitud desencriptada</param>
        /// <returns>Devuelve el array desencriptado</returns>
        public byte[] Decrypt(byte[] data, int index, ref int length)
        {
            byte[] result = null;
            try
            {
                using (MemoryStream ms = new MemoryStream(data, index, length))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    result = new byte[length];
                    length = cs.Read(result, 0, length);
                }
            }
            catch { result = null; }
            return result;
        }
        public bool Decrypt(byte[] data, ref byte[] result, out int result_length)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(data))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    result = new byte[data.Length];
                    result_length = cs.Read(result, 0, result.Length);
                    return true;
                }
            }
            catch { }
            result_length = -1;
            return false;
        }
        /// <summary>
        /// Check if its OK
        /// </summary>
        /// <param name="cfg">Config</param>
        public static bool IsConfigured(IAESConfig cfg)
        {
            if (cfg == null) return false;
            return !string.IsNullOrEmpty(cfg.AesPassword) && !string.IsNullOrEmpty(cfg.AesIV) && !string.IsNullOrEmpty(cfg.AesRGBSalt);
        }
    }
}