using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XPloit.Core;
using XPloit.Core.Attributes;

namespace XPloit.Modules.Auxiliary.Local
{
    public class AuxiliaryWordListGenerator : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Generate a wordList"; } }
        public override DateTime DisclosureDate { get { return DateTime.MinValue; } }
        public override bool IsLocal { get { return true; } }
        public override bool IsRemote { get { return false; } }
        public override string Path { get { return "Auxiliary/Local"; } }
        public override string Name { get { return "WordListGenerator"; } }
        #endregion

        #region Properties
        [ConfigurableProperty(Required = true, Description = "File where is write the word list")]
        public string File { get; set; }
        #endregion

        public override bool Run()
        {
            DictinaryGenCracker caction = null;
            try { caction = new DictinaryGenCracker(File); }
            catch
            {
                if (caction != null) caction.Dispose();
                return false;
            }
            finally { }

            // TODO: Read the properties

            int gen = 0;

            string[] asep1 = new string[] { "", "#*" };
            string[] asep2 = new string[] { "word1", "word2", "word3", "word4", "word5", "word6", "word7" };
            string[] asep3 = new string[] { "", "#*" };
            string[] asep4 = new string[] { "word1", "word2", "word3", "word4", "word5", "word6", "word7" };
            string[] asep5 = new string[] { "", "#*" };

            foreach (string sep1 in asep1)
            {
                BruteForceAllowedChars a1 = null;
                if (!string.IsNullOrEmpty(sep1)) a1 = new BruteForceAllowedChars(sep1);

                foreach (string word1 in asep2)
                {
                    BruteForceAllowedChars[] a2 = null;
                    if (!string.IsNullOrEmpty(word1)) a2 = BruteForceAllowedChars.SplitWordMixed(word1, BruteForceAllowedChars.EMixCase.OnlyFirst);

                    foreach (string sep2 in asep3)
                    {
                        BruteForceAllowedChars a3 = null;
                        if (!string.IsNullOrEmpty(sep2)) a3 = new BruteForceAllowedChars(sep2);

                        foreach (string word2 in asep4)
                        {
                            BruteForceAllowedChars[] a4 = null;
                            if (!string.IsNullOrEmpty(word2)) a4 = BruteForceAllowedChars.SplitWordMixed(word2, BruteForceAllowedChars.EMixCase.OnlyFirst);

                            foreach (string sep3 in asep5)
                            {
                                BruteForceAllowedChars a5 = null;
                                if (!string.IsNullOrEmpty(sep3)) a5 = new BruteForceAllowedChars(sep3);

                                List<BruteForceAllowedChars> ch = new List<BruteForceAllowedChars>();

                                if (a1 != null) ch.Add(a1);
                                if (a2 != null) ch.AddRange(a2);
                                if (a3 != null) ch.Add(a3);
                                if (a4 != null) ch.AddRange(a4);
                                if (a5 != null) ch.Add(a5);

                                int itry2 = BruteForce.Run(ch.ToArray(), "", "", 1, caction);
                                gen += itry2;
                            }
                        }
                    }
                }
            }

            WriteInfo("Generated file successful ", gen.ToString(), ConsoleColor.Green);
            caction.Dispose();
            return true;
        }


        class DictinaryGenCracker : IBruteForceAction, IDisposable
        {
            FileStream _Item = null;
            /// <summary>
            /// Acción de desbloqueo de bitlocker
            /// </summary>
            /// <param name="file">Donde se generará el diccionario</param>
            public DictinaryGenCracker(string file)
            {
                _Item = System.IO.File.OpenWrite(file);
            }
            public override bool CheckPassword(string password)
            {
                byte[] data = Encoding.UTF8.GetBytes(password + "\n");
                _Item.Write(data, 0, data.Length);
                return false;
            }
            /// <summary>
            /// Libera los recursos
            /// </summary>
            public void Dispose() { if (_Item != null) _Item.Dispose(); }
        }
        class BruteForce
        {
            /// <summary>
            /// Lanza el proceso de fuerza bruta
            /// </summary>
            /// <param name="length">Tamaño</param>
            /// <param name="prefix">Prefijo de la contraseña</param>
            /// <param name="suffix">Sufijo de la contraseña</param>
            /// <param name="threads">Hijos a lanzar</param>
            /// <param name="action">Acción a realizar</param>
            public static int Run(BruteForceAllowedChars[] length, string prefix, string suffix, int threads, IBruteForceAction action)
            {
                CancellationTokenSource cancel = new CancellationTokenSource();
                action.CancellationTokenTask = cancel;

                bool multiple = action.AllowMultiplePasswords;

                // Calculo total
                int lleva = 0, total = 1;
                foreach (BruteForceAllowedChars c in length) { total *= c.Allowed.Length; }

                // Contraseña
                IEnumerable<string> passwords = GetAllMatches(length, prefix, suffix);

                int founds = 0;
                // Tarea
                try
                {
                    ParallelLoopResult res =
                        Parallel.ForEach<string>(passwords,
                        new ParallelOptions()
                        {
                            MaxDegreeOfParallelism = threads,
                            CancellationToken = cancel.Token
                        },
                        password =>
                        {
                            // Progreso aumentado
                            Interlocked.Increment(ref lleva);
                            // Comprobar contraseña
                            if (action.CheckPassword(password))
                            {
                                if (multiple || Interlocked.Increment(ref founds) == 1)
                                {
                                    SystemSounds.Question.Play();
                                    action.Accept(password);
                                    if (!multiple) action.Cancel(true);
                                }
                            }
                            // Comprobar si se ha cancelado
                            action.CheckIfNeedCancel();
                        });
                }
                catch (Exception e)
                {
                }
                return lleva;
            }
            #region BruteForce Core
            static IEnumerable<string> GetAllMatches(BruteForceAllowedChars[] chars, string prefix, string suffix)
            {
                int length = chars.Length;

                int[] indexes = new int[length];
                char[] current = new char[length];
                for (int i = 0; i < length; i++)
                    current[i] = chars[i].Allowed[0];

                do
                {
                    yield return prefix + new string(current) + suffix;
                }
                while (Increment(indexes, current, chars));
            }

