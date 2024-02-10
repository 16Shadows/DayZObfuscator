using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace DayZObfuscatorModel
{
	public static class StringExtensions
	{
        static StringExtensions()
        {
            _EscapeSequences = new Dictionary<string, string>()
            {
                {@"""", @"\"""},
                {@"\", @"\\"},
                {"\a", @"\a"},
                {"\b", @"\b"},
                {"\f", @"\f"},
                {"\n", @"\n"},
                {"\r", @"\r"},
                {"\t", @"\t"},
                {"\v", @"\v"},
                {"\0", @"\0"},
            };

            _EscapeMatcher = new Regex(string.Join('|', _EscapeSequences.Keys.Select(Regex.Escape)));

            _UnescapeSequences = _EscapeSequences.ToDictionary(x => x.Value, x => x.Key);
            _UnescapeMatcher = new Regex(string.Join('|', _UnescapeSequences.Keys.Select(Regex.Escape)));
        }

		private static Dictionary<string, string> _EscapeSequences;
        private static Dictionary<string, string> _UnescapeSequences;
        private static Regex _EscapeMatcher;
        private static Regex _UnescapeMatcher;

        /// <summary>
        /// Escapes this string using default escape table
        /// </summary>
        /// <param name="str">This string</param>
        /// <returns>Escaped string</returns>
		public static string Escape(this string str)
		{
            return _EscapeMatcher.Replace(str, (match) => _EscapeSequences[match.Value] );
		}

        /// <summary>
        /// Unescape this string using default escape table
        /// </summary>
        /// <param name="str">This string</param>
        /// <returns>Unescaped string</returns>
        public static string Unescape(this string str)
        {
            return _UnescapeMatcher.Replace(str, (match) => _UnescapeSequences[match.Value] );
        }

        /// <summary>
        /// Escape this string using custom escape table
        /// </summary>
        /// <param name="str">This string</param>
        /// <param name="escapeTable">Escape table</param>
        /// <returns>Escaped string</returns>
        public static string Escape(this string str, Dictionary<string, string> escapeTable)
		{
            Regex matcher = new Regex(string.Join("|", escapeTable.Keys.Select(Regex.Escape)));
            return matcher.Replace(str, (match) => escapeTable[match.Value] );
		}

        /// <summary>
        /// Unescapes this string using default escape table (this table is the same which is used in <see cref="Escape(string, Dictionary{string, string})"/>, it will be automatically reversed).
        /// </summary>
        /// <param name="str">This string</param>
        /// <param name="escapeTable">Escape table (same as in <see cref="Escape(string, Dictionary{string, string})"/>, it will be automatically reversed).</param>
        /// <returns>Unescaped string</returns>
        public static string Unescape(this string str, Dictionary<string, string> escapeTable) =>
            Escape(str, escapeTable.ToDictionary(x => x.Value, x => x.Key));
	}
}
