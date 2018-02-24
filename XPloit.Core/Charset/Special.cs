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
    public class Special : Pattern
    {
        /// <summary>
        /// Transform Method hex-lower or hex-upper
        /// </summary>
        /// <param name="charsetName"></param>
        /// <returns></returns>
        public static List<string> Hex(string charsetName)
        {
            if (charsetName == "hex-lower")
            {
                //hex-lower
                CharsetSelecting = Digits.Concat(Hexa).ToList();
                Validated = false;
            }
            else if (charsetName == "hex-upper")
            {
                //hex-upper 
                CharsetSelecting = Digits.Concat(Hexa.ConvertAll(item => item.ToUpper())).ToList();
                Validated = false;
            }

            return CharsetSelecting;
        }


        /// <summary>
        /// Transform Method Digit  
        /// </summary>
        /// <param name="charsetName"></param>
        /// <returns></returns>
        public static List<string> Numeric(string charsetName)
        {
            if (charsetName == "numeric")
            {
                //numeric
                CharsetSelecting = Digits;
                Validated = false;
            }
            else if (charsetName == "numeric-space")
            {
                //numeric-space
                CharsetSelecting = Digits.Concat(Space).ToList();
                Validated = false;
            }

            return CharsetSelecting;
        }

        /// <summary>
        /// Transform Method Latin Symbols Alphabet
        /// </summary>
        /// <param name="charsetName"></param>
        /// <returns></returns>
        public static List<string> Symbols(string charsetName)
        {
            if (charsetName == "symbols14")
            {
                //symbols14  
                CharsetSelecting = Symbols14;
                Validated = false;
            }
            else if (charsetName == "symbols14-space")
            {
                //symbols14-space
                CharsetSelecting = Symbols14.Concat(Space).ToList();
                Validated = false;
            }
            else if (charsetName == "symbols-all")
            {
                //symbols-all
                CharsetSelecting = Symbols14.Concat(SymbolsAll).ToList();
                Validated = false;
            }
            else if (charsetName == "symbols-all-space")
            {
                //symbols-all-space
                CharsetSelecting = Symbols14.Concat(SymbolsAll).Concat(Space).ToList();
                Validated = false;
            }

            return CharsetSelecting;
        }
    }
}

