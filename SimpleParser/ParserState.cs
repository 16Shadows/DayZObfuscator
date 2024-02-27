namespace DayZObfuscatorModel.Parser
{
	/// <summary>
	/// A base class for a parser's state used for error-handling.
	/// </summary>
	/// <typeparam name="LexerToken">The type of tokens produced by lexer and consumed by parser</typeparam>
	/// <typeparam name="ParserStates">All possible states of the <see cref="IParser{LexerToken, ParserResult, ParserError, ParserStates}"/></typeparam>
	public class ParserState<LexerToken, ParserStates> where LexerToken : LexerTokenBase where ParserStates : Enum
	{
		/// <summary>
		/// The state in which the parser was when the error occured
		/// </summary>
		public ParserStates CurrentState => StateStack.Last();

		/// <summary>
		/// A list of all states the parser entered excluding current state
		/// </summary>
		public IEnumerable<ParserStates> PreviousStates => StateStack.Take(StateStack.Count() - 1);

		/// <summary>
		/// A list of all states the parser has entered (including current state)
		/// </summary>
		public IEnumerable<ParserStates> StateStack { get; }

		/// <summary>
		/// All the tokens consumed in this parser's state
		/// </summary>
		public IEnumerable<LexerToken> ConsumedTokens { get; }

		/// <summary>
		/// The token which caused an error.
		/// </summary>
		public LexerToken CurrentToken { get; }

		/// <summary>
		/// Creates an instance of ParserState
		/// </summary>
		/// <param name="stateStack">The state the parser is in.</param>
		/// <param name="currentToken">The token the parser is currently examining</param>
		/// <param name="consumedTokens">A set of tokens already consumed by the parser's state. If the parser's state were to received these tokens, it would end up in this same state.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ParserState(IEnumerable<ParserStates> stateStack, LexerToken currentToken, IEnumerable<LexerToken> consumedTokens)
		{
			StateStack = stateStack;
			ConsumedTokens = consumedTokens ?? throw new ArgumentNullException(nameof(consumedTokens));
			CurrentToken = currentToken ?? throw new ArgumentNullException(nameof(currentToken));
		}

		public ParserState(IEnumerable<ParserStates> stateStack, LexerToken currentToken, params LexerToken[] consumedTokens) : this(stateStack, currentToken, (IEnumerable<LexerToken>)consumedTokens) {}

		/// <summary>
		/// Create a parser state with only a single token peeked
		/// </summary>
		/// <param name="stateStack">The state the parser is in.</param>
		/// <param name="currentToken">The token the parser is currently examining</param>
		public ParserState(IEnumerable<ParserStates> stateStack, LexerToken currentToken) : this(stateStack, currentToken, Enumerable.Empty<LexerToken>()) { }
	}
}
