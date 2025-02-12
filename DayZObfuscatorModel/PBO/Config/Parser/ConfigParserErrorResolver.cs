﻿using DayZObfuscatorModel.Parser;
using DayZObfuscatorModel.PBO.Config.Parser.Lexer;
using SimpleParser;

namespace DayZObfuscatorModel.PBO.Config.Parser
{
	public class ConfigParserErrorResolver : ParserErrorResolverBase<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates>
	{
		public ConfigParserErrorResolver() : base(IsEndOfDocument)
		{
			AddErrorResolver(ConfigParserStates.Value, ResolveValue);
			AddErrorResolver(ConfigParserStates.Array, ResolveArray);
			AddErrorResolver(ConfigParserStates.VariableExpression, ResolveVariableExpression);
			AddErrorResolver(ConfigParserStates.ArrayExpression, ResolveArrayExpression);
			AddErrorResolver(ConfigParserStates.Class, ResolveClass);
			AddErrorResolver(ConfigParserStates.RootScope, ResolveRootScope);
		}

		private static bool IsEndOfDocument(ConfigToken token) => token.TokenType == ConfigToken.ConfigTokenType.EndOfDocument;

		protected ILexer<ConfigToken> ResolveRootScope(ILexer<ConfigToken> lexer, IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates> parser, ParserState<ConfigToken, ConfigParserStates> state, ParserErrorBase<ConfigParserErrors, ConfigToken> error)
		{
			switch (error.Message)
			{
				case ConfigParserErrors.UnexpectedToken:
					return ResolveByInjectReplaceSkip(lexer, parser, state, new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "Dummy", 0, 0, 0));
				default:
					throw new ArgumentException($"Error {error} is not supported by the resolver for RootScope state.");
			}
		}

