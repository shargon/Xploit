using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XPloit.Core;
using XPloit.Core.Attributes;

namespace Auxiliary.Local
{
    public class WordListGenerator : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Generate a wordList"; } }
        public override DateTime DisclosureDate { get { return DateTime.MinValue; } }
        public override bool IsLocal { get { return true; } }
        #endregion

        #region Properties
        [ConfigurableProperty(Required = true, Description = "File where is write the word list")]
        public FileInfo File { get; set; }
        #endregion

        class check
        {
            public string Input;
            public BruteForceAllowedChars.EMixCase MixCase;

            public check(string input) { Input = input; }
            public check(string input, BruteForceAllowedChars.EMixCase mix) { Input = input; MixCase = mix; }

            public BruteForceAllowedChars[] Get()
            {
                if (MixCase == BruteForceAllowedChars.EMixCase.None) return new BruteForceAllowedChars[] { new BruteForceAllowedChars(Input) };
                return BruteForceAllowedChars.SplitWordMixed(Input, MixCase);
            }
        }

        public override bool Run()
        {
            DictinaryGenCracker caction = null;
            try { caction = new DictinaryGenCracker(File.FullName); }
            catch
            {
                if (caction != null) caction.Dispose();
                WriteError("Cant open file '" + File + "'");
                return false;
            }
            finally { }

            // TODO: Read the properties

            List<check[]> checks = new List<check[]>();
            checks.Add(new check[] { new check(""), new check("#*") });
            checks.Add(new check[] { new check("word1", BruteForceAllowedChars.EMixCase.OnlyFirst), new check("word2", BruteForceAllowedChars.EMixCase.OnlyFirst), new check("word3", BruteForceAllowedChars.EMixCase.OnlyFirst), new check("word4", BruteForceAllowedChars.EMixCase.OnlyFirst), new check("word5", BruteForceAllowedChars.EMixCase.OnlyFirst) });
            checks.Add(new check[] { new check(""), new check("#*") });
            checks.Add(new check[] { new check("word1", BruteForceAllowedChars.EMixCase.OnlyFirst), new check("word2", BruteForceAllowedChars.EMixCase.OnlyFirst), new check("word3", BruteForceAllowedChars.EMixCase.OnlyFirst), new check("word4", BruteForceAllowedChars.EMixCase.OnlyFirst), new check("word5", BruteForceAllowedChars.EMixCase.OnlyFirst) });
            checks.Add(new check[] { new check(""), new check("#*") });

            // Copy one slot peer array
            List<BruteForceAllowedChars[]> chList = new List<BruteForceAllowedChars[]>();
            foreach (check[] c in checks) chList.Add(null);

            int gen = DoRecursive(checks, chList, 0, checks.Count - 1, 0, caction);

            WriteInfo("Generated file successful ", gen.ToString(), ConsoleColor.Green);
            caction.Dispose();
            return true;
        }

        int DoRecursive(List<check[]> checks, List<BruteForceAllowedChars[]> chList, int index, int count, int ret, DictinaryGenCracker caction)
        {
            check[] current = checks[index];
            bool isLast = index == count;

            foreach (check c in current)
            {
                // Set current
                if (!string.IsNullOrEmpty(c.Input)) chList[index] = c.Get();
                else chList[index] = null;

                if (isLast)
                {
                    // Recall all
                    List<BruteForceAllowedChars> ch = new List<BruteForceAllowedChars>();

                    foreach (BruteForceAllowedChars[] a1 in chList)
                        if (a1 != null) ch.AddRange(a1);

                    ret += BruteForce.Run(ch.ToArray(), "", "", 1, caction);
                }
                else
                {
                    ret += DoRecursive(checks, chList, index + 1, count, 0, caction);
                }
            }

            return ret;
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