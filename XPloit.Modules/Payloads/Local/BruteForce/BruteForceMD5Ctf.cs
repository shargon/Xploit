using Auxiliary.Local;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Cryptography;
using XPloit.Helpers.Attributes;

namespace Payloads.Local.BruteForce
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "BruteForce MD5 for Cybercamp 2017 CTF")]
    public class BruteForceMD5Ctf : Payload, WordListBruteForce.ICheckPassword
    {
        public enum EHashType : uint
        {
            MD5 = 0,
            SHA1 = 1,
            SHA256 = 2,
            SHA384 = 3,
            SHA512 = 4,
            MD4 = 5
        }

        [ConfigurableProperty(Description = "Hash 50afXXXXXX6351475e54bf6eb2c96f2b")]
        public string Hash { get; set; }
        [ConfigurableProperty(Description = "Hash Type (MD5)")]
        public EHashType HashType { get; set; }

        [ConfigurableProperty(Description = "Hash Iterations (1)")]
        public int HashIterations { get; set; } = 1;

        int _CheckArrayLength;
        byte[] _CheckArray;
        byte[] _MustBe;

        public bool CheckPassword(string password)
        {
            byte[] data = Encoding.ASCII.GetBytes(password);

            for (int x = 0, hi = HashIterations; x < hi; x++)
                using (HashAlgorithm m = Get())
                    data = m.ComputeHash(data);

            for (int x = 0; x < _CheckArrayLength; x++)
            {
                if (data[_CheckArray[x]] != _MustBe[x])
                    return false;
            }

            return true;
        }

        public HashAlgorithm Get()
        {
            switch (HashType)
            {
                case EHashType.MD5: return MD5.Create();
                case EHashType.SHA1: return new SHA1Managed();
                case EHashType.SHA256: return new SHA256Managed();
                case EHashType.SHA384: return new SHA384Managed();
                case EHashType.SHA512: return new SHA512Managed();
                case EHashType.MD4: return new MD4();
            }

            throw (new NotImplementedException());
        }

        public bool PreRun()
        {
            if (Hash.Length % 2 != 0)
            {
                WriteError("Hash must be in hex");
                return false;
            }

            List<byte> checkArray = new List<byte>();
            List<byte> mustBe = new List<byte>();

            for (byte x = 0, y = 0; x < Hash.Length; x += 2, y++)
            {
                string sp = Hash.Substring(x, 2);

                byte val;
                if (byte.TryParse(sp, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out val))
                {
                    checkArray.Add(y);
                    mustBe.Add(val);
                }
            }

            _CheckArray = checkArray.ToArray();
            _MustBe = mustBe.ToArray();
            _CheckArrayLength = _CheckArray.Length;
            return true;
        }

        public void PostRun()
        {
        }
    }
}