using DayZObfuscatorModel.Parser;
using DayZObfuscatorModel.PBO.Config.Parser.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.PBO.Config.Parser
{
	public class ConfigParserErrorResolver : IParserErrorResolver<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors>, ConfigParserStates>
	{
		public ILexer<ConfigToken> Resolve(ILexer<ConfigToken> lexer, IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors>, ConfigParserStates> parser, ParserState<ConfigToken, ConfigParserStates> state, ParserErrorBase<ConfigParserErrors> error)
		{
			switch(state.CurrentState)
			{
				case ConfigParserStates.Value:
					return ResolveValue(lexer, parser, state, error.Message);
				case ConfigParserStates.Array:
					return ResolveArray(lexer, parser, state, error.Message);
				case ConfigParserStates.VariableExpression:
					return ResolveVariableExpression(lexer, parser, state, error.Message);
				case ConfigParserStates.ArrayExpression:
					return ResolveArrayExpression(lexer, parser, state, error.Message);
				case ConfigParserStates.Class:
					return ResolveClass(lexer, parser, state, error.Message);
				case ConfigParserStates.RootScope:
					return ResolveRootScope(lexer, parser, state, error.Message);
			}

			throw new ArgumentException($"State {state.CurrentState} is not supported by the resolver.");
		}

		protected ILexer<ConfigToken> ResolveRootScope(ILexer<ConfigToken> lexer, IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors>, ConfigParserStates> parser, ParserState<ConfigToken, ConfigParserStates> state, ConfigParserErrors error)
		{
			ConfigToken replacementToken;
			switch (error)
			{
				case ConfigParserErrors.UnexpectedToken:
					replacementToken = new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "Dummy", 0, 0, 0);
					break;
				default:
					throw new ArgumentException($"Error {error} is not supported by the resolver for RootScope state.");
			}
			return ResolveBySkipOrReplaceOrInject(lexer, parser, state, replacementToken);
		}

		protected ILexer<ConfigToken> ResolveClass(ILexer<ConfigToken> lexer, IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors>, ConfigParserStates> parser, ParserState<ConfigToken, ConfigParserStates> state, ConfigParserErrors error)
		{
			ConfigToken replacementToken;
			switch (error)
			{
				case ConfigParserErrors.ExpectedClassKeyword:
					replacementToken = new ConfigToken(ConfigToken.ConfigTokenType.Keyword_Class, "class", 0, 0, 0);
					break;
				case ConfigParserErrors.UnexpectedToken:
				case ConfigParserErrors.ExpectedIdentifier:
					replacementToken = new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "Dummy", 0, 0, 0);
					break;
				case ConfigParserErrors.ExpectedLeftCurlyBracket:
					replacementToken = new ConfigToken(ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft, "{", 0, 0, 0);
					break;
				case ConfigParserErrors.ExpectedRightCurlyBracket:
					replacementToken = new ConfigToken(ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight, "}", 0, 0, 0);
					break;
				case ConfigParserErrors.ExpectedSemicolumn:
					return lexer.Prepend(new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Semicolumn, ";", 0, 0, 0));
				default:
					throw new ArgumentException($"Error {error} is not supported by the resolver for Class state.");
			}
			return ResolveBySkipOrReplaceOrInject(lexer, parser, state, replacementToken);
		}

		protected ILexer<ConfigToken> ResolveArrayExpression(ILexer<ConfigToken> lexer, IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors>, ConfigParserStates> parser, ParserState<ConfigToken, ConfigParserStates> state, ConfigParserErrors error)
		{
			ConfigToken replacementToken;
			switch (error)
			{
				case ConfigParserErrors.ExpectedArrayIdentifier:
					replacementToken = new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "Dummy[]", 0, 0, 0);
					break;
				case ConfigParserErrors.ExpectedOperator:
					replacementToken = new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Assign, "=", 0, 0, 0);
					break;
				case ConfigParserErrors.ExpectedSemicolumn:
					return lexer.Prepend(new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Semicolumn, ";", 0, 0, 0));
				default:
					throw new ArgumentException($"Error {error} is not supported by the resolver for ArrayExpression state.");
			}
			return ResolveBySkipOrReplaceOrInject(lexer, parser, state, replacementToken);
		}

		protected ILexer<ConfigToken> ResolveVariableExpression(ILexer<ConfigToken> lexer, IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors>, ConfigParserStates> parser, ParserState<ConfigToken, ConfigParserStates> state, ConfigParserErrors error)
		{
			ConfigToken replacementToken;
			switch (error)
			{
				case ConfigParserErrors.ExpectedIdentifier:
					replacementToken = new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "Dummy", 0, 0, 0);
					break;
				case ConfigParserErrors.ExpectedOperator:
					replacementToken = new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Assign, "=", 0, 0, 0);
					break;
				case ConfigParserErrors.ExpectedSemicolumn:
					return lexer.Prepend(new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Semicolumn, ";", 0, 0, 0));
				default:
					throw new ArgumentException($"Error {error} is not supported by the resolver for VariableExpression state.");
			}
			return ResolveBySkipOrReplaceOrInject(lexer, parser, state, replacementToken);
		}

		protected ILexer<ConfigToken> ResolveArray(ILexer<ConfigToken> lexer, IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors>, ConfigParserStates> parser, ParserState<ConfigToken, ConfigParserStates> state, ConfigParserErrors error)
		{
			ConfigToken replacementToken;
			switch (error)
			{
				case ConfigParserErrors.ExpectedLeftCurlyBracket:
					replacementToken = new ConfigToken(ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft, "{", 0, 0, 0);
					break;
				case ConfigParserErrors.ExpectedRightCurlyBracket:
					replacementToken = new ConfigToken(ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight, "}", 0, 0, 0);
					break;
				case ConfigParserErrors.ExpectedComma:
					replacementToken = new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Comma, ",", 0, 0, 0);
					break;
				default:
					throw new ArgumentException($"Error {error} is not supported by the resolver for Array state.");
			}
			return ResolveBySkipOrReplaceOrInject(lexer, parser, state, replacementToken);
		}

		protected ILexer<ConfigToken> ResolveValue(ILexer<ConfigToken> lexer, IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors>, ConfigParserStates> parser, ParserState<ConfigToken, ConfigParserStates> state, ConfigParserErrors error)
		{
			if (error == ConfigParserErrors.BrokenString)
			{
				IEnumerable<ConfigToken> tokens = state.ConsumedTokens;

				if (!state.CurrentTokenConsumed)
					lexer.Consume();
				else
					tokens = tokens.SkipLast(1);

				var correctedString = new ConfigToken(ConfigToken.ConfigTokenType.String, state.CurrentToken.Token + '\"', state.CurrentToken.Index, state.CurrentToken.Line, state.CurrentToken.IndexOnLine);
				tokens = tokens.Append(correctedString);
				return lexer.Prepend(tokens);
			}
			else if (error == ConfigParserErrors.InvalidNumber)
			{
				IEnumerable<ConfigToken> tokens = state.ConsumedTokens;

				if (!state.CurrentTokenConsumed)
					lexer.Consume();
				else
					tokens = tokens.SkipLast(1);

				var correctedNumber = new ConfigToken(ConfigToken.ConfigTokenType.Number, "0", state.CurrentToken.Index, state.CurrentToken.Line, state.CurrentToken.IndexOnLine);
				tokens = tokens.Append(correctedNumber);
				return lexer.Prepend(tokens);
			}
			else if (error == ConfigParserErrors.UnexpectedToken)
			{
				var replacementToken = new ConfigToken(ConfigToken.ConfigTokenType.Number, "0", 0, 0, 0);
				return ResolveBySkipOrReplaceOrInject(lexer, parser, state, replacementToken);
			}
			else
				throw new ArgumentException($"Error {error} is not supported by the resolver for Value state.");
		}

		protected static ILexer<ConfigToken> ResolveBySkipOrReplaceOrInject(ILexer<ConfigToken> lexer, IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors>, ConfigParserStates> parser, ParserState<ConfigToken, ConfigParserStates> state, ConfigToken newToken)
		{
			//If we reached EndOfDocument, we can't drop the token, we have to prepend
			if (state.CurrentToken.TokenType == ConfigToken.ConfigTokenType.EndOfDocument)
				return lexer.Prepend(newToken);

			//Try parsing with the next token skipped
			int errorCountOnRemoval = parser.TryParseFromState(BuildLexerWithSkippedToken(lexer, state), state.CurrentState).Count();

			int errorsOnReplace = parser.TryParseFromState(BuildLexerWithReplacedToken(lexer, state, newToken), state.CurrentState).Count();

			//Try parsing with a dummy token inserted in front of the erroneous token
			int errorsOnInsert = parser.TryParseFromState(BuildLexerWithInjectedToken(lexer, state, newToken), state.CurrentState).Count();

			int minErrors = Math.Min(Math.Min(errorCountOnRemoval, errorsOnReplace), errorsOnInsert);

			//Choose the best option based on the number of errors during the parsing where removal > replacement > injection
			if (errorCountOnRemoval == minErrors)
			{
				lexer.Consume();
				return lexer;
			}
			else if (errorsOnReplace == minErrors)
			{
				lexer.Consume();
				return lexer.Prepend(newToken);
			}
			else
				return lexer.Prepend(newToken);
		}

		protected static ILexer<ConfigToken> BuildLexerWithSkippedToken(ILexer<ConfigToken> lexer, ParserState<ConfigToken, ConfigParserStates> state)
		{
			IEnumerable<ConfigToken> tokens = state.ConsumedTokens;
			var newLexer = lexer.AsPreview();

			if (state.CurrentTokenConsumed)
				tokens = tokens.SkipLast(1);
			else
				newLexer.Consume();

			return newLexer.Prepend(tokens);
		}

		protected static ILexer<ConfigToken> BuildLexerWithReplacedToken(ILexer<ConfigToken> lexer, ParserState<ConfigToken, ConfigParserStates> state, ConfigToken replacementToken)
		{
			IEnumerable<ConfigToken> tokens = state.ConsumedTokens;
			var newLexer = lexer.AsPreview();

			if (state.CurrentTokenConsumed)
				tokens = tokens.SkipLast(1);
			else
				newLexer.Consume();

			return newLexer.Prepend(replacementToken).Prepend(tokens);
		}

		protected static ILexer<ConfigToken> BuildLexerWithInjectedToken(ILexer<ConfigToken> lexer, ParserState<ConfigToken, ConfigParserStates> state, ConfigToken injectedToken)
		{
			IEnumerable<ConfigToken> tokens = state.ConsumedTokens;
			var newLexer = lexer.AsPreview();

			if (state.CurrentTokenConsumed)
			{
				tokens = tokens.SkipLast(1);
				newLexer = newLexer.Prepend(state.CurrentToken);
			}

			return newLexer.Prepend(injectedToken).Prepend(tokens);
		}
	}
}
