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
			}

			throw new ArgumentException($"State {state.CurrentState} is not supported by the resolver.");
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

				//If we reached EndOfDocument, we can't drop the token, we have to prepend
				if (state.CurrentToken.TokenType == ConfigToken.ConfigTokenType.EndOfDocument)
					return lexer.Prepend(replacementToken);

				//Try parsing with the next token skipped
				IEnumerable<ConfigToken> tokens = state.ConsumedTokens;
				var newLexer = lexer.AsPreview();

				if (state.CurrentTokenConsumed)
					tokens = tokens.SkipLast(1);
				else
					newLexer.Consume();

				newLexer = newLexer.Prepend(tokens);

				int errorCountOnRemoval = parser.TryParseFromState(newLexer, ConfigParserStates.Value).Count();

				//Try parsing with a dummy token inserted in front of the erroneous token
				tokens = state.ConsumedTokens;
				newLexer = lexer.AsPreview();
				
				if (state.CurrentTokenConsumed)
				{
					tokens = tokens.SkipLast(1);
					newLexer = newLexer.Prepend(state.CurrentToken);
				}

				newLexer = newLexer.Prepend(replacementToken).Prepend(tokens);

				int errorsOnSubstitution = parser.TryParseFromState(newLexer, ConfigParserStates.Value).Count();

				//Choose the best option based on the number of errors during the parsing, prefering token removal in case of a tie.
				if (errorCountOnRemoval <= errorsOnSubstitution)
				{
					lexer.Consume();
					return lexer;
				}
				else
					return lexer.Prepend(replacementToken);
			}
			else
				throw new ArgumentException($"Error {error} is not supported by the resolver for Value state.");
		}
	}
}
