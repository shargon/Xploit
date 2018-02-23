using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XPloit.Core.Helpers
{
    public class ArgumentHelper
    {
        #region
        // Parse command line string to array
        // Solution for Daniel Earwicker
        // http://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp/298990#298990
        static string TrimMatchingQuotes(string input, char quote)
        {
            input = input.Trim();

            if ((input.Length >= 2) &&
                (input[0] == quote) && (input[input.Length - 1] == quote))
                return input.Substring(1, input.Length - 2);

            return input;
        }
        static IEnumerable<string> Split(string str, Func<char, bool> controller)
        {
            int nextPiece = 0;

            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }
        /// <summary>
        /// Parse CommandLine to Array
        /// </summary>
        /// <param name="commandLine">Command Line</param>
        /// <returns>Return array for conversion</returns>
        public static string[] ArrayFromCommandLine(string commandLine)
        {
            bool inQuotes = false;

            return (Split(commandLine, c =>
            {
                if (c == '\"')
                    inQuotes = !inQuotes;

                return !inQuotes && c == ' ';
            })
                          .Select(arg => TrimMatchingQuotes(arg, '\"'))
                          .Where(arg => !string.IsNullOrEmpty(arg))).ToArray();
        }
        #endregion

        /// <summary>
        /// Parse object from arguments
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="commandLine">Arguments</param>
        /// <returns>Return object T</returns>
        public static object Parse(Type type, string commandLine)
        {
            return Parse(type, ArrayFromCommandLine(commandLine));
        }
        /// <summary>
        /// Parse object from arguments
        /// </summary>
        /// <param name="commandLine">Arguments</param>
        /// <returns>Return object T</returns>
        public static T Parse<T>(string commandLine)
        {
            return Parse<T>(ArrayFromCommandLine(commandLine));
        }
        /// <summary>
        /// Parse object from arguments
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="arguments">Arguments</param>
        /// <returns>Return object T</returns>
        public static object Parse(Type type, string[] arguments)
        {
            if (arguments != null)
            {
                object obj = Activator.CreateInstance(type);
                FillWithArguments(obj, arguments);
                return obj;
            }

            return null;
        }
        /// <summary>
        /// Parse object from arguments
        /// </summary>
        /// <param name="arguments">Arguments</param>
        /// <returns>Return object T</returns>
        public static T Parse<T>(string[] arguments)
        {
            if (arguments != null)
            {
                T obj = Activator.CreateInstance<T>();
                FillWithArguments(obj, arguments);
                return obj;
            }

            return default(T);
        }
        /// <summary>
        /// Load object from arguments
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="arguments">Arguments</param>
        static void FillWithArguments(object obj, string[] arguments)
        {
            Type tp = obj.GetType();

            foreach (string s in arguments)
            {
                string l, r;
                if (!StringHelper.Split(s, "=", out l, out r)) continue;

                PropertyInfo pi = tp.GetProperty(l);
                if (pi == null) continue;

                pi.SetValue(obj, ConvertHelper.ConvertTo(r, pi.PropertyType));
            }
        }
    }
}