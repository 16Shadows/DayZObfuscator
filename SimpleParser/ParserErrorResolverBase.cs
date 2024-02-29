using DayZObfuscatorModel.Parser;

namespace SimpleParser
{
	public abstract class ParserErrorResolverBase<LexerToken, ParserResult, ParserError, ParserStates> : IParserErrorResolver<LexerToken, ParserResult, ParserError, ParserStates> where LexerToken : LexerTokenBase where ParserStates : Enum
	{
		protected class ErrorEncounteredException : Exception {}

		protected class ThrowerErrorResolver : IParserErrorResolver<LexerToken, ParserResult, ParserError, ParserStates>
		{
			public ILexer<LexerToken> Resolve(ILexer<LexerToken> lexer, IParser<LexerToken, ParserResult, ParserError, ParserStates> parser, ParserState<LexerToken, ParserStates> state, ParserError error)
			{
				throw new ErrorEncounteredException();
			}
		}
		protected static readonly ThrowerErrorResolver _RouteBackResolver = new ThrowerErrorResolver();

		protected delegate bool EndOfDocumentDetectorType(LexerToken token);

		private readonly EndOfDocumentDetectorType EndOfDocumentDetector;

		protected delegate ILexer<LexerToken> ErrorResolverType(ILexer<LexerToken> lexer, IParser<LexerToken, ParserResult, ParserError, ParserStates> parser, ParserState<LexerToken, ParserStates> state, ParserError error);
		private readonly Dictionary<ParserStates, ErrorResolverType> _Resolvers = new Dictionary<ParserStates, ErrorResolverType>();

		protected ParserErrorResolverBase(EndOfDocumentDetectorType endOfDocumentDetector)
		{
			EndOfDocumentDetector = endOfDocumentDetector ?? throw new ArgumentNullException(nameof(endOfDocumentDetector));
		}

		protected void AddErrorResolver(ParserStates state,  ErrorResolverType errorResolverType) => _Resolvers.Add(state, errorResolverType);

		public ILexer<LexerToken> Resolve(ILexer<LexerToken> lexer, IParser<LexerToken, ParserResult, ParserError, ParserStates> parser, ParserState<LexerToken, ParserStates> state, ParserError error)
		{
			if (_Resolvers.TryGetValue(state.CurrentState, out var resolver))
				return resolver(lexer, parser, state, error);
			else
				throw new ArgumentException($"State {state.CurrentState} is not supported by the resolver.");
		}

		protected ILexer<LexerToken> ResolveBySkipReplaceInject(ILexer<LexerToken> lexer, IParser<LexerToken, ParserResult, ParserError, ParserStates> parser, ParserState<LexerToken, ParserStates> state, LexerToken newToken)
		{
			//If we reached EndOfDocument, we can't drop a token or replace it, we have to prepend
			if (EndOfDocumentDetector(state.CurrentToken))
				return lexer.Prepend(newToken);

			int tokensConsumedOnRemoval, tokensConsumedOnReplace, tokensConsumedOnInsert;

			//Try parsing with the next token skipped
			PreviewLexer<LexerToken> previewLexer = BuildLexerWithSkippedToken(lexer, state);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnRemoval = previewLexer.ConsumedCount;
			
			//Tty parsing with next token replaced
			previewLexer = BuildLexerWithReplacedToken(lexer, state, newToken);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnReplace = previewLexer.ConsumedCount - 1; //Account for the 1 token we replaced (it is supposed to be consumed)

			//Try parsing with a token injected in from of the next one
			previewLexer = BuildLexerWithInjectedToken(lexer, state, newToken);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnInsert = previewLexer.ConsumedCount - 1; //Account for the 1 token we injected (it is supposed to be consumed)

			int maxTokens = Math.Max(Math.Max(tokensConsumedOnInsert, tokensConsumedOnReplace), tokensConsumedOnRemoval);

			//Choose the best option based on the number of tokens consumed during parsing
			if (tokensConsumedOnRemoval == maxTokens)
			{
				lexer.Consume();
				return lexer;
			}
			else if (tokensConsumedOnReplace == maxTokens)
			{
				lexer.Consume();
				return lexer.Prepend(newToken);
			}
			else 
				return lexer.Prepend(newToken);
		}

