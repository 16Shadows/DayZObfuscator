using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public ParserStates CurrentState { get; }

		/// <summary>
		/// All the tokens consumed in this parser's state
		/// </summary>
		public IEnumerable<LexerToken> ConsumedTokens { get; }

		/// <summary>
		/// The token which caused an error.
		/// </summary>
		public LexerToken CurrentToken { get; }

		public bool CurrentTokenConsumed { get; }

		/// <summary>
		/// Creates an instance of ParserState
		/// </summary>
		/// <param name="currentState">The state the parser is in.</param>
		/// <param name="currentToken">The token the parser is currently examining</param>
		/// <param name="currentTokenConsumed">If true, <paramref name="currentToken"/> has been consumed while being examined and is included in <paramref name="consumedTokens"/>. If false, the token has only been peeked.</param>
		/// <param name="consumedTokens">A set of tokens already consumed by the parser's state. If the parser's state were to received these tokens, it would end up in this same state.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ParserState(ParserStates currentState, LexerToken currentToken, bool currentTokenConsumed, IEnumerable<LexerToken> consumedTokens)
		{
			CurrentState = currentState;
			ConsumedTokens = consumedTokens ?? throw new ArgumentNullException(nameof(consumedTokens));
			CurrentToken = currentToken ?? throw new ArgumentNullException(nameof(currentToken));
			CurrentTokenConsumed = currentTokenConsumed;
		}

		public ParserState(ParserStates currentState, LexerToken currentToken, bool currentTokenConsumed, params LexerToken[] consumedTokens) : this(currentState, currentToken, currentTokenConsumed, (IEnumerable<LexerToken>)consumedTokens) {}

		/// <summary>
		/// Create a parser state with only a single token peeked
		/// </summary>
		/// <param name="currentState">The state the parser is in.</param>
		/// <param name="currentToken">The token the parser is currently examining</param>
		public ParserState(ParserStates currentState, LexerToken currentToken) : this(currentState, currentToken, false, Enumerable.Empty<LexerToken>()) { }
	}
}
