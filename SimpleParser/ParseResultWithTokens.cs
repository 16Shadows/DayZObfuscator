using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.Parser
{
	public class ParseResultWithTokens<ParserResult, ParserError, LexerToken> : ParseResult<ParserResult, ParserError> where LexerToken : LexerTokenBase
	{
		public ParseResultWithTokens(ParserResult result, bool success, IEnumerable<ParserError> errors, IEnumerable<LexerToken> consumedTokens) : base(result, success, errors)
		{
			ConsumedTokens = consumedTokens;
		}

		public ParseResultWithTokens(ParserResult result, bool success, IEnumerable<ParserError> errors, params LexerToken[] consumedTokens) : this(result, success, errors, (IEnumerable<LexerToken>)consumedTokens) { }

		public IEnumerable<LexerToken> ConsumedTokens { get; }

		override public ParseResultWithTokens<T, ParserError, LexerToken> WithResultAs<T>(Func<ParserResult, T> converter)
		{
			ArgumentNullException.ThrowIfNull(converter);
			return new ParseResultWithTokens<T, ParserError, LexerToken>(converter(Result), Success, Errors, ConsumedTokens);
		}
	}
}
