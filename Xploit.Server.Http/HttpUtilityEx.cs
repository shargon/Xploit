using System.Security.Cryptography;
using System.Text;

namespace XPloit.Server.Http
{
    public class HttpUtilityEx
    {
        public static string Md5(string cad)
        {
            if (string.IsNullOrEmpty(cad)) return "";
            return Md5(Encoding.UTF8.GetBytes(cad));
        }
        public static string Md5(byte[] bs)
        {
            if (bs == null) return "";

            StringBuilder s = new StringBuilder();
            using (MD5CryptoServiceProvider cmd5 = new MD5CryptoServiceProvider())
                bs = cmd5.ComputeHash(bs);
            foreach (byte b in bs) s.Append(b.ToString("x2").ToLower());
            return s.ToString();
        }
        public static void SeparaEnDos(string palabra, char sep, out string izq, out string drc)
        {
            int fi = string.IsNullOrEmpty(palabra) ? -1 : palabra.IndexOf(sep);
            if (fi == -1) { izq = palabra; drc = ""; return; }

            izq = palabra.Substring(0, fi);
            drc = palabra.Substring(fi + 1, palabra.Length - fi - 1);
        }
        public static void SeparaEnDos(string palabra, string sep, out string izq, out string drc)
        {
            int fi = string.IsNullOrEmpty(palabra) ? -1 : palabra.IndexOf(sep);
            if (fi == -1) { izq = palabra; drc = ""; return; }

            izq = palabra.Substring(0, fi);
            int sl = sep.Length;
            drc = palabra.Substring(fi + sl, palabra.Length - fi - sl);
        }
    }
}