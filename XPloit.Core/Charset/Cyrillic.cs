//  Author:
//       Teeknofil <teeknofil.dev@gmail.com>
//
//  Copyright (c) 2015 Teeknofil
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;

namespace XPloit.Core.Charset
{
    public class Cyrillic : Pattern
    {
        /// <summary>
        /// Transform Method Cyrillic Lowercase Alphabet
        /// </summary>
        /// <param name="charsetName"></param>
        /// <returns></returns>
        public static List<string> AlphabetLowercase(string charsetName)
        {
            if (charsetName == "lcyrillic")
            {
                //lcyrillic
                CharsetSelecting = Cyrillic.ToList();

            }
            else if (charsetName == "lcyrillic-space")
            {
                // lcyrillic-space
                CharsetSelecting = Cyrillic.Concat(Space).ToList();
                Validated = false;
            }
            else if (charsetName == "lcyrillic-numeric")
            {
                // lcyrillic-numeric
                CharsetSelecting = Cyrillic.Concat(Digits).ToList();
                Validated = false;
            }
            else if (charsetName == "lcyrillic-numeric-space")
            {
                //lcyrillic - numeric - space
                CharsetSelecting = Cyrillic.Concat(Digits).Concat(Space).ToList();
                Validated = false;
            }
            else if (charsetName == "lcyrillic-numeric-symbol14")
            {
                //lcyrillic-numeric-symbol14
                CharsetSelecting = Cyrillic.Concat(Digits).Concat(Symbols14).ToList();
                Validated = false;
            }
            else if (charsetName == "lcyrillic-numeric-symbol14-space")
            {
                // lcyrillic-numeric-symbol14-space
                CharsetSelecting = Cyrillic.Concat(Digits).Concat(Symbols14).Concat(Space).ToList();
                Validated = false;
            }
            else if (charsetName == "lcyrillic-numeric-all")
            {
                //lcyrillic-numeric-all
                CharsetSelecting = Cyrillic.Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).ToList();
                Validated = false;
            }
            else if (charsetName == "lcyrillic-numeric-all-space")
            {
                //lcyrillic-numeric-all-space
                CharsetSelecting = Cyrillic.Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).Concat(Space).ToList();
                Validated = false;
            }
            else
            {
                Validated = true;
            }

            return CharsetSelecting;
        }

        /// <summary>
        /// Transform Method Cyrillic Uppercase Alphabet
        /// </summary>
        /// <param name="charsetName"></param>
        /// <returns></returns>
        public static List<string> AlphabetUppercase(string charsetName)
        {
            if (charsetName == "ucyrillic")
            {
                //ucyrillique
                CharsetSelecting = Cyrillic.ConvertAll(item => item.ToUpper()).ToList();
                Validated = false;
            }
            else if (charsetName == "ucyrillic-space")
            {
                //uacyrillique-space
                CharsetSelecting = Cyrillic.ConvertAll(item => item.ToUpper()).Concat(Space).ToList();
                Validated = false;
            }
            else if (charsetName == "ucyrillic-numeric")
            {
                //ucyrillique-numeric
                CharsetSelecting = Cyrillic.ConvertAll(item => item.ToUpper()).Concat(Digits).ToList();
                Validated = false;
            }
            else if (charsetName == "ucyrillic-numeric-space")
            {
                //ucyrillique_numeric_space
                CharsetSelecting = Cyrillic.ConvertAll(item => item.ToUpper()).Concat(Digits).Concat(Space).ToList();
                Validated = false;
            }
            else if (charsetName == "ucyrillic-numeric-symbol14")
            {
                //ucyrillic-numeric-symbol14
                CharsetSelecting = Cyrillic.ConvertAll(item => item.ToUpper()).Concat(Digits).Concat(Symbols14).ToList();
                Validated = false;
            }
            else if (charsetName == "ucyrillic-numeric-symbol14-space")
            {
                //ucyrillic-numeric-symbol14-space
                CharsetSelecting = Cyrillic.ConvertAll(item => item.ToUpper()).Concat(Digits).Concat(Symbols14).Concat(Space).ToList();
                Validated = false;
            }
            else if (charsetName == "ucyrillic-numeric-all")
            {
                //ucyrillique-numeric-all
                CharsetSelecting = Cyrillic.ConvertAll(item => item.ToUpper()).Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).ToList();
                Validated = false;
            }
            else if (charsetName == "ucyrillic-numeric-all-space")
            {
                //ucyrillique-numeric-all-space
                CharsetSelecting = Cyrillic.ConvertAll(item => item.ToUpper()).Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).Concat(Space).ToList();
            }
            else
            {
                Validated = true;
            }

            return CharsetSelecting;
        }

        /// <summary>
        /// Transform Method Latin Lowercase and Uppercase Alphabet
        /// </summary>
        /// <param name="charsetName"></param>
        /// <returns></returns>
        public static List<string> AlphabettMix(string charsetName)
        {

            if (charsetName == "mixcyrillic")
            {
                //mixcyrillic
                CharsetSelecting = Cyrillic.Concat(Cyrillic.ConvertAll(item => item.ToUpper())).ToList();
                Validated = false;

            }
            else if (charsetName == "mixcyrillic-space")
            {
                //mixcyrillic-space
                CharsetSelecting = Cyrillic.Concat(Cyrillic.ConvertAll(item => item.ToUpper())).Concat(Space).ToList();
                Validated = false;

            }
            else if (charsetName == "mixcyrillic-numeric")
            {
                //mixcyrillic-numeric
                CharsetSelecting = Cyrillic.Concat(Cyrillic.ConvertAll(item => item.ToUpper())).Concat(Digits).ToList();
                Validated = false;

            }
            else if (charsetName == "mixcyrillic-numeric-space")
            {
                //mixcyrillic-numeric-space
                CharsetSelecting = Cyrillic.Concat(Cyrillic.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Space).ToList();
                Validated = false;

            }
            else if (charsetName == "mixcyrillic-numeric-symbol14")
            {
                //mixcyrillic-numeric-symbol14
                CharsetSelecting = Cyrillic.Concat(Cyrillic.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Symbols14).ToList();
                Validated = false;

            }
            else if (charsetName == "mixcyrillic-numeric-symbol14-space")
            {
                //mixcyrillic-numeric-symbol14-space
                CharsetSelecting = Cyrillic.Concat(Cyrillic.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Symbols14).Concat(Space).ToList();
                Validated = false;

            }
            else if (charsetName == "mixcyrillic-numeric-all")
            {
                //mixcyrillic-numeric-all
                CharsetSelecting = Cyrillic.Concat(Cyrillic.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).ToList();
                Validated = false;
            }
            else if (charsetName == "mixcyrillic-numeric-all-space")
            {
                //mixcyrillic-numeric-all-space
                CharsetSelecting = Cyrillic.Concat(Cyrillic.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).Concat(Space).ToList();
                Validated = false;
            }
            else
            {
                Validated = true;
            }

            return CharsetSelecting;
        }
    }
}