		protected ILexer<LexerToken> ResolveBySkipInjectReplace(ILexer<LexerToken> lexer, IParser<LexerToken, ParserResult, ParserError, ParserStates> parser, ParserState<LexerToken, ParserStates> state, LexerToken newToken)
		{
			//If we reached EndOfDocument, we can't drop a token or replace it, we have to prepend
			if (EndOfDocumentDetector(state.CurrentToken))
				return lexer.Prepend(newToken);

			int tokensConsumedOnRemoval, tokensConsumedOnReplace, tokensConsumedOnInsert;

			//Try parsing with the next token skipped
			PreviewLexer<LexerToken> previewLexer = BuildLexerWithSkippedToken(lexer, state);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnRemoval = previewLexer.ConsumedCount;
			
			//Tty parsing with next token replaced
			previewLexer = BuildLexerWithReplacedToken(lexer, state, newToken);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnReplace = previewLexer.ConsumedCount - 1; //Account for the 1 token we replaced (it is supposed to be consumed)

			//Try parsing with a token injected in from of the next one
			previewLexer = BuildLexerWithInjectedToken(lexer, state, newToken);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnInsert = previewLexer.ConsumedCount - 1; //Account for the 1 token we injected (it is supposed to be consumed)

			int maxTokens = Math.Max(Math.Max(tokensConsumedOnInsert, tokensConsumedOnReplace), tokensConsumedOnRemoval);

			//Choose the best option based on the number of tokens consumed during parsing
			if (tokensConsumedOnRemoval == maxTokens)
			{
				lexer.Consume();
				return lexer;
			}
			else if (tokensConsumedOnInsert == maxTokens)
				return lexer.Prepend(newToken);
			else
			{
				lexer.Consume();
				return lexer.Prepend(newToken);
			}
		}

		protected ILexer<LexerToken> ResolveByInjectReplaceSkip(ILexer<LexerToken> lexer, IParser<LexerToken, ParserResult, ParserError, ParserStates> parser, ParserState<LexerToken, ParserStates> state, LexerToken newToken)
		{
			//If we reached EndOfDocument, we can't drop a token or replace it, we have to prepend
			if (EndOfDocumentDetector(state.CurrentToken))
				return lexer.Prepend(newToken);

			int tokensConsumedOnRemoval, tokensConsumedOnReplace, tokensConsumedOnInsert;

			//Try parsing with the next token skipped
			PreviewLexer<LexerToken> previewLexer = BuildLexerWithSkippedToken(lexer, state);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnRemoval = previewLexer.ConsumedCount;
			
			//Tty parsing with next token replaced
			previewLexer = BuildLexerWithReplacedToken(lexer, state, newToken);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnReplace = previewLexer.ConsumedCount - 1; //Account for the 1 token we replaced (it is supposed to be consumed)

			//Try parsing with a token injected in from of the next one
			previewLexer = BuildLexerWithInjectedToken(lexer, state, newToken);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnInsert = previewLexer.ConsumedCount - 1; //Account for the 1 token we injected (it is supposed to be consumed)

			int maxTokens = Math.Max(Math.Max(tokensConsumedOnInsert, tokensConsumedOnReplace), tokensConsumedOnRemoval);

			//Choose the best option based on the number of tokens consumed during parsing
			if (tokensConsumedOnInsert == maxTokens)
			{
				return lexer.Prepend(newToken);
			}
			else if (tokensConsumedOnReplace == maxTokens)
			{
				lexer.Consume();
				return lexer.Prepend(newToken);
			}
			else
			{
				lexer.Consume();
				return lexer;
			}
		}

		protected ILexer<LexerToken> ResolveByInjectSkipReplace(ILexer<LexerToken> lexer, IParser<LexerToken, ParserResult, ParserError, ParserStates> parser, ParserState<LexerToken, ParserStates> state, LexerToken newToken)
		{
			//If we reached EndOfDocument, we can't drop a token or replace it, we have to prepend
			if (EndOfDocumentDetector(state.CurrentToken))
				return lexer.Prepend(newToken);

			int tokensConsumedOnRemoval, tokensConsumedOnReplace, tokensConsumedOnInsert;

			//Try parsing with the next token skipped
			PreviewLexer<LexerToken> previewLexer = BuildLexerWithSkippedToken(lexer, state);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnRemoval = previewLexer.ConsumedCount;
			
			//Tty parsing with next token replaced
			previewLexer = BuildLexerWithReplacedToken(lexer, state, newToken);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnReplace = previewLexer.ConsumedCount - 1; //Account for the 1 token we replaced (it is supposed to be consumed)

			//Try parsing with a token injected in from of the next one
			previewLexer = BuildLexerWithInjectedToken(lexer, state, newToken);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnInsert = previewLexer.ConsumedCount - 1; //Account for the 1 token we injected (it is supposed to be consumed)

			int maxTokens = Math.Max(Math.Max(tokensConsumedOnInsert, tokensConsumedOnReplace), tokensConsumedOnRemoval);

			//Choose the best option based on the number of tokens consumed during parsing
			if (tokensConsumedOnInsert == maxTokens)
			{
				return lexer.Prepend(newToken);
			}
			else if (tokensConsumedOnRemoval == maxTokens)
			{
				lexer.Consume();
				return lexer;
			}
			else
			{
				lexer.Consume();
				return lexer.Prepend(newToken);
			}
		}

