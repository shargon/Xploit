using System.Collections.Generic;

namespace XPloit.Core.BruteForce
{
    /// <summary>
    /// Letras disponibles de fuerza bruta
    /// </summary>
    public class BruteForceAllowedChars
    {
        public char[] Allowed = null;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="allow">Letras permitidas</param>
        public BruteForceAllowedChars(string allow)
        {
            Allowed = GetDifferent(allow);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="allow">Letras permitidas</param>
        /// <param name="mix">Separar en distintas letras que digan lo mismo</param>
        /// <param name="mixCase">Mezclar mayusculas y minusculas</param>
        public BruteForceAllowedChars(char allow, bool mix, bool mixCase)
        {
            if (mix)
            {
                Allowed = GetMix(allow, mixCase);
            }
            else
            {
                if (mixCase)
                    Allowed = new char[] { char.ToLowerInvariant(allow), char.ToUpperInvariant(allow) };
                else
                    Allowed = new char[] { allow };
            }
        }
        /// <summary>
        /// Obtiene las distintas letras para una letra
        /// </summary>
        /// <param name="allow">Letra</param>
        /// <param name="mixCase">Mezclar mayusculas y minusculas</param>
        public static char[] GetMix(char allow, bool mixCase)
        {
            List<char> ch = new List<char>();

            if (mixCase)
            {
                ch.Add(char.ToUpperInvariant(allow));

                char c = char.ToLowerInvariant(allow);
                if (!ch.Contains(c)) ch.Add(c);
            }
            else
            {
                ch.Add(allow);
            }

            switch (char.ToUpperInvariant(allow))
            {
                case 'A': ch.Add('4'); ch.Add('@'); break;
                case 'E': ch.Add('3'); break;
                case 'I': ch.Add('1'); break;
                case 'O': ch.Add('0'); break;
                //case '#': ch.Add('*'); break;

                //case 'R': ch.Add('D'); ch.Add('d'); break;  // Para Hacker y Hacked
            }

            return ch.ToArray();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="allow">Letras permitidas</param>
        public BruteForceAllowedChars(bool lowerAndUpper, bool mix, string cad)
        {
            if (lowerAndUpper) cad = cad.ToUpperInvariant() + cad.ToLowerInvariant();
            if (mix)
            {
                foreach (char c in cad.ToCharArray())
                {
                    foreach (char c2 in GetMix(c, false))
                        cad += c2;
                }
            }
            Allowed = GetDifferent(cad);
        }
        /// <summary>
        /// Obtiene las letras permitidas
        /// </summary>
        /// <param name="input">Cadena de letras disponibles</param>
        /// <returns>Devuelve los caracteres disponibles</returns>
        static char[] GetDifferent(string input)
        {
            List<char> ls = new List<char>();
            foreach (char c in input.ToCharArray())
                if (!ls.Contains(c)) ls.Add(c);
            return ls.ToArray();
        }
        /// <summary>
        /// Obtiene los AllowedChars desde varias cadenas de strings
        /// </summary>
        /// <param name="alloweds">Caracteres permitidos</param>
        public static BruteForceAllowedChars[] SplitAllowed(params string[] alloweds)
        {
            BruteForceAllowedChars[] ar = new BruteForceAllowedChars[alloweds.Length];
            for (int x = 0; x < ar.Length; x++)
                ar[x] = new BruteForceAllowedChars(alloweds[x]);
            return ar;
        }
        /// <summary>
        /// Separa una palabra en todos los posibles caracteres que puede tener cada caracter
        /// </summary>
        /// <param name="word">Palabra a separar</param>
        public static BruteForceAllowedChars[] SplitWordMixed(string word, bool lowerAndUpper, bool onlyFirstUpper, bool mixCase)
        {
            BruteForceAllowedChars[] ar = new BruteForceAllowedChars[word.Length];
            for (int x = 0; x < ar.Length; x++)
            {
                ar[x] = new BruteForceAllowedChars(word[x], lowerAndUpper || onlyFirstUpper && x == 0, mixCase);
            }
            return ar;
        }
        public override string ToString() { return new string(Allowed); }
    }
}