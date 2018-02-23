using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XPloit.Helpers;

namespace XPloit.Core.BruteForce
{
    public class BruteForce
    {
        int _Sets = 0;
        List<BruteForceStep[]> _Steps;

        /// <summary>
        /// Sets
        /// </summary>
        public int Sets { get { return _Sets; } }
        /// <summary>
        /// Steps
        /// </summary>
        public int Steps { get { return _Steps == null ? 0 : _Steps.Count; } }

        bool ParseSection(string input, Dictionary<string, string[]> set, List<BruteForceStep[]> checks)
        {
            if (string.IsNullOrEmpty(input)) return true;

            string iz, dr;
            StringHelper.Split(input, '=', out iz, out dr);

            string[] toca = null;
            if (!set.TryGetValue(iz, out toca))
            {
                if (toca == null)
                {
                    // Default values
                    switch (iz)
                    {
                        case "0-F": toca = new string[] { "0123456789ABCDEF" }; break;
                        case "0-f": toca = new string[] { "0123456789abcdef" }; break;
                        case "0-9": toca = new string[] { "0123456789" }; break;
                        case "¡!": toca = new string[] { " !\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~" }; break;
                        case "¡m!": toca = new string[] { "!#$%&()*+,-./:;<=>?@_|~" }; break;
                        case "a-z": toca = new string[] { "abcdefghijklmnopqrstuvwxyz" }; break;
                        case "A-Z": toca = new string[] { "ABCDEFGHIJKLMNOPQRSTUVWXYZ" }; break;
                        case "a-Z": toca = new string[] { "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" }; break;
                        default: toca = new string[] { iz }; dr = "w"; break;
                    }
                }

                if (toca == null)
                {
                    throw (new Exception("Cant find section '" + iz + "'"));
                }
            }
            //checks.Add(new check[] { new check(""), new check("#*") });
            //checks.Add(new check[] { new check("word1", BruteForceAllowedChars.EMixCase.OnlyFirst), new check("word2", BruteForceAllowedChars.EMixCase.OnlyFirst), new check("word3", BruteForceAllowedChars.EMixCase.OnlyFirst), new check("word4", BruteForceAllowedChars.EMixCase.OnlyFirst), new check("word5", BruteForceAllowedChars.EMixCase.OnlyFirst) });
            //checks.Add(new check[] { new check(""), new check("#*") });
            //checks.Add(new check[] { new check("word1", BruteForceAllowedChars.EMixCase.OnlyFirst), new check("word2", BruteForceAllowedChars.EMixCase.OnlyFirst), new check("word3", BruteForceAllowedChars.EMixCase.OnlyFirst), new check("word4", BruteForceAllowedChars.EMixCase.OnlyFirst), new check("word5", BruteForceAllowedChars.EMixCase.OnlyFirst) });
            //checks.Add(new check[] { new check(""), new check("#*") });

            bool allowVacia = false;
            if (dr.StartsWith("!"))
            {
                dr = dr.TrimStart('!');
                allowVacia = true;
            }

            dr = dr.TrimEnd('s', 'S'); // words / chars

            if (dr == "" || dr == "c" || dr == "C" || dr.ToUpperInvariant() == "CHAR" || dr.ToUpperInvariant() == "CH4R")
            {
                bool mix = dr.Contains("4");

                bool lowerAndUpper = dr == "C" || dr == "CHAR" || dr == "CH4R";
                if (allowVacia) checks.Add(new BruteForceStep[] { new BruteForceStep(), BruteForceStep.FromChar(toca, lowerAndUpper, mix) });
                else checks.Add(new BruteForceStep[] { BruteForceStep.FromChar(toca, lowerAndUpper, mix) });
            }
            else
            {
                if (dr == "w" || dr == "W" || dr.ToUpperInvariant() == "WORD" || dr.ToUpperInvariant() == "W0RD")
                {
                    bool upperOnlyFirst = false;
                    bool mix = dr.Contains("0");
                    if (dr == "W0rd" || dr == "Word") upperOnlyFirst = true;

                    bool lowerAndUpper = dr == "W" || dr == "WORD" || dr == "W0RD";
                    checks.Add(BruteForceStep.FromWord(allowVacia, toca, lowerAndUpper, upperOnlyFirst, mix));
                }
            }

            return true;
        }
        /// <summary>
        /// Compile the current Picture
        /// </summary>
        /// <param name="file">File</param>
        /// <param name="picture">Picture</param>
        /// <param name="sets">Sets loaded</param>
        public bool CompilePicture(string file, string picture)
        {
            _Sets = 0;
            Dictionary<string, string[]> set = new Dictionary<string, string[]>();
            List<BruteForceStep[]> checks = new List<BruteForceStep[]>();
            if (!string.IsNullOrEmpty(file))
            {
                if (File.Exists(file))
                {
                    IniHelper ini = new IniHelper(file, false, false, false);
                    foreach (string section in ini.Sections)
                    {
                        List<string> list = new List<string>();
                        foreach (string line in ini.GetVariables(section))
                            list.Add(line);

                        set.Add(section, list.ToArray());
                    }
                }
                else throw (new Exception("File '" + file + "' not found"));
                _Sets = set.Count;
            }

            if (string.IsNullOrEmpty(picture))
            {
                throw (new Exception("Error in ConfigPicture"));
            }

            StringBuilder sb = new StringBuilder();
            int index = -1;
            bool isOpen = false;
            foreach (char c in picture.ToCharArray())
            {
                index++;
                switch (c)
                {
                    case '[':
                        {
                            if (!ParseSection(sb.ToString(), set, checks))
                                return false;

                            sb.Clear();
                            if (isOpen)
                            {
                                throw (new Exception("Error in picture position " + index.ToString() + ", [ already open"));
                            }
                            isOpen = true;
                            break;
                        }
                    case ']':
                        {
                            if (!isOpen)
                            {
                                throw (new Exception("Error in picture position " + index.ToString() + ", ] already closed"));
                            }
                            isOpen = false;

                            if (!ParseSection(sb.ToString(), set, checks))
                                return false;

                            sb.Clear();
                            break;
                        }
                    default:
                        {
                            sb.Append(c);
                            break;
                        }
                }
            }

            if (!ParseSection(sb.ToString(), set, checks))
                return false;
            sb.Clear();

            if (checks.Count <= 0) return false;

            _Steps = checks;
            return true;
        }
        /// <summary>
        /// Run BruteForce
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> Run()
        {
            List<BruteForceAllowedChars[]> chList = new List<BruteForceAllowedChars[]>();
            foreach (BruteForceStep[] c in _Steps) chList.Add(null);

            return Run(_Steps.ToArray(), chList.ToArray(), 0, _Steps.Count - 1);
        }
        IEnumerable<string> Run(BruteForceStep[][] steps, BruteForceAllowedChars[][] chList, int index, int count)
        {
            BruteForceStep[] current = steps[index];
            bool isLast = index == count;

            foreach (BruteForceStep c in current)
            {
                // Set current
                chList[index] = c.Get();

                if (isLast)
                {
                    // Recall all
                    List<BruteForceAllowedChars> ch = new List<BruteForceAllowedChars>();

                    foreach (BruteForceAllowedChars[] a1 in chList)
                        if (a1 != null) ch.AddRange(a1);

                    foreach (string pwd in GetAllMatches(ch.ToArray(), "", ""))
                        yield return pwd;
                }
                else
                {
                    foreach (string pwd in Run(steps, chList, index + 1, count))
                        yield return pwd;
                }
            }
        }
        /// <summary>
        /// Count
        /// </summary>
        public ulong Count()
        {
            List<BruteForceAllowedChars[]> chList = new List<BruteForceAllowedChars[]>();
            foreach (BruteForceStep[] c in _Steps) chList.Add(null);

            return Count(_Steps.ToArray(), chList.ToArray(), 0, _Steps.Count - 1);
        }
        ulong Count(BruteForceStep[][] checks, BruteForceAllowedChars[][] chList, int index, int count)
        {
            BruteForceStep[] current = checks[index];
            bool isLast = index == count;

            ulong hay = 0;
            foreach (BruteForceStep c in current)
            {
                // Set current
                chList[index] = c.Get();

                if (isLast)
                {
                    // Recall all
                    List<BruteForceAllowedChars> ch = new List<BruteForceAllowedChars>();

                    foreach (BruteForceAllowedChars[] a1 in chList)
                        if (a1 != null) ch.AddRange(a1);

                    hay += (ulong)Count(ch.ToArray());
                }
                else
                {
                    hay += Count(checks, chList, index + 1, count);
                }
            }
            return hay;
        }
        long Count(BruteForceAllowedChars[] length)
        {
            long total = 1;
            foreach (BruteForceAllowedChars c in length) { total *= c.Allowed.Length; }
            return total;
        }

        #region BruteForce Core
        IEnumerable<string> GetAllMatches(BruteForceAllowedChars[] chars, string prefix, string suffix)
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

        bool Increment(int[] indexes, char[] current, BruteForceAllowedChars[] chars)
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
}