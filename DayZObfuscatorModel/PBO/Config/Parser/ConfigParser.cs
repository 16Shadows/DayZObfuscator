using DayZObfuscatorModel.Parser;
using DayZObfuscatorModel.PBO.Config.Parser.Lexer;
using System.Globalization;

namespace DayZObfuscatorModel.PBO.Config.Parser
{
	public enum ConfigParserStates
	{
		ArrayExpression,
		VariableExpression,
		Delete,
		Value,
		Array,
		Class,
		RootScope
	}

	public enum ConfigParserErrors
	{
		UnexpectedToken,
		ExpectedArrayIdentifier,
		ExpectedIdentifier,
		ExpectedOperator,
		ExpectedSemicolumn,
		ExpectedLeftCurlyBracket,
		ExpectedRightCurlyBracket,
		ExpectedCommaOrRightCurlyBracket,
		InvalidNumber,
		ExpectedClassKeyword,
		ExpectedDeleteKeyword,
		BrokenString
	}

	public class ConfigParser : IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates>
	{
		public ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>> Parse(ILexer<ConfigToken> lexer, IParserErrorResolver<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates> errorResolver)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors, ConfigToken>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors, ConfigToken>>();
			IEnumerable<ConfigParserStates> states = Enumerable.Repeat(ConfigParserStates.RootScope, 1);

			PBOConfig rootScope = new PBOConfig();

			ConfigToken nextToken;
			while (true)
			{
				nextToken = lexer.Peek();

				if (nextToken.TokenType != ConfigToken.ConfigTokenType.EndOfDocument && nextToken.TokenType != ConfigToken.ConfigTokenType.Keyword_Class && nextToken.TokenType != ConfigToken.ConfigTokenType.Identifier)
				{
					var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(nextToken, ConfigParserErrors.UnexpectedToken);
					errors = errors.Append(error);
					lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, nextToken), error);
					nextToken = lexer.Peek();
				}