            static bool Increment(int[] indexes, char[] current, BruteForceAllowedChars[] chars)
            {
                int position = indexes.Length - 1;

                while (position >= 0)
                {
                    indexes[position]++;
                    BruteForceAllowedChars ch = chars[position];
                    if (indexes[position] < ch.Allowed.Length)
                    {
                        current[position] = ch.Allowed[indexes[position]];
                        return true;
                    }
                    indexes[position] = 0;
                    current[position] = ch.Allowed[0];
                    position--;
                }
                return false;
            }
            #endregion
        }
        /// <summary>
        /// Interfaz de acción bruta
        /// </summary>
        class IBruteForceAction
        {
            List<string> _Candidates = new List<string>();
            CancellationTokenSource _CancellationToken;

            public event delOnPassword OnPassword = null;
            public delegate void delOnPassword(object sender, string password);

            /// <summary>
            /// True para permitir varias contraseñas
            /// </summary>
            public virtual bool AllowMultiplePasswords { get { return false; } }
            /// <summary>
            /// Token para cancelar tareas
            /// </summary>
            internal CancellationTokenSource CancellationTokenTask { get { return _CancellationToken; } set { _CancellationToken = value; } }
            /// <summary>
            /// Claves candidatas
            /// </summary>
            public string[] PasswordCandidates { get { return _Candidates.ToArray(); } }
            /// <summary>
            /// Obtiene la acción a realizar
            /// </summary>
            /// <param name="password">Contraseña a probar</param>
            /// <returns>Devuelve true si la contraseña es correcta</returns>
            public virtual bool CheckPassword(string password) { return true; }
            /// <summary>
            /// Cancela la acción
            /// </summary>
            /// <param name="throwException">True para lanzar una escepción y finalizar la búsqueda</param>
            public void Cancel(bool throwException)
            {
                if (_CancellationToken != null)
                {
                    _CancellationToken.Cancel();
                    if (throwException) _CancellationToken.Token.ThrowIfCancellationRequested();
                }
            }
            /// <summary>
            /// Acepta la acción
            /// </summary>
            /// <param name="password">Contraseña</param>
            public void Accept(string password)
            {
                lock (_Candidates)
                {
                    if (!_Candidates.Contains(password))
                        _Candidates.Add(password);
                }

                if (OnPassword != null) OnPassword(this, password);
            }
            /// <summary>
            /// Comprueba si tiene que tirar una excepción para cancelarlo
            /// </summary>
            public void CheckIfNeedCancel()
            {
                if (_CancellationToken != null) _CancellationToken.Token.ThrowIfCancellationRequested();
            }
        }
        /// <summary>
        /// Letras disponibles de fuerza bruta
        /// </summary>
        class BruteForceAllowedChars
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
                else Allowed = new char[] { allow };
            }
            /// <summary>
            /// Obtiene las distintas letras para una letra
            /// </summary>
            /// <param name="allow">Letra</param>
            /// <param name="mixCase">Mezclar mayusculas y minusculas</param>
            char[] GetMix(char allow, bool mixCase)
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
                    case 'A': ch.Add('4'); break;
                    case 'E': ch.Add('3'); break;
                    case 'I': ch.Add('1'); break;
                    case 'O': ch.Add('0'); break;
                    case '#': ch.Add('*'); break;

                    //case 'R': ch.Add('D'); ch.Add('d'); break;  // Para Hacker y Hacked
                }

                return ch.ToArray();
            }
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="allow">Letras permitidas</param>
            public BruteForceAllowedChars(params char[] allow)
            {
                Allowed = GetDifferent(new string(allow));
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
            public enum EMixCase { None, All, OnlyFirst }
            /// <summary>
            /// Separa una palabra en todos los posibles caracteres que puede tener cada caracter
            /// </summary>
            /// <param name="word">Palabra a separar</param>
            public static BruteForceAllowedChars[] SplitWordMixed(string word, EMixCase mixCase)
            {
                BruteForceAllowedChars[] ar = new BruteForceAllowedChars[word.Length];
                for (int x = 0; x < ar.Length; x++)
                {
                    ar[x] = new BruteForceAllowedChars(word[x], true, mixCase == EMixCase.All || (mixCase == EMixCase.OnlyFirst && x == 0));
                }
                return ar;
            }
            public override string ToString() { return new string(Allowed); }
        }
    }
}