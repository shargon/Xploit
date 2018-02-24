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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPloit.Core.Charset
{
    public class Mix : Pattern
    {
        /// <summary>
        /// Mix two Transform Method
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static List<string> MixMergeTwo(List<string> list1, List<string> list2)
        {
            Validated = false;
            return CharsetSelecting = list1.Concat(list2).ToList();
        }


        /// <summary>
        /// Mix three Transform Method
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <param name="list3"></param>
        /// <returns></returns>
        public static List<string> MixMergeThree(List<string> list1, List<string> list2, List<string> list3)
        {
            Validated = false;
            return CharsetSelecting = list1.Concat(list2).Concat(list3).ToList();
        }


        /// <summary>
        /// Mix four Transform Method
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <param name="list3"></param>
        /// <param name="list4"></param>
        /// <returns></returns>
        public static   List<string> MixMergeFour(List<string> list1, List<string> list2, List<string> list3, List<string> list4)
        {
            Validated = false;
            return CharsetSelecting = list1.Concat(list2).Concat(list3).Concat(list4).ToList();
        }
    }
}
