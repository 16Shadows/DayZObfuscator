namespace DayZObfuscatorModel.Parser
{
	/// <summary>
	/// An interface describing a parser.
	/// </summary>
	/// <typeparam name="LexerToken">The type of tokens consumed by the parser.</typeparam>
	/// <typeparam name="ParserResult">The result type produced by the parser.</typeparam>
	/// <typeparam name="ParserError">The type used to describe errors encountered by the parser.</typeparam>
	/// <typeparam name="ParserStates">A set of the parser's states.</typeparam>
	public interface IParser<LexerToken, ParserResult, ParserError, ParserStates> where LexerToken : LexerTokenBase where ParserStates : Enum
	{
		/// <summary>
		/// Perform full parse, producing <see cref="ParserResult"/>.
		/// </summary>
		/// <param name="lexer">The <see cref="ILexer{LexerToken}"/> which will provide tokens for the operation.</param>
		/// <returns><see cref="ParseResult{ParserResult, ParserError}"/> - an object describing parsing results.</returns>
		ParseResult<ParserResult, ParserError> Parse(ILexer<LexerToken> lexer);

		/// <summary>
		/// Perform partial parse from a particular parser state.
		/// This method exists to allow <see cref="IParserErrorResolver{LexerToken, ParserResult, ParserError, ParserStates}"/> to access partial parsing in order to resolve errors.
		/// </summary>
		/// <param name="lexer">The <see cref="ILexer{LexerToken}"/> which will provide tokens for the operation.</param>
		/// <param name="state">The state from which the parser should start.</param>
		/// <param name="stateStack">State stack of the parser to use (for context-sensitive parsing or to pass to <see cref="IParserErrorResolver{LexerToken, ParserResult, ParserError, ParserStates}"/>).</param>
		/// <returns>A set of errors encountered during the parse. If no errors where encountered, should be an empty enumerable.</returns>
		IEnumerable<ParserError> TryParseFromState(ILexer<LexerToken> lexer, ParserStates state, IEnumerable<ParserStates> stateStack);
	}
}
