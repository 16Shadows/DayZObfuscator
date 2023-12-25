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

		public static string Escape(this string str)
		{
            return _EscapeMatcher.Replace(str, (match) => _EscapeSequences[match.Value] );
		}

        public static string Unescape(this string str)
        {
            return _UnescapeMatcher.Replace(str, (match) => _UnescapeSequences[match.Value] );
        }
	}
}
