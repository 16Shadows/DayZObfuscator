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
		ExpectedComma,
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

			return new ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors>>(rootScope, true, errors);
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

			ConfigToken identifier = lexer.Consume();
			if (identifier.TokenType != ConfigToken.ConfigTokenType.Identifier || !identifier.Token.EndsWith("[]"))
			{
				var error = new ParserErrorBase<ConfigParserErrors>(identifier, ConfigParserErrors.ExpectedArrayIdentifier);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.ArrayExpression, identifier, true, identifier), error);
				identifier = lexer.Consume();
			}

			ConfigToken expressionToken = lexer.Consume();
			
			PBOConfigArrayExpressionBase? expr = null;
			
			if (expressionToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Assign && expressionToken.TokenType != ConfigToken.ConfigTokenType.Symbol_PlusAssign && expressionToken.TokenType != ConfigToken.ConfigTokenType.Symbol_MinusAssign)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(identifier, ConfigParserErrors.ExpectedOperator);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.ArrayExpression, expressionToken, true, identifier, expressionToken), error);
				expressionToken = lexer.Consume();
			}

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

			ConfigToken nextToken = lexer.Consume();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Semicolumn)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(identifier, ConfigParserErrors.ExpectedSemicolumn);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.ArrayExpression, nextToken, true, identifier, expressionToken, nextToken), error);
				nextToken = lexer.Consume();
			}

			return new ParseResult<PBOConfigArrayExpressionBase, ParserErrorBase<ConfigParserErrors>>(expr, true, errors);
		}

		protected ParseResult<PBOConfigExpressionBase, ParserErrorBase<ConfigParserErrors>> ParseVariableExpression(ILexer<ConfigToken> lexer)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors>>();

			ConfigToken identifier = lexer.Consume();
			if (identifier.TokenType != ConfigToken.ConfigTokenType.Identifier || identifier.Token.EndsWith("[]"))
			{
				var error = new ParserErrorBase<ConfigParserErrors>(identifier, ConfigParserErrors.ExpectedIdentifier);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.VariableExpression, identifier, true, identifier), error);
				identifier = lexer.Consume();
			}

			ConfigToken expressionToken = lexer.Consume();

			if ((expressionToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Assign))
			{
				var error = new ParserErrorBase<ConfigParserErrors>(identifier, ConfigParserErrors.ExpectedOperator);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.VariableExpression, expressionToken, true, identifier, expressionToken), error);
				expressionToken = lexer.Consume();
			}

			PBOConfigExpressionBase? expr = null;
			if (expressionToken.TokenType == ConfigToken.ConfigTokenType.Symbol_Assign)
			{
				var result = ParseValue(lexer);
				expr = new PBOConfigExpressionVariableAssignment(identifier.TokenTrimmed, result.Result);
				errors = errors.Concat(result.Errors);
			}
			else
				throw new InvalidSyntaxException($"Expected expression symbol, found '{identifier.Token}'", identifier.Index, identifier.Line, identifier.IndexOnLine);
			
			ConfigToken nextToken = lexer.Consume();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Semicolumn)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(identifier, ConfigParserErrors.ExpectedSemicolumn);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.VariableExpression, nextToken, true, identifier, expressionToken, nextToken), error);
				nextToken = lexer.Consume();
			}

			return new ParseResult<PBOConfigExpressionBase, ParserErrorBase<ConfigParserErrors>>(expr, true, errors);
		}

		protected ParseResult<object, ParserErrorBase<ConfigParserErrors>> ParseValue(ILexer<ConfigToken> lexer)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors>>();

			ConfigToken nextToken = lexer.Consume();
			if (nextToken.TokenType == ConfigToken.ConfigTokenType.Number)
			{
				if (int.TryParse(nextToken.Token, CultureInfo.InvariantCulture, out int intValue))
					return new ParseResult<object, ParserErrorBase<ConfigParserErrors>>(intValue, true, errors);
				else if (double.TryParse(nextToken.Token, CultureInfo.InvariantCulture, out double dValue))
					return new ParseResult<object, ParserErrorBase<ConfigParserErrors>>(dValue, true, errors);
				else
				{
					var error = new ParserErrorBase<ConfigParserErrors>(nextToken, ConfigParserErrors.InvalidNumber);
					errors = errors.Append(error);
					lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Value, nextToken, true, nextToken), error);
					return ParseValue(lexer).TransformErrors(e => errors.Concat(e));
				}
			}
			else if (nextToken.TokenType == ConfigToken.ConfigTokenType.String)
				return new ParseResult<object, ParserErrorBase<ConfigParserErrors>>(new PBOConfigValueString(nextToken.Token[1..^1]), true, errors);
			else if (nextToken.TokenType == ConfigToken.ConfigTokenType.BrokenString)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(nextToken, ConfigParserErrors.BrokenString);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Value, nextToken, true, nextToken), error);
				return ParseValue(lexer).TransformErrors(e => errors.Concat(e));
			}
			else
			{
				var error = new ParserErrorBase<ConfigParserErrors>(nextToken, ConfigParserErrors.UnexpectedToken);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Value, nextToken, true, nextToken), error);
				return ParseValue(lexer).TransformErrors(e => errors.Concat(e));
			}
		}

		protected ParseResult<IList<object>, ParserErrorBase<ConfigParserErrors>> ParseArray(ILexer<ConfigToken> lexer)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors>>();

			ConfigToken leftBracket = lexer.Consume();
			if (leftBracket.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(leftBracket, ConfigParserErrors.ExpectedSemicolumn);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Array, leftBracket, true, leftBracket), error);
				leftBracket = lexer.Consume();
			}

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
				if (nextToken.TokenType == ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight || nextToken.TokenType == ConfigToken.ConfigTokenType.EndOfDocument)
					break;
				
				nextToken = lexer.Consume();
				if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Comma)
				{
					var error = new ParserErrorBase<ConfigParserErrors>(leftBracket, ConfigParserErrors.ExpectedComma);
					errors = errors.Append(error);
					lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Array, nextToken, true, leftBracket, firstValue, nextToken), error);
					nextToken = lexer.Consume();
				}
				
				var result = ParseValue(lexer);
				arr.Add(result.Result);
				errors = errors.Concat(result.Errors);
			}

			nextToken = lexer.Consume();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(leftBracket, ConfigParserErrors.ExpectedRightCurlyBracket);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Array, nextToken, true, leftBracket, firstValue, nextToken), error);
				nextToken = lexer.Consume();
			}

			return new ParseResult<IList<object>, ParserErrorBase<ConfigParserErrors>>(arr, true, errors);
		}

		protected ParseResult<PBOConfigClass, ParserErrorBase<ConfigParserErrors>> ParseClass(ILexer<ConfigToken> lexer)
		{
			IEnumerable<ParserErrorBase<ConfigParserErrors>> errors = Enumerable.Empty<ParserErrorBase<ConfigParserErrors>>();

			ConfigToken classToken = lexer.Consume();
			if (classToken.TokenType != ConfigToken.ConfigTokenType.Keyword_Class)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(classToken, ConfigParserErrors.ExpectedClassKeyword);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Class, classToken, true, classToken), error);
				classToken = lexer.Consume();
			}

			ConfigToken identifier = lexer.Consume();
			if (identifier.TokenType != ConfigToken.ConfigTokenType.Identifier)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(identifier, ConfigParserErrors.ExpectedIdentifier);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Class, identifier, true, classToken, identifier), error);
				identifier = lexer.Consume();
			}

			ConfigToken leftBracket = lexer.Consume();
			if (leftBracket.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(identifier, ConfigParserErrors.ExpectedLeftCurlyBracket);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Class, leftBracket, true, classToken, identifier, leftBracket), error);
				leftBracket = lexer.Consume();
			}

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

			ConfigToken rightBracket = lexer.Consume();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(nextToken, ConfigParserErrors.ExpectedRightCurlyBracket);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Class, rightBracket, false, classToken, identifier, leftBracket, rightBracket), error);
				rightBracket = lexer.Consume();
			}

			nextToken = lexer.Consume();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Semicolumn)
			{
				var error = new ParserErrorBase<ConfigParserErrors>(nextToken, ConfigParserErrors.ExpectedSemicolumn);
				errors = errors.Append(error);
				lexer = _ErrorResolver.Resolve(lexer, this, new ParserState<ConfigToken, ConfigParserStates>(ConfigParserStates.Class, nextToken, false, classToken, identifier, leftBracket, rightBracket, nextToken), error);
				nextToken = lexer.Consume();
			}

			return new ParseResult<PBOConfigClass, ParserErrorBase<ConfigParserErrors>>(rootScope, true, errors);
		}
	}
}