		protected ILexer<LexerToken> ResolveByReplaceSkipInject(ILexer<LexerToken> lexer, IParser<LexerToken, ParserResult, ParserError, ParserStates> parser, ParserState<LexerToken, ParserStates> state, LexerToken newToken)
		{
			//If we reached EndOfDocument, we can't drop a token or replace it, we have to prepend
			if (EndOfDocumentDetector(state.CurrentToken))
				return lexer.Prepend(newToken);

			int tokensConsumedOnRemoval, tokensConsumedOnReplace, tokensConsumedOnInsert;

			//Try parsing with the next token skipped
			PreviewLexer<LexerToken> previewLexer = BuildLexerWithSkippedToken(lexer, state);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnRemoval = previewLexer.ConsumedCount;
			
			//Tty parsing with next token replaced
			previewLexer = BuildLexerWithReplacedToken(lexer, state, newToken);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnReplace = previewLexer.ConsumedCount - 1; //Account for the 1 token we replaced (it is supposed to be consumed)

			//Try parsing with a token injected in from of the next one
			previewLexer = BuildLexerWithInjectedToken(lexer, state, newToken);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnInsert = previewLexer.ConsumedCount - 1; //Account for the 1 token we injected (it is supposed to be consumed)

			int maxTokens = Math.Max(Math.Max(tokensConsumedOnInsert, tokensConsumedOnReplace), tokensConsumedOnRemoval);

			//Choose the best option based on the number of tokens consumed during parsing
			if (tokensConsumedOnReplace == maxTokens)
			{
				lexer.Consume();
				return lexer.Prepend(newToken);
			}
			else if (tokensConsumedOnRemoval == maxTokens)
			{
				lexer.Consume();
				return lexer;
			}
			else
			{
				return lexer.Prepend(newToken);
			}
		}

		protected ILexer<LexerToken> ResolveByReplaceInjectSkip(ILexer<LexerToken> lexer, IParser<LexerToken, ParserResult, ParserError, ParserStates> parser, ParserState<LexerToken, ParserStates> state, LexerToken newToken)
		{
			//If we reached EndOfDocument, we can't drop a token or replace it, we have to prepend
			if (EndOfDocumentDetector(state.CurrentToken))
				return lexer.Prepend(newToken);

			int tokensConsumedOnRemoval, tokensConsumedOnReplace, tokensConsumedOnInsert;

			//Try parsing with the next token skipped
			PreviewLexer<LexerToken> previewLexer = BuildLexerWithSkippedToken(lexer, state);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnRemoval = previewLexer.ConsumedCount;
			
			//Tty parsing with next token replaced
			previewLexer = BuildLexerWithReplacedToken(lexer, state, newToken);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnReplace = previewLexer.ConsumedCount - 1; //Account for the 1 token we replaced (it is supposed to be consumed)

			//Try parsing with a token injected in from of the next one
			previewLexer = BuildLexerWithInjectedToken(lexer, state, newToken);
			try
			{
				parser.TryParseFromState(previewLexer, state.CurrentState, state.PreviousStates, _RouteBackResolver);
			}
			catch (ErrorEncounteredException) {}
			tokensConsumedOnInsert = previewLexer.ConsumedCount - 1; //Account for the 1 token we injected (it is supposed to be consumed)

			int maxTokens = Math.Max(Math.Max(tokensConsumedOnInsert, tokensConsumedOnReplace), tokensConsumedOnRemoval);

			//Choose the best option based on the number of tokens consumed during parsing
			if (tokensConsumedOnReplace == maxTokens)
			{
				lexer.Consume();
				return lexer.Prepend(newToken);
			}
			else if (tokensConsumedOnInsert == maxTokens)
			{
				return lexer.Prepend(newToken);
			}
			else
			{
				lexer.Consume();
				return lexer;
			}
		}

		protected static PreviewLexer<LexerToken> BuildLexerWithSkippedToken(ILexer<LexerToken> lexer, ParserState<LexerToken, ParserStates> state)
		{
			IEnumerable<LexerToken> tokens = state.ConsumedTokens;
			var newLexer = lexer.AsPreview();

			newLexer.Consume();

			return new PreviewLexer<LexerToken>(newLexer.Prepend(tokens));
		}

		protected static PreviewLexer<LexerToken> BuildLexerWithReplacedToken(ILexer<LexerToken> lexer, ParserState<LexerToken, ParserStates> state, LexerToken replacementToken)
		{
			IEnumerable<LexerToken> tokens = state.ConsumedTokens;
			var newLexer = lexer.AsPreview();

			newLexer.Consume();

			return new PreviewLexer<LexerToken>(newLexer.Prepend(replacementToken).Prepend(tokens));
		}

		protected static PreviewLexer<LexerToken> BuildLexerWithInjectedToken(ILexer<LexerToken> lexer, ParserState<LexerToken, ParserStates> state, LexerToken injectedToken)
		{
			IEnumerable<LexerToken> tokens = state.ConsumedTokens;
			var newLexer = lexer.AsPreview();

			return new PreviewLexer<LexerToken>(newLexer.Prepend(injectedToken).Prepend(tokens));
		}
	}
}