				if (nextToken.TokenType == ConfigToken.ConfigTokenType.EndOfDocument)
					break;
				else if (nextToken.TokenType == ConfigToken.ConfigTokenType.Keyword_Class)
				{
					var result = ParseClass(lexer, states, errorResolver);

					if (result.Result is PBOConfigClass pboClass)
						rootScope.Classes.Add(pboClass);
					else if (result.Result is PBOConfigExpressionBase expr)
						rootScope.Expressions.Add(expr);
					else
						throw new InvalidOperationException($"Result of type {result.Result.GetType().Name} is unhandled.");

					errors = errors.Concat(result.Errors);
				}
				else if (nextToken.TokenType == ConfigToken.ConfigTokenType.Identifier)
				{
					var result = ParseExpression(lexer, states, errorResolver);
					rootScope.Expressions.Add(result.Result);
					errors = errors.Concat(result.Errors);
				}
				else if (nextToken.TokenType == ConfigToken.ConfigTokenType.Keyword_Delete)
				{
					var result = ParseDelete(lexer, states, errorResolver);
					rootScope.Expressions.Add(result.Result);
					errors = errors.Concat(result.Errors);
				}
			}

			return new ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>>(rootScope, errors.Count() == 0, errors);
		}

		public IEnumerable<ParserErrorBase<ConfigParserErrors, ConfigToken>> TryParseFromState(ILexer<ConfigToken> lexer, ConfigParserStates state, IEnumerable<ConfigParserStates> stateStack, IParserErrorResolver<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates> errorResolver)
		{
			switch (state)
			{
				case ConfigParserStates.RootScope:
					return Parse(lexer, errorResolver).Errors;
				case ConfigParserStates.ArrayExpression:
					return ParseArrayExpression(lexer, stateStack, errorResolver).Errors;
				case ConfigParserStates.VariableExpression:
					return ParseVariableExpression(lexer, stateStack, errorResolver).Errors;
				case ConfigParserStates.Value:
					return ParseValue(lexer, stateStack, errorResolver).Errors;
				case ConfigParserStates.Array:
					return ParseArray(lexer, stateStack, errorResolver).Errors;
				case ConfigParserStates.Class:
					return ParseClass(lexer, stateStack, errorResolver).Errors;
			}
			throw new ArgumentException("Unsupported parser state.");
		}

		protected ParseResult<PBOConfigExpressionDelete, ParserErrorBase<ConfigParserErrors, ConfigToken>> ParseDelete(ILexer<ConfigToken> lexer, IEnumerable<ConfigParserStates> states, IParserErrorResolver<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates> errorResolver)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors, ConfigToken>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors, ConfigToken>>();
			states = states.Append(ConfigParserStates.Delete);

			ConfigToken keyword = lexer.Peek();
			if (keyword.TokenType != ConfigToken.ConfigTokenType.Keyword_Delete)
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(keyword, ConfigParserErrors.ExpectedDeleteKeyword);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, keyword), error);
			}
			keyword = lexer.Consume();

			ConfigToken identifier = lexer.Peek();
			if (identifier.TokenType != ConfigToken.ConfigTokenType.Identifier || identifier.Token.EndsWith("[]"))
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(identifier, ConfigParserErrors.ExpectedIdentifier);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, identifier, keyword), error);
			}
			identifier = lexer.Consume();

			ConfigToken semicolumn = lexer.Peek();
			if (semicolumn.TokenType != ConfigToken.ConfigTokenType.Symbol_Semicolumn)
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(semicolumn, ConfigParserErrors.ExpectedSemicolumn);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, semicolumn, keyword, identifier), error);
			}
			semicolumn = lexer.Consume();

			return new ParseResult<PBOConfigExpressionDelete, ParserErrorBase<ConfigParserErrors, ConfigToken>>(new PBOConfigExpressionDelete(identifier.Token), true, errors);
		}

		protected ParseResult<PBOConfigExpressionBase, ParserErrorBase<ConfigParserErrors, ConfigToken>> ParseExpression(ILexer<ConfigToken> lexer, IEnumerable<ConfigParserStates> states, IParserErrorResolver<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates> errorResolver)
		{
			ConfigToken identifier = lexer.Peek();
			
			if (identifier.Token.EndsWith("[]"))
				return ParseArrayExpression(lexer, states, errorResolver).WithResultAs<PBOConfigExpressionBase>(x => x);
			else
				return ParseVariableExpression(lexer, states, errorResolver);
		}

		protected ParseResult<PBOConfigArrayExpressionBase, ParserErrorBase<ConfigParserErrors, ConfigToken>> ParseArrayExpression(ILexer<ConfigToken> lexer, IEnumerable<ConfigParserStates> states, IParserErrorResolver<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates> errorResolver)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors, ConfigToken>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors, ConfigToken>>();
			states = states.Append(ConfigParserStates.ArrayExpression);

			ConfigToken identifier = lexer.Peek();
			if (identifier.TokenType != ConfigToken.ConfigTokenType.Identifier || !identifier.Token.EndsWith("[]"))
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(identifier, ConfigParserErrors.ExpectedArrayIdentifier);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, identifier), error);
			}
			identifier = lexer.Consume();

			ConfigToken expressionToken = lexer.Peek();
			
			PBOConfigArrayExpressionBase? expr = null;
			
			if (expressionToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Assign && expressionToken.TokenType != ConfigToken.ConfigTokenType.Symbol_PlusAssign && expressionToken.TokenType != ConfigToken.ConfigTokenType.Symbol_MinusAssign)
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(expressionToken, ConfigParserErrors.ExpectedOperator);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, expressionToken, identifier), error);
			}

			expressionToken = lexer.Consume();

			if (expressionToken.TokenType == ConfigToken.ConfigTokenType.Symbol_Assign)
			{
				var array = ParseArray(lexer, states, errorResolver);
				expr = new PBOConfigArrayExpressionAssignment(identifier.TokenTrimmed, array.Result);
				errors = errors.Concat(array.Errors);
			}
			else if (expressionToken.TokenType == ConfigToken.ConfigTokenType.Symbol_PlusAssign)
			{
				var array = ParseArray(lexer, states, errorResolver);
				expr = new PBOConfigArrayExpressionAdd(identifier.TokenTrimmed, array.Result);
				errors = errors.Concat(array.Errors);
			}
			else if (expressionToken.TokenType == ConfigToken.ConfigTokenType.Symbol_MinusAssign)
			{
				var array = ParseArray(lexer, states, errorResolver);
				expr = new PBOConfigArrayExpressionSubtract(identifier.TokenTrimmed, array.Result);
				errors = errors.Concat(array.Errors);
			}
			else
				throw new InvalidSyntaxException($"Expected operator, found '{expressionToken.Token}'", expressionToken.Index, expressionToken.Line, expressionToken.IndexOnLine);

			ConfigToken nextToken = lexer.Peek();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Semicolumn)
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(nextToken, ConfigParserErrors.ExpectedSemicolumn);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, nextToken, identifier, expressionToken), error);
			}
			nextToken = lexer.Consume();

			return new ParseResult<PBOConfigArrayExpressionBase, ParserErrorBase<ConfigParserErrors, ConfigToken>>(expr, true, errors);
		}

		protected ParseResult<PBOConfigExpressionBase, ParserErrorBase<ConfigParserErrors, ConfigToken>> ParseVariableExpression(ILexer<ConfigToken> lexer, IEnumerable<ConfigParserStates> states, IParserErrorResolver<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates> errorResolver)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors, ConfigToken>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors, ConfigToken>>();
			states = states.Append(ConfigParserStates.VariableExpression);

			ConfigToken identifier = lexer.Peek();
			if (identifier.TokenType != ConfigToken.ConfigTokenType.Identifier || identifier.Token.EndsWith("[]"))
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(identifier, ConfigParserErrors.ExpectedIdentifier);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, identifier), error);
			}
			identifier = lexer.Consume();

			ConfigToken expressionToken = lexer.Peek();
			if ((expressionToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Assign))
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(expressionToken, ConfigParserErrors.ExpectedOperator);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, expressionToken, identifier), error);
			}
			expressionToken = lexer.Consume();

			ParseResultWithTokens<PBOConfigVariableValue, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigToken> value;

			PBOConfigExpressionBase? expr = null;
			if (expressionToken.TokenType == ConfigToken.ConfigTokenType.Symbol_Assign)
			{
				value = ParseValue(lexer, states, errorResolver);
				expr = new PBOConfigExpressionVariableAssignment(identifier.TokenTrimmed, value.Result);
				errors = errors.Concat(value.Errors);
			}
			else
				throw new InvalidSyntaxException($"Expected expression symbol, found '{identifier.Token}'", identifier.Index, identifier.Line, identifier.IndexOnLine);
			
			ConfigToken nextToken = lexer.Peek();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Semicolumn)
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(nextToken, ConfigParserErrors.ExpectedSemicolumn);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, nextToken, value.ConsumedTokens.Prepend(expressionToken).Prepend(identifier)), error);
			}
			nextToken = lexer.Consume();

			return new ParseResult<PBOConfigExpressionBase, ParserErrorBase<ConfigParserErrors, ConfigToken>>(expr, true, errors);
		}

		protected ParseResultWithTokens<PBOConfigVariableValue, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigToken> ParseValue(ILexer<ConfigToken> lexer, IEnumerable<ConfigParserStates> states, IParserErrorResolver<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates> errorResolver)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors, ConfigToken>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors, ConfigToken>>();
			states = states.Append(ConfigParserStates.Value);

			ConfigToken nextToken = lexer.Peek();
			if (nextToken.TokenType == ConfigToken.ConfigTokenType.Number)
			{
				if (int.TryParse(nextToken.Token, CultureInfo.InvariantCulture, out int intValue))
				{
					_ = lexer.Consume();
					return new ParseResultWithTokens<PBOConfigVariableValue, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigToken>(new PBOConfigValueInt(intValue), true, errors, nextToken);
				}
				else if (float.TryParse(nextToken.Token, CultureInfo.InvariantCulture, out float dValue))
				{
					_ = lexer.Consume();
					return new ParseResultWithTokens<PBOConfigVariableValue, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigToken>( new PBOConfigValueFloat(dValue), true, errors, nextToken);
				}
				else
				{
					var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(nextToken, ConfigParserErrors.InvalidNumber);
					errors = errors.Append(error);
					lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, nextToken), error);
					return (ParseResultWithTokens<PBOConfigVariableValue, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigToken>)ParseValue(lexer, states, errorResolver).TransformErrors(e => errors.Concat(e));
				}
			}
			else if (nextToken.TokenType == ConfigToken.ConfigTokenType.String)
			{
				lexer.Consume();
				return new ParseResultWithTokens<PBOConfigVariableValue, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigToken>(new PBOConfigValueString(nextToken.Token[1..^1]), true, errors, nextToken);
			}
			else if (nextToken.TokenType == ConfigToken.ConfigTokenType.BrokenString)
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(nextToken, ConfigParserErrors.BrokenString);
				errors = errors.Append(error);
				lexer =	errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, nextToken), error);
				return (ParseResultWithTokens<PBOConfigVariableValue, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigToken>)ParseValue(lexer, states, errorResolver).TransformErrors(e => errors.Concat(e));
			}
			else
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(nextToken, ConfigParserErrors.UnexpectedToken);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, nextToken), error);
				return (ParseResultWithTokens<PBOConfigVariableValue, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigToken>)ParseValue(lexer, states, errorResolver).TransformErrors(e => errors.Concat(e));
			}
		}

		protected ParseResult<PBOConfigArray, ParserErrorBase<ConfigParserErrors, ConfigToken>> ParseArray(ILexer<ConfigToken> lexer, IEnumerable<ConfigParserStates> states, IParserErrorResolver<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates> errorResolver)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors, ConfigToken>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors, ConfigToken>>();
			states = states.Append(ConfigParserStates.Array);

			ConfigToken leftBracket = lexer.Peek();
			if (leftBracket.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft)
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(leftBracket, ConfigParserErrors.ExpectedLeftCurlyBracket);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, leftBracket), error);
			}

			leftBracket = lexer.Consume();

			List<PBOConfigValueBase> arr = new List<PBOConfigValueBase>();
			ConfigToken firstValue = lexer.Peek();

			if (firstValue.TokenType == ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight)
			{
				lexer.Consume();
				return new ParseResult<PBOConfigArray, ParserErrorBase<ConfigParserErrors, ConfigToken>>(new PBOConfigArray(arr), true, errors);
			}
			else if (firstValue.TokenType == ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft)
			{
				var result = ParseArray(lexer, states, errorResolver);
				arr.Add(result.Result);
				errors = errors.Concat(result.Errors);
			}
			else
			{
				var result = ParseValue(lexer, states, errorResolver);
				arr.Add(result.Result);
				errors = errors.Concat(result.Errors);
			}
				

			ConfigToken nextToken;
			while (true)
			{
				nextToken = lexer.Peek();
				if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Comma && nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight && nextToken.TokenType != ConfigToken.ConfigTokenType.EndOfDocument)
				{
					var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(nextToken, ConfigParserErrors.ExpectedCommaOrRightCurlyBracket);
					errors = errors.Append(error);
					lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, nextToken, leftBracket, firstValue), error);
				}
				nextToken = lexer.Peek();

				if (nextToken.TokenType == ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight || nextToken.TokenType == ConfigToken.ConfigTokenType.EndOfDocument)
					break;
				
				nextToken = lexer.Consume();

				if (firstValue.TokenType == ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft)
				{
					var result = ParseArray(lexer, states, errorResolver);
					arr.Add(result.Result);
					errors = errors.Concat(result.Errors);
				}
				else
				{
					var result = ParseValue(lexer, states, errorResolver);
					arr.Add(result.Result);
					errors = errors.Concat(result.Errors);
				}
			}

			nextToken = lexer.Peek();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight)
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(nextToken, ConfigParserErrors.ExpectedRightCurlyBracket);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, nextToken, leftBracket, firstValue), error);
			}
			nextToken = lexer.Consume();

			return new ParseResult<PBOConfigArray, ParserErrorBase<ConfigParserErrors, ConfigToken>>(new PBOConfigArray(arr), true, errors);
		}

		protected ParseResult<object, ParserErrorBase<ConfigParserErrors, ConfigToken>> ParseClass(ILexer<ConfigToken> lexer, IEnumerable<ConfigParserStates> states, IParserErrorResolver<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>, ConfigParserStates> errorResolver)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors, ConfigToken>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors, ConfigToken>>();
			states = states.Append(ConfigParserStates.Class);
			List<ConfigToken> stateTokens = new List<ConfigToken>();

			ConfigToken nextToken = lexer.Peek();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Keyword_Class)
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(nextToken, ConfigParserErrors.ExpectedClassKeyword);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, nextToken, stateTokens), error);
			}
			nextToken = lexer.Consume();
			stateTokens.Add(nextToken);

			nextToken = lexer.Peek();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Identifier)
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(nextToken, ConfigParserErrors.ExpectedIdentifier);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, nextToken, stateTokens), error);
			}
			nextToken = lexer.Consume();
			stateTokens.Add(nextToken);
			string identifier = nextToken.Token;

			string? parentClass = null;
			nextToken = lexer.Peek();
			if (nextToken.TokenType == ConfigToken.ConfigTokenType.Symbol_Column)
			{
				nextToken = lexer.Consume();
				stateTokens.Add(nextToken);
				nextToken = lexer.Peek();
				if (nextToken.TokenType != ConfigToken.ConfigTokenType.Identifier)
				{
					var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(nextToken, ConfigParserErrors.ExpectedIdentifier);
					errors = errors.Append(error);
					lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, nextToken, stateTokens), error);
				}
				nextToken = lexer.Consume();
				stateTokens.Add(nextToken);
				parentClass = nextToken.Token;
			}
			else if (nextToken.TokenType == ConfigToken.ConfigTokenType.Symbol_Semicolumn)
			{
				nextToken = lexer.Consume();
				return new ParseResult<object, ParserErrorBase<ConfigParserErrors, ConfigToken>>( new PBOConfigExternalClass(identifier), true, errors );
			}

			nextToken = lexer.Peek();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft)
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(nextToken, ConfigParserErrors.ExpectedLeftCurlyBracket);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, nextToken, stateTokens), error);
			}
			nextToken = lexer.Consume();
			stateTokens.Add(nextToken);

			PBOConfigClass rootScope = new PBOConfigClass(identifier, parentClass);

			while (true)
			{
				nextToken = lexer.Peek();
				if (nextToken.TokenType == ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight || nextToken.TokenType == ConfigToken.ConfigTokenType.EndOfDocument)
					break;
				else if (nextToken.TokenType == ConfigToken.ConfigTokenType.Keyword_Class)
				{
					var result = ParseClass(lexer, states, errorResolver);
					
					if (result.Result is PBOConfigClass pboClass)
						rootScope.Classes.Add(pboClass);
					else if (result.Result is PBOConfigExpressionBase expr)
						rootScope.Expressions.Add(expr);
					else
						throw new InvalidOperationException($"Result of type {result.Result.GetType().Name} is unhandled.");

					errors = errors.Concat(result.Errors);
				}
				else if (nextToken.TokenType == ConfigToken.ConfigTokenType.Identifier)
				{
					var result = ParseExpression(lexer, states, errorResolver);
					rootScope.Expressions.Add(result.Result);
					errors = errors.Concat(result.Errors);
				}
				else if (nextToken.TokenType == ConfigToken.ConfigTokenType.Keyword_Delete)
				{
					var result = ParseDelete(lexer, states, errorResolver);
					rootScope.Expressions.Add(result.Result);
					errors = errors.Concat(result.Errors);
				}
				else
				{
					var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(nextToken, ConfigParserErrors.UnexpectedToken);
					errors = errors.Append(error);
					lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, nextToken, stateTokens), error);
				}
			}

			nextToken = lexer.Peek();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight)
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(nextToken, ConfigParserErrors.ExpectedRightCurlyBracket);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, nextToken, stateTokens), error);
			}
			nextToken = lexer.Consume();
			stateTokens.Add(nextToken);

			nextToken = lexer.Peek();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Semicolumn)
			{
				var error = new ParserErrorBase<ConfigParserErrors, ConfigToken>(nextToken, ConfigParserErrors.ExpectedSemicolumn);
				errors = errors.Append(error);
				lexer = errorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(states, nextToken, stateTokens), error);
			}
			nextToken = lexer.Consume();

			return new ParseResult<object, ParserErrorBase<ConfigParserErrors, ConfigToken>>(rootScope, true, errors);
		}
	}
}
