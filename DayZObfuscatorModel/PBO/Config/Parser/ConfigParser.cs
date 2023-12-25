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
	public class ConfigParser : IParser<ConfigToken, PBOConfig>
	{
		public PBOConfig Parse(ILexer<ConfigToken> lexer)
		{
			PBOConfig rootScope = new PBOConfig();

			ConfigToken nextToken;
			while (true)
			{
				nextToken = lexer.Peek();
				if (nextToken.TokenType == ConfigToken.ConfigTokenType.EndOfDocument)
					break;
				else if (nextToken.TokenType == ConfigToken.ConfigTokenType.Keyword_Class)
					rootScope.Expressions.Add(ParseClass(lexer));
				else if (nextToken.TokenType == ConfigToken.ConfigTokenType.Identifier)
					rootScope.Expressions.Add(ParseExpression(lexer));
				else
					throw new InvalidSyntaxException("Unexpected token", nextToken.Index, nextToken.Line, nextToken.IndexOnLine);
			}

			return rootScope;
		}

		protected PBOConfigExpressionBase ParseExpression(ILexer<ConfigToken> lexer)
		{
			ConfigToken identifier = lexer.Peek();
			
			if (identifier.Token.EndsWith("[]"))
				return ParseArrayExpression(lexer);
			else
				return ParseVariableExpression(lexer);
		}

		protected PBOConfigArrayExpressionBase ParseArrayExpression(ILexer<ConfigToken> lexer)
		{
			ConfigToken identifier = lexer.Consume();
			if (identifier.TokenType != ConfigToken.ConfigTokenType.Identifier || !identifier.Token.EndsWith("[]"))
				throw new InvalidSyntaxException($"Expected array identifier, found '{identifier.Token}'", identifier.Index, identifier.Line, identifier.IndexOnLine);

			ConfigToken expressionToken = lexer.Consume();
			if (expressionToken.TokenType == ConfigToken.ConfigTokenType.Symbol_Assign)
				return new PBOConfigArrayExpressionAssignment(identifier.TokenTrimmed, ParseArray(lexer));
			else if (expressionToken.TokenType == ConfigToken.ConfigTokenType.Symbol_PlusAssign)
				return new PBOConfigArrayExpressionUnion(identifier.TokenTrimmed, ParseArray(lexer));
			else if (expressionToken.TokenType == ConfigToken.ConfigTokenType.Symbol_MinusAssign)
				return new PBOConfigArrayExpressionDifference(identifier.TokenTrimmed, ParseArray(lexer));
			else
				throw new InvalidSyntaxException($"Expected expression symbol, found '{identifier.Token}'", identifier.Index, identifier.Line, identifier.IndexOnLine);
		}

		protected PBOConfigExpressionBase ParseVariableExpression(ILexer<ConfigToken> lexer)
		{
			ConfigToken identifier = lexer.Consume();
			if (identifier.TokenType != ConfigToken.ConfigTokenType.Identifier || !identifier.Token.EndsWith("[]"))
				throw new InvalidSyntaxException($"Expected array identifier, found '{identifier.Token}'", identifier.Index, identifier.Line, identifier.IndexOnLine);

			ConfigToken expressionToken = lexer.Consume();
			if (expressionToken.TokenType == ConfigToken.ConfigTokenType.Symbol_Assign)
				return new PBOConfigExpressionVariableAssignment(identifier.TokenTrimmed, ParseValue(lexer));
			else
				throw new InvalidSyntaxException($"Expected expression symbol, found '{identifier.Token}'", identifier.Index, identifier.Line, identifier.IndexOnLine);
		}

		protected object ParseValue(ILexer<ConfigToken> lexer)
		{
			ConfigToken nextToken = lexer.Consume();
			if (nextToken.TokenType == ConfigToken.ConfigTokenType.Number)
			{
				if (int.TryParse(nextToken.Token, CultureInfo.InvariantCulture, out int intValue))
					return intValue;
				else if (double.TryParse(nextToken.Token, CultureInfo.InvariantCulture, out double dValue))
					return dValue;
				else
					throw new InvalidSyntaxException($"Invalid number {nextToken.Token}", nextToken.Index, nextToken.Line, nextToken.IndexOnLine);
			}
			else if (nextToken.TokenType == ConfigToken.ConfigTokenType.String)
				return new PBOConfigValueString(nextToken.Token[1..^1]);
			else if (nextToken.TokenType == ConfigToken.ConfigTokenType.BrokenString)
				return new PBOConfigValueString(nextToken.Token[1..]);
			else
				throw new InvalidSyntaxException("Unexpected token", nextToken.Index, nextToken.Line, nextToken.IndexOnLine);
		}

		protected IList<object> ParseArray(ILexer<ConfigToken> lexer)
		{
			ConfigToken nextToken = lexer.Consume();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft)
				throw new InvalidSyntaxException($"Expected '{{', found '{nextToken.Token}'", nextToken.Index, nextToken.Line, nextToken.IndexOnLine);

			List<object> arr = new List<object>();

			while (true)
			{
				nextToken = lexer.Peek();
				if (nextToken.TokenType == ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight || nextToken.TokenType == ConfigToken.ConfigTokenType.EndOfDocument)
					break;
				
				arr.Add(ParseValue(lexer));

				nextToken = lexer.Peek();
				if (nextToken.TokenType == ConfigToken.ConfigTokenType.Symbol_Comma)
				{
					lexer.Consume();
					if (lexer.Peek().TokenType == ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight)
						throw new InvalidSyntaxException($"Expected value, found '}}'", nextToken.Index, nextToken.Line, nextToken.IndexOnLine);
				}
			}

			nextToken = lexer.Consume();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight)
				throw new InvalidSyntaxException($"Expected '}}', found '{nextToken.Token}'", nextToken.Index, nextToken.Line, nextToken.IndexOnLine);

			nextToken = lexer.Consume();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Semicolumn)
				throw new InvalidSyntaxException($"Expected ';', found '{nextToken.Token}'", nextToken.Index, nextToken.Line, nextToken.IndexOnLine);

			return arr;
		}

		protected PBOConfigClass ParseClass(ILexer<ConfigToken> lexer)
		{
			ConfigToken nextToken = lexer.Consume();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Keyword_Class)
				throw new InvalidSyntaxException($"Expected 'class', found '{nextToken.Token}'", nextToken.Index, nextToken.Line, nextToken.IndexOnLine);

			ConfigToken identifier = lexer.Consume();
			if (identifier.TokenType != ConfigToken.ConfigTokenType.Identifier)
				throw new InvalidSyntaxException($"Expected identifier, found '{identifier.Token}'", identifier.Index, identifier.Line, identifier.IndexOnLine);

			nextToken = lexer.Consume();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft)
				throw new InvalidSyntaxException($"Expected '{{', found '{nextToken.Token}'", nextToken.Index, nextToken.Line, nextToken.IndexOnLine);

			PBOConfigClass rootScope = new PBOConfigClass(identifier.Token);

			while (true)
			{
				nextToken = lexer.Peek();
				if (nextToken.TokenType == ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight || nextToken.TokenType == ConfigToken.ConfigTokenType.EndOfDocument)
					break;
				else if (nextToken.TokenType == ConfigToken.ConfigTokenType.Keyword_Class)
					rootScope.Expressions.Add(ParseClass(lexer));
				else if (nextToken.TokenType == ConfigToken.ConfigTokenType.Identifier)
					rootScope.Expressions.Add(ParseExpression(lexer));
				else
					throw new InvalidSyntaxException("Unexpected token", nextToken.Index, nextToken.Line, nextToken.IndexOnLine);
			}

			nextToken = lexer.Consume();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight)
				throw new InvalidSyntaxException($"Expected '}}', found '{nextToken.Token}'", nextToken.Index, nextToken.Line, nextToken.IndexOnLine);

			nextToken = lexer.Consume();
			if (nextToken.TokenType != ConfigToken.ConfigTokenType.Symbol_Semicolumn)
				throw new InvalidSyntaxException($"Expected ';', found '{nextToken.Token}'", nextToken.Index, nextToken.Line, nextToken.IndexOnLine);

			return rootScope;
		}
	}
}