		protected ILexer<ConfigToken> ResolveClass(ILexer<ConfigToken> lexer, IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates> parser, ParserState<ConfigToken, ConfigParserStates> state, ParserErrorBase<ConfigParserErrors, ConfigToken> error)
		{
			switch (error.Message)
			{
				case ConfigParserErrors.ExpectedClassKeyword:
					return ResolveByInjectReplaceSkip(lexer, parser, state, new ConfigToken(ConfigToken.ConfigTokenType.Keyword_Class, "class", 0, 0, 0));
				case ConfigParserErrors.UnexpectedToken:
				case ConfigParserErrors.ExpectedIdentifier:
					return ResolveByInjectReplaceSkip(lexer, parser, state, new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "Dummy", 0, 0, 0));
				case ConfigParserErrors.ExpectedLeftCurlyBracket:
					return ResolveByInjectReplaceSkip(lexer, parser, state, new ConfigToken(ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft, "{", 0, 0, 0));
				case ConfigParserErrors.ExpectedRightCurlyBracket:
					return ResolveByInjectReplaceSkip(lexer, parser, state, new ConfigToken(ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight, "}", 0, 0, 0));
				case ConfigParserErrors.ExpectedSemicolumn:
					return ResolveByInjectReplaceSkip(lexer, parser, state, new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Semicolumn, ";", 0, 0, 0));
				default:
					throw new ArgumentException($"Error {error} is not supported by the resolver for Class state.");
			}
		}

		protected ILexer<ConfigToken> ResolveArrayExpression(ILexer<ConfigToken> lexer, IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates> parser, ParserState<ConfigToken, ConfigParserStates> state, ParserErrorBase<ConfigParserErrors, ConfigToken> error)
		{
			switch (error.Message)
			{
				case ConfigParserErrors.ExpectedArrayIdentifier:
					return ResolveByInjectReplaceSkip(lexer, parser, state, new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "Dummy[]", 0, 0, 0));
				case ConfigParserErrors.ExpectedOperator:
					return ResolveByInjectReplaceSkip(lexer, parser, state, new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Assign, "=", 0, 0, 0));
				case ConfigParserErrors.ExpectedSemicolumn:
					return ResolveByInjectReplaceSkip(lexer, parser, state, new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Semicolumn, ";", 0, 0, 0));
				default:
					throw new ArgumentException($"Error {error} is not supported by the resolver for ArrayExpression state.");
			}
		}

		protected ILexer<ConfigToken> ResolveVariableExpression(ILexer<ConfigToken> lexer, IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates> parser, ParserState<ConfigToken, ConfigParserStates> state, ParserErrorBase<ConfigParserErrors, ConfigToken> error)
		{
			switch (error.Message)
			{
				case ConfigParserErrors.ExpectedIdentifier:
					return ResolveByInjectReplaceSkip(lexer, parser, state, new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "Dummy", 0, 0, 0));
				case ConfigParserErrors.ExpectedOperator:
					return ResolveByInjectReplaceSkip(lexer, parser, state, new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Assign, "=", 0, 0, 0));
				case ConfigParserErrors.ExpectedSemicolumn:
					return ResolveByInjectReplaceSkip(lexer, parser, state, new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Semicolumn, ";", 0, 0, 0));
				default:
					throw new ArgumentException($"Error {error} is not supported by the resolver for VariableExpression state.");
			}
		}

		protected ILexer<ConfigToken> ResolveArray(ILexer<ConfigToken> lexer, IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates> parser, ParserState<ConfigToken, ConfigParserStates> state, ParserErrorBase<ConfigParserErrors, ConfigToken> error)
		{
			switch (error.Message)
			{
				case ConfigParserErrors.ExpectedLeftCurlyBracket:
					return ResolveByInjectReplaceSkip(lexer, parser, state, new ConfigToken(ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft, "{", 0, 0, 0));
				case ConfigParserErrors.ExpectedRightCurlyBracket:
					return ResolveByInjectReplaceSkip(lexer, parser, state, new ConfigToken(ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight, "}", 0, 0, 0));
				case ConfigParserErrors.ExpectedCommaOrRightCurlyBracket:
					{
						ConfigToken nextToken;
						nextToken = lexer.Peek(2).First();

						if (nextToken.TokenType == ConfigToken.ConfigTokenType.Number || nextToken.TokenType == ConfigToken.ConfigTokenType.String || nextToken.TokenType == ConfigToken.ConfigTokenType.BrokenString)
							return lexer.Prepend(new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Comma, ",", 0, 0, 0));
						else
							return ResolveByInjectReplaceSkip(lexer, parser, state, new ConfigToken(ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight, "}", 0, 0, 0));
					}
				default:
					throw new ArgumentException($"Error {error} is not supported by the resolver for Array state.");
			}
		}

		protected ILexer<ConfigToken> ResolveValue(ILexer<ConfigToken> lexer, IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates> parser, ParserState<ConfigToken, ConfigParserStates> state, ParserErrorBase<ConfigParserErrors, ConfigToken> error)
		{
			if (error.Message == ConfigParserErrors.BrokenString)
			{
				IEnumerable<ConfigToken> tokens = state.ConsumedTokens;

				lexer.Consume();

				var correctedString = new ConfigToken(ConfigToken.ConfigTokenType.String, state.CurrentToken.Token + '\"', state.CurrentToken.Index, state.CurrentToken.Line, state.CurrentToken.IndexOnLine);
				tokens = tokens.Append(correctedString);
				return lexer.Prepend(tokens);
			}
			else if (error.Message == ConfigParserErrors.InvalidNumber)
			{
				IEnumerable<ConfigToken> tokens = state.ConsumedTokens;

				lexer.Consume();

				var correctedNumber = new ConfigToken(ConfigToken.ConfigTokenType.Number, "0", state.CurrentToken.Index, state.CurrentToken.Line, state.CurrentToken.IndexOnLine);
				tokens = tokens.Append(correctedNumber);
				return lexer.Prepend(tokens);
			}
			else if (error.Message == ConfigParserErrors.UnexpectedToken)
			{
				var replacementToken = new ConfigToken(ConfigToken.ConfigTokenType.Number, "0", 0, 0, 0);
				return ResolveByReplaceInjectSkip(lexer, parser, state, replacementToken);
			}
			else
				throw new ArgumentException($"Error {error} is not supported by the resolver for Value state.");
		}
	}
}
