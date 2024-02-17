using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.Parser
{
	/// <summary>
	/// An interface describing an algorithm used to resolve errors during parsing process.
	/// </summary>
	/// <typeparam name="LexerToken">The type of tokens consumed by the parser.</typeparam>
	/// <typeparam name="ParserResult">The result type produced by the parser.</typeparam>
	/// <typeparam name="ParserError">The type used to describe errors encountered by the parser.</typeparam>
	/// <typeparam name="ParserStates">A set of the parser's states.</typeparam>
	public interface IParserErrorResolver<LexerToken, ParserResult, ParserError, ParserStates> where LexerToken : LexerTokenBase where ParserStates : Enum
	{
		ILexer<LexerToken> Resolve(ILexer<LexerToken> lexer, IParser<LexerToken, ParserResult, ParserError, ParserStates> parser, ParserState<LexerToken, ParserStates> state, ParserError error);
	}
}
