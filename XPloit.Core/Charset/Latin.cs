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
    public class Latin : Pattern
    {

        /// <summary>
        /// Transform Method Latin Lowercase Alphabet
        /// </summary>
        /// <param name="charsetName"></param>
        /// <returns></returns>
        public static List<string> AlphabetLowercase(string charsetName)
        {
            if (charsetName == "lalpha")
            {
                // lalpha
                CharsetSelecting = AlphaBasicLatin.ToList();
                Validated = false;
            }

            else if (charsetName == "lalpha-space")
            {
                // lalpha_space
                CharsetSelecting = AlphaBasicLatin.Concat(Space).ToList();
                Validated = false;
            }
            else if (charsetName == "lalpha-numeric")
            {
                // lalpha-numeric
                CharsetSelecting = AlphaBasicLatin.Concat(Digits).ToList();
                Validated = false;
            }
            else if (charsetName == "lalpha-numeric-space")
            {
                // lalpha-numeric-space
                CharsetSelecting = AlphaBasicLatin.Concat(Digits).Concat(Space).ToList();
                Validated = false;
            }
            else if (charsetName == "lalpha-numeric-symbol14")
            {
                //lalpha-numeric-symbol14
                CharsetSelecting = AlphaBasicLatin.Concat(Digits).Concat(Symbols14).ToList();
                Validated = false;
            }
            else if (charsetName == "lalpha-numeric-symbol14-space")
            {
                // lalpha-numeric-symbol14-space
                CharsetSelecting = AlphaBasicLatin.Concat(Digits).Concat(Symbols14).Concat(Space).ToList();
                Validated = false;
            }
            else if (charsetName == "lalpha-numeric-all")
            {
                // lalpha-numeric-all
                CharsetSelecting = AlphaBasicLatin.Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).ToList();
                Validated = false;
            }
            else if (charsetName == "lalpha-numeric-all-space")
            {
                // lalpha-numeric-all-space
                CharsetSelecting = AlphaBasicLatin.Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).Concat(Space).ToList();
                Validated = false;
            }

            return CharsetSelecting;
        }

        /// <summary>
        /// Transform Method Latin Uppercase Alphabet
        /// </summary>
        /// <param name="charsetName"></param>
        /// <returns></returns>
        public static List<string> AlphabetUppercase(string charsetName)
        {
            if (charsetName == "ualpha")
            {
                //ualpha
                CharsetSelecting = AlphaBasicLatin.ConvertAll(item => item.ToUpper());
                Validated = false;
            }
            else if (charsetName == "ualpha-space")
            {
                //ualpha-space
                CharsetSelecting = AlphaBasicLatin.ConvertAll(item => item.ToUpper()).Concat(Space).ToList();
                Validated = false;
            }
            else if (charsetName == "ualpha-numeric")
            {
                //ualpha-numeric
                CharsetSelecting = AlphaBasicLatin.ConvertAll(item => item.ToUpper()).Concat(Digits).ToList();
                Validated = false;
            }
            else if (charsetName == "ualpha-numeric-space")
            {
                //ualpha_numeric_space
                CharsetSelecting = AlphaBasicLatin.ConvertAll(item => item.ToUpper()).Concat(Digits).Concat(Space).ToList();
                Validated = false;
            }
            else if (charsetName == "ualpha-numeric-symbol14")
            {
                // ualpha_numeric_symbol14
                CharsetSelecting = AlphaBasicLatin.ConvertAll(item => item.ToUpper()).Concat(Digits).Concat(Symbols14).ToList();
                Validated = false;
            }
            else if (charsetName == "ualpha-numeric-symbol14-space")
            {
                //ualpha_numeric-symbol14-space
                CharsetSelecting = AlphaBasicLatin.ConvertAll(item => item.ToUpper()).Concat(Digits).Concat(Symbols14).Concat(Space).ToList();
                Validated = false;
            }
            else if (charsetName == "ualpha-numeric-all")
            {
                //ualpha-numeric-all
                CharsetSelecting = AlphaBasicLatin.ConvertAll(item => item.ToUpper()).Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).ToList();
                Validated = false;
            }
            else if (charsetName == "ualpha-numeric-all-space")
            {
                //ualpha-numeric-all-space
                CharsetSelecting = AlphaBasicLatin.ConvertAll(item => item.ToUpper()).Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).Concat(Space).ToList();
                Validated = false;
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
        public static List<string> AlphabetMixCase(string charsetName)
        {

            if (charsetName == "mixalpha")
            {
                //mixalpha
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaBasicLatin.ConvertAll(item => item.ToUpper())).ToList();
                Validated = false;
            }
            else if (charsetName == "mixalpha-space")
            {
                //mixalpha-space
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaBasicLatin.ConvertAll(item => item.ToUpper())).Concat(Space).ToList();
                Validated = false;

            }
            else if (charsetName == "mixalpha-numeric")
            {
                //mixalpha-numeric
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaBasicLatin.ConvertAll(item => item.ToUpper())).Concat(Digits).ToList();
                Validated = false;

            }
            else if (charsetName == "mixalpha-numeric-space")
            {
                //mixalpha-numeric-space
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaBasicLatin.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Space).ToList();
                Validated = false;

            }
            else if (charsetName == "mixalpha-numeric-symbol14")
            {
                //mixalpha-numeric-symbol14
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaBasicLatin.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Symbols14).ToList();
                Validated = false;

            }
            else if (charsetName == "mixalpha-numeric-symbol14-space")
            {
                //mixalpha-numeric-symbol14-space
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaBasicLatin.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Symbols14).Concat(Space).ToList();
                Validated = false;
            }
            else if (charsetName == "mixalpha-numeric-all")
            {
                // mixalpha-numeric-all
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaBasicLatin.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).ToList();
                Validated = false;
            }
            else if (charsetName == "mixalpha-numeric-all-space")
            {
                // mixalpha-numeric-all-space
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaBasicLatin.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).Concat(Space).ToList();
                Validated = false;
            }
            else
            {
                Validated = true;
            }

            return CharsetSelecting;
        }

        /// <summary>
        /// Transform Method SV Latin  Lowercase  Alphabet
        /// </summary>
        /// <param name="charsetName"></param>
        /// <returns></returns>
        public static List<string> AlphabetSvLowercase(string charsetName)
        {

            if (charsetName == "sv-lalpha")
            {
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaSv).ToList();
                Validated = false;
            }
            else if (charsetName == "sv-lalpha-space")
            {
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaSv).Concat(Space).ToList();
                Validated = false;
            }
            else if (charsetName == "sv-lalpha-numeric")
            {
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaSv).Concat(Digits).ToList();
                Validated = false;

            }
            else if (charsetName == "sv-lalpha-numeric-space")
            {
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaSv).Concat(Digits).Concat(Space).ToList();
                Validated = false;

            }
            else if (charsetName == "sv-lalpha-numeric-symbol14")
            {
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaSv).Concat(Digits).Concat(Symbols14).ToList();
                Validated = false;

            }
            else if (charsetName == "sv-lalpha-numeric-symbol14-space")
            {
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaSv).Concat(Digits).Concat(Symbols14).Concat(Space).ToList();
                Validated = false;

            }
            else if (charsetName == "sv-lalpha-numeric-all")
            {
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaSv).Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).ToList();
                Validated = false;


            }
            else if (charsetName == "sv-lalpha-numeric-all-space")
            {
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaSv).Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).Concat(Space).ToList();
                Validated = false;

            }
            else
            {
                Validated = true;
            }

            return CharsetSelecting;
        }

        /// <summary>
        /// Transform Method SV Latin Uppercase Alphabet
        /// </summary>
        /// <param name="charsetName"></param>
        /// <returns></returns>
        public static List<string> AlphabetSvUppercase(string charsetName)
        {

            if (charsetName == "sv-ualpha")
            {
                //ualpha-sv
                CharsetSelecting = AlphaBasicLatin.ConvertAll(item => item.ToUpper()).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).ToList();
                Validated = false;
            }
            else if (charsetName == "sv-ualpha-space")
            {
                //ualpha-space-sv
                CharsetSelecting = AlphaBasicLatin.ConvertAll(item => item.ToUpper()).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).Concat(Space).ToList();
                Validated = false;

            }
            else if (charsetName == "sv-ualpha-numeric")
            {
                //ualpha-numeric-sv
                CharsetSelecting = AlphaBasicLatin.ConvertAll(item => item.ToUpper()).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).Concat(Digits).ToList();
                Validated = false;

            }
            else if (charsetName == "sv-ualpha-numeric-space")
            {
                //ualpha-numeric-space-sv
                CharsetSelecting = AlphaBasicLatin.ConvertAll(item => item.ToUpper()).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Space).ToList();
                Validated = false;

            }
            else if (charsetName == "sv-ualpha-numeric-symbol14")
            {
                //ualpha-numeric-symbol14-sv
                CharsetSelecting = AlphaBasicLatin.ConvertAll(item => item.ToUpper()).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Symbols14).ToList();
                Validated = false;

            }
            else if (charsetName == "sv-ualpha-numeric-symbol14-space")
            {
                //ualpha-numeric-symbol14-space-sv
                CharsetSelecting = AlphaBasicLatin.ConvertAll(item => item.ToUpper()).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Symbols14).Concat(Space).ToList();
                Validated = false;

            }
            else if (charsetName == "sv-ualpha-numeric-all")
            {
                //ualpha-numeric-all-sv
                CharsetSelecting = AlphaBasicLatin.ConvertAll(item => item.ToUpper()).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).ToList();
                Validated = false;

            }
            else if (charsetName == "sv-ualpha-numeric-all-space")
            {
                //ualpha-numeric-all-space-sv
                CharsetSelecting = AlphaBasicLatin.ConvertAll(item => item.ToUpper()).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).Concat(Space).ToList();
                Validated = false;

            }
            else
            {
                Validated = true;
            }

            return CharsetSelecting;
        }


        /// <summary>
        /// Transform Method SV Latin  Lowercase and Uppercase Alphabet
        /// </summary>
        /// <param name="charsetName"></param>
        /// <returns></returns>
        public static List<string> AlphabetSvMixcase(string charsetName)
        {

            if (charsetName == "sv-mixalpha")
            {
                //mixalpha-sv
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaSv).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).ToList();
                Validated = false;

            }
            else if (charsetName == "sv-mixalpha-space")
            {
                //mixalpha-space-sv
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaSv).Concat(AlphaBasicLatin.ConvertAll(item => item.ToUpper())).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).Concat(Space).ToList();
                Validated = false;

            }
            else if (charsetName == "sv-mixalpha-numeric")
            {
                // mixalpha-numeric-sv
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaSv).Concat(AlphaBasicLatin.ConvertAll(item => item.ToUpper())).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).Concat(Digits).ToList();
                Validated = false;
            }
            else if (charsetName == "sv-mixalpha-numeric-space")
            {
                //mixalpha-numeric-space_sv
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaSv).Concat(AlphaBasicLatin.ConvertAll(item => item.ToUpper())).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Space).ToList();
                Validated = false;

            }
            else if (charsetName == "sv-mixalpha-numeric-symbol14")
            {
                //mixalpha-numeric-symbol14-sv
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaSv).Concat(AlphaBasicLatin.ConvertAll(item => item.ToUpper())).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Symbols14).ToList();
                Validated = false;

            }
            else if (charsetName == "sv-mixalpha-numeric-symbol14-space")
            {
                //mixalpha-numeric-symbol14-space-sv
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaSv).Concat(AlphaBasicLatin.ConvertAll(item => item.ToUpper())).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Symbols14).Concat(Space).ToList();
                Validated = false;

            }
            else if (charsetName == "sv-mixalpha-numeric-all")
            {
                //mixalpha-numeric-all-sv
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaSv).Concat(AlphaBasicLatin.ConvertAll(item => item.ToUpper())).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).ToList();
                Validated = false;

            }
            else if (charsetName == "sv-mixalpha-numeric-all-space")
            {
                //mixalpha-numeric-all-space-sv
                CharsetSelecting = AlphaBasicLatin.Concat(AlphaSv).Concat(AlphaBasicLatin.ConvertAll(item => item.ToUpper())).Concat(AlphaSv.ConvertAll(item => item.ToUpper())).Concat(Digits).Concat(Symbols14).Concat(SymbolsAll).Concat(Space).ToList();
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
