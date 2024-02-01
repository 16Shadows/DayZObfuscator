using DayZObfuscatorModel.Parser;
using DayZObfuscatorModel.PBO.Config.Parser.Lexer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.PBO.Config.Parser
{
	public enum ConfigParserStates
	{
		ArrayExpression,
		VariableExpression,
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
		BrokenString
	}

	public class ConfigParser : IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors>, ConfigParserStates>
	{
		protected IParserErrorResolver<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors>, ConfigParserStates> _ErrorResolver;

		public ConfigParser(IParserErrorResolver<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors>, ConfigParserStates> errorResolver)
		{
			_ErrorResolver = errorResolver ?? throw new ArgumentNullException(nameof(errorResolver));
		}

		public ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors>> Parse(ILexer<ConfigToken> lexer)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors>>();

			PBOConfig rootScope = new PBOConfig();

			ConfigToken nextToken;
			while (true)
			{
				nextToken = lexer.Peek();

				if (nextToken.TokenType != ConfigToken.ConfigTokenType.EndOfDocument && nextToken.TokenType != ConfigToken.ConfigTokenType.Keyword_Class && nextToken.TokenType != ConfigToken.ConfigTokenType.Identifier)
				{
					var error = new ParserErrorBase<ConfigParserErrors>(nextToken, ConfigParserErrors.UnexpectedToken);
					errors = errors.Append(error);
					lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.RootScope, nextToken, false), error);
					nextToken = lexer.Peek();
				}

				if (nextToken.TokenType == ConfigToken.ConfigTokenType.EndOfDocument)
					break;
				else if (nextToken.TokenType == ConfigToken.ConfigTokenType.Keyword_Class)
				{
					var result = ParseClass(lexer);
					rootScope.Expressions.Add(result.Result);
					errors = errors.Concat(result.Errors);
				}
				else if (nextToken.TokenType == ConfigToken.ConfigTokenType.Identifier)
				{
					var result = ParseExpression(lexer);
					rootScope.Expressions.Add(result.Result);
					errors = errors.Concat(result.Errors);
				}
			}

			return new ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors>>(rootScope, errors.Count() == 0, errors);
		}

		public IEnumerable<ParserErrorBase<ConfigParserErrors>> TryParseFromState(ILexer<ConfigToken> lexer, ConfigParserStates state)
		{
			switch (state)
			{
				case ConfigParserStates.RootScope:
					return Parse(lexer).Errors;
				case ConfigParserStates.ArrayExpression:
					return ParseArrayExpression(lexer).Errors;
				case ConfigParserStates.VariableExpression:
					return ParseVariableExpression(lexer).Errors;
				case ConfigParserStates.Value:
					return ParseValue(lexer).Errors;
				case ConfigParserStates.Array:
					return ParseArray(lexer).Errors;
				case ConfigParserStates.Class:
					return ParseClass(lexer).Errors;
			}
			throw new ArgumentException("Unsupported parser state.");
		}

		protected ParseResult<PBOConfigExpressionBase, ParserErrorBase<ConfigParserErrors>> ParseExpression(ILexer<ConfigToken> lexer)
		{
			ConfigToken identifier = lexer.Peek();
			
			if (identifier.Token.EndsWith("[]"))
				return ParseArrayExpression(lexer).WithResultAs<PBOConfigExpressionBase>(x => x);
			else
				return ParseVariableExpression(lexer);
		}

		protected ParseResult<PBOConfigArrayExpressionBase, ParserErrorBase<ConfigParserErrors>> ParseArrayExpression(ILexer<ConfigToken> lexer)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors>>();

			ConfigToken identifier = lexer.Peek();
			if (identifier.TokenType != ConfigToken.ConfigTokenType.Identifier || !identifier.Token.EndsWith("[]"))
			{
				var error = new ParserErrorBase<ConfigParserErrors>(identifier, ConfigParserErrors.ExpectedArrayIdentifier);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.ArrayExpression, identifier, false), error);
			}
			identifier = lexer.Consume();

			ConfigToken expressionToken = lexer.Peek();
			
			PBOConfigArrayExpressionBase? expr = null;
			
			if (expressionToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Assign && expressionToken.TokenType != ConfigToken.ConfigTokenType.Symbol_PlusAssign && expressionToken.TokenType != ConfigToken.ConfigTokenType.Symbol_MinusAssign)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(expressionToken, ConfigParserErrors.ExpectedOperator);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.ArrayExpression, expressionToken, false, identifier), error);
			}

			expressionToken = lexer.Consume();

			if (expressionToken.TokenType == ConfigToken.ConfigTokenType.Symbol_Assign)
			{
				var array = ParseArray(lexer);
				expr = new PBOConfigArrayExpressionAssignment(identifier.TokenTrimmed, array.Result);
				errors = errors.Concat(array.Errors);
			}
			else if (expressionToken.TokenType == ConfigToken.ConfigTokenType.Symbol_PlusAssign)
			{
				var array = ParseArray(lexer);
				expr = new PBOConfigArrayExpressionAdd(identifier.TokenTrimmed, array.Result);
				errors = errors.Concat(array.Errors);
			}
			else if (expressionToken.TokenType == ConfigToken.ConfigTokenType.Symbol_MinusAssign)
			{
				var array = ParseArray(lexer);
				expr = new PBOConfigArrayExpressionSubtract(identifier.TokenTrimmed, array.Result);
				errors = errors.Concat(array.Errors);
			}
			else
				throw new InvalidSyntaxException($"Expected operator, found '{expressionToken.Token}'", expressionToken.Index, expressionToken.Line, expressionToken.IndexOnLine);

			ConfigToken nextToken = lexer.Peek();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Semicolumn)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(nextToken, ConfigParserErrors.ExpectedSemicolumn);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.ArrayExpression, nextToken, false, identifier, expressionToken), error);
			}
			nextToken = lexer.Consume();

			return new ParseResult<PBOConfigArrayExpressionBase, ParserErrorBase<ConfigParserErrors>>(expr, true, errors);
		}

		protected ParseResult<PBOConfigExpressionBase, ParserErrorBase<ConfigParserErrors>> ParseVariableExpression(ILexer<ConfigToken> lexer)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors>>();

			ConfigToken identifier = lexer.Peek();
			if (identifier.TokenType != ConfigToken.ConfigTokenType.Identifier || identifier.Token.EndsWith("[]"))
			{
				var error = new ParserErrorBase<ConfigParserErrors>(identifier, ConfigParserErrors.ExpectedIdentifier);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.VariableExpression, identifier, false), error);
			}
			identifier = lexer.Consume();

			ConfigToken expressionToken = lexer.Peek();
			if ((expressionToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Assign))
			{
				var error = new ParserErrorBase<ConfigParserErrors>(expressionToken, ConfigParserErrors.ExpectedOperator);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.VariableExpression, expressionToken, false, identifier), error);
			}
			expressionToken = lexer.Consume();

			ParseResultWithTokens<object, ParserErrorBase<ConfigParserErrors>, ConfigToken> value;

			PBOConfigExpressionBase? expr = null;
			if (expressionToken.TokenType == ConfigToken.ConfigTokenType.Symbol_Assign)
			{
				value = ParseValue(lexer);
				expr = new PBOConfigExpressionVariableAssignment(identifier.TokenTrimmed, value.Result);
				errors = errors.Concat(value.Errors);
			}
			else
				throw new InvalidSyntaxException($"Expected expression symbol, found '{identifier.Token}'", identifier.Index, identifier.Line, identifier.IndexOnLine);
			
			ConfigToken nextToken = lexer.Peek();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Semicolumn)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(nextToken, ConfigParserErrors.ExpectedSemicolumn);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.VariableExpression, nextToken, false, value.ConsumedTokens.Prepend(expressionToken).Prepend(identifier)), error);
			}
			nextToken = lexer.Consume();

			return new ParseResult<PBOConfigExpressionBase, ParserErrorBase<ConfigParserErrors>>(expr, true, errors);
		}

		protected ParseResultWithTokens<object, ParserErrorBase<ConfigParserErrors>, ConfigToken> ParseValue(ILexer<ConfigToken> lexer)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors>>();

			ConfigToken nextToken = lexer.Peek();
			if (nextToken.TokenType == ConfigToken.ConfigTokenType.Number)
			{
				if (int.TryParse(nextToken.Token, CultureInfo.InvariantCulture, out int intValue))
				{
					_ = lexer.Consume();
					return new ParseResultWithTokens<object, ParserErrorBase<ConfigParserErrors>, ConfigToken>(intValue, true, errors, nextToken);
				}
				else if (double.TryParse(nextToken.Token, CultureInfo.InvariantCulture, out double dValue))
				{
					_ = lexer.Consume();
					return new ParseResultWithTokens<object, ParserErrorBase<ConfigParserErrors>, ConfigToken>(dValue, true, errors, nextToken);
				}
				else
				{
					var error = new ParserErrorBase<ConfigParserErrors>(nextToken, ConfigParserErrors.InvalidNumber);
					errors = errors.Append(error);
					lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Value, nextToken, false), error);
					return (ParseResultWithTokens<object, ParserErrorBase<ConfigParserErrors>, ConfigToken>)ParseValue(lexer).TransformErrors(e => errors.Concat(e));
				}
			}
			else if (nextToken.TokenType == ConfigToken.ConfigTokenType.String)
			{
				lexer.Consume();
				return new ParseResultWithTokens<object, ParserErrorBase<ConfigParserErrors>, ConfigToken>(new PBOConfigValueString(nextToken.Token[1..^1]), true, errors, nextToken);
			}
			else if (nextToken.TokenType == ConfigToken.ConfigTokenType.BrokenString)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(nextToken, ConfigParserErrors.BrokenString);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Value, nextToken, false), error);
				return (ParseResultWithTokens<object, ParserErrorBase<ConfigParserErrors>, ConfigToken>)ParseValue(lexer).TransformErrors(e => errors.Concat(e));
			}
			else
			{
				var error = new ParserErrorBase<ConfigParserErrors>(nextToken, ConfigParserErrors.UnexpectedToken);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Value, nextToken, false), error);
				return (ParseResultWithTokens<object, ParserErrorBase<ConfigParserErrors>, ConfigToken>)ParseValue(lexer).TransformErrors(e => errors.Concat(e));
			}
		}

		protected ParseResult<IList<object>, ParserErrorBase<ConfigParserErrors>> ParseArray(ILexer<ConfigToken> lexer)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors>>();

			ConfigToken leftBracket = lexer.Peek();
			if (leftBracket.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(leftBracket, ConfigParserErrors.ExpectedLeftCurlyBracket);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Array, leftBracket, false), error);
			}

			leftBracket = lexer.Consume();

			List<object> arr = new List<object>();
			ConfigToken firstValue = lexer.Peek();

			if (firstValue.TokenType == ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight)
			{
				lexer.Consume();
				return new ParseResult<IList<object>, ParserErrorBase<ConfigParserErrors>>(arr, true, errors);
			}
			else
			{
				var result = ParseValue(lexer);
				arr.Add(result.Result);
				errors = errors.Concat(result.Errors);
			}
				

			ConfigToken nextToken;
			while (true)
			{
				nextToken = lexer.Peek();
				if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Comma && nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight && nextToken.TokenType != ConfigToken.ConfigTokenType.EndOfDocument)
				{
					var error = new ParserErrorBase<ConfigParserErrors>(nextToken, ConfigParserErrors.ExpectedCommaOrRightCurlyBracket);
					errors = errors.Append(error);
					lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Array, nextToken, false, leftBracket, firstValue), error);
				}
				nextToken = lexer.Peek();

				if (nextToken.TokenType == ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight || nextToken.TokenType == ConfigToken.ConfigTokenType.EndOfDocument)
					break;


				nextToken = lexer.Consume();
				
				var result = ParseValue(lexer);
				arr.Add(result.Result);
				errors = errors.Concat(result.Errors);
			}

			nextToken = lexer.Peek();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(nextToken, ConfigParserErrors.ExpectedRightCurlyBracket);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Array, nextToken, false, leftBracket, firstValue), error);
			}
			nextToken = lexer.Consume();

			return new ParseResult<IList<object>, ParserErrorBase<ConfigParserErrors>>(arr, true, errors);
		}

		protected ParseResult<PBOConfigClass, ParserErrorBase<ConfigParserErrors>> ParseClass(ILexer<ConfigToken> lexer)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors>>();

			ConfigToken classToken = lexer.Peek();
			if (classToken.TokenType != ConfigToken.ConfigTokenType.Keyword_Class)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(classToken, ConfigParserErrors.ExpectedClassKeyword);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Class, classToken, false), error);
			}
			classToken = lexer.Consume();

			ConfigToken identifier = lexer.Peek();
			if (identifier.TokenType != ConfigToken.ConfigTokenType.Identifier)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(identifier, ConfigParserErrors.ExpectedIdentifier);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Class, identifier, false, classToken), error);
			}
			identifier = lexer.Consume();

			ConfigToken leftBracket = lexer.Peek();
			if (leftBracket.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(leftBracket, ConfigParserErrors.ExpectedLeftCurlyBracket);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Class, leftBracket, false, classToken, identifier), error);
			}
			leftBracket = lexer.Consume();

			PBOConfigClass rootScope = new PBOConfigClass(identifier.Token);

			ConfigToken nextToken;
			while (true)
			{
				nextToken = lexer.Peek();
				if (nextToken.TokenType == ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight || nextToken.TokenType == ConfigToken.ConfigTokenType.EndOfDocument)
					break;
				else if (nextToken.TokenType == ConfigToken.ConfigTokenType.Keyword_Class)
				{
					var result = ParseClass(lexer);
					rootScope.Expressions.Add(result.Result);
					errors = errors.Concat(result.Errors);
				}
				else if (nextToken.TokenType == ConfigToken.ConfigTokenType.Identifier)
				{
					var result = ParseExpression(lexer);
					rootScope.Expressions.Add(result.Result);
					errors = errors.Concat(result.Errors);
				}
				else
				{
					var error = new ParserErrorBase<ConfigParserErrors>(nextToken, ConfigParserErrors.UnexpectedToken);
					errors = errors.Append(error);
					lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Class, nextToken, false, classToken, identifier, leftBracket), error);
				}
			}

			ConfigToken rightBracket = lexer.Peek();
			if (rightBracket.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(rightBracket, ConfigParserErrors.ExpectedRightCurlyBracket);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Class, rightBracket, false, classToken, identifier, leftBracket), error);
			}
			rightBracket = lexer.Consume();

			nextToken = lexer.Peek();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Semicolumn)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(nextToken, ConfigParserErrors.ExpectedSemicolumn);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Class, nextToken, false, classToken, identifier, leftBracket, rightBracket), error);
			}
			nextToken = lexer.Consume();

			return new ParseResult<PBOConfigClass, ParserErrorBase<ConfigParserErrors>>(rootScope, true, errors);
		}
	}
}
