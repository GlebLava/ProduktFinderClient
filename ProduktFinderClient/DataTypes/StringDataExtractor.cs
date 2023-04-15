using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ProduktFinderClient.DataTypes
{
    public class StringDataExtractor
    {
        /// <summary>
        /// Returns leftest float value in a string.
        /// In case there are no floats or string is empty/null a FormatExceptionError is thrown
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static float ExtractFloat(string s)
        {
            if (s == null || s == "")
                throw new FormatException("String cant be null or empty");
            var match = Regex.Match(s, @"([-+]?[0-9]*\.[0-9]+)");

            if (match.Success)
                return float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);

            throw new FormatException("String:  " + s + "   does not have a float in it");
        }

        public static string RightestKommaToDot(string s)
        {
            if (s == null)
                return s;

            char[] c = s.ToCharArray();
            for (int i = c.Length - 1; i >= 0; i--)
            {
                if (c[i] == ',')
                {
                    c[i] = '.';
                    break;
                }
            }
            return new string(c);
        }

        /// <summary>
        /// Returns leftest int value in a string.
        /// In case there are no floats or string is empty/null a FormatExceptionError is thrown
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int ExtractInt(string s)
        {
            if (s == null || s == "")
                throw new FormatException("String cant be null or empty");

            var match = Regex.Match(s, @"-?\d+");

            if (match.Success)
                return Int32.Parse(match.Groups[0].Value);

            throw new FormatException("String:  " + s + "   does not have a int in it");
        }


    }
}
