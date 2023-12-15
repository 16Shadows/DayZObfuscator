using System.Collections;
using System.Globalization;
using System.Text;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigScope : IEnumerable<KeyValuePair<string, object>>
	{
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => Variables.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public IDictionary<string, object> Variables { get; } = new Dictionary<string, object>();

		public IEnumerable<KeyValuePair<string, PBOConfigScope>> Scopes => Variables.Where(x => x.Value is PBOConfigScope).Select(x => new KeyValuePair<string, PBOConfigScope>(x.Key, (PBOConfigScope)x.Value) );

		public object this[string name]
		{
			get => Variables[name ?? throw new ArgumentNullException(nameof(name))];
			set => Variables[name ?? throw new ArgumentNullException(nameof(name))] = value ?? throw new ArgumentNullException(nameof(value));
		}

		public T GetVariable<T>(string name) => (T)this[name];

		public PBOConfigScope GetScope(string name) => GetVariable<PBOConfigScope>(name);

		protected const string _TokenSymbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890_";
		protected const string _ArrayNameSymbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890_[]";
		protected const string _NumberSymbols = "-1234567890.";

		protected enum TokenType
		{
			None,
			ClassKeyword,
			Variable,
			Array,
			ExpressionSign
		}

		protected static PBOConfigScope ParseScope(ReadOnlySpan<char> text, ref int index, ref int line, ref int lineIndex)
		{
			SkipWhitespaces(text, ref index, ref line, ref lineIndex);

			if (text[index] != '{')
				throw new InvalidSyntaxException($"Expected '{{', found '{text[index]}'", index, line, lineIndex);
			
			PBOConfigScope scope = new PBOConfigScope();

			index++;
			lineIndex++;

			bool isArray = false;
			string token,
				   varName = "";
			TokenType currentToken = TokenType.None;

			while (index < text.Length)
			{
				//Skip till next token
				SkipWhitespaces(text, ref index, ref line, ref lineIndex);

				if (text[index] == '}')
				{
					if (currentToken == TokenType.None)
					{
						index++;
						lineIndex++;
						return scope;
					}
					else if (currentToken == TokenType.ClassKeyword)
						throw new InvalidSyntaxException("Expected class name, found '}'.", index, line, lineIndex);
					else if (currentToken == TokenType.Variable || currentToken == TokenType.Array)
						throw new InvalidSyntaxException("Expected '=', found '}'.", index, line, lineIndex);
					else if (currentToken == TokenType.ExpressionSign)
						throw new InvalidSyntaxException("Expected expression value, found '}'.", index, line, lineIndex);
				}

				switch (currentToken)
				{
					case TokenType.None:
					{
						token = ParseToken(text, ref index, ref line, ref lineIndex, _ArrayNameSymbols);
						if (token.ToLower() == "class")
							currentToken = TokenType.ClassKeyword;
						else if (token.EndsWith("[]"))
						{
							token = token[..^2];

							if (!ValidateToken(token.AsSpan(), _TokenSymbols))
								throw new InvalidSyntaxException($"Invalid variable name '{token}[]'.", index - token.Length - 2, line, lineIndex - token.Length - 2);

							isArray = true;
							varName = token;
							currentToken = TokenType.Array;
						}
						else
						{
							isArray = false;
							varName = token;
							currentToken = TokenType.Variable;
						}
						break;
					}
					case TokenType.ClassKeyword:
					{
						token = ParseToken(text, ref index, ref line, ref lineIndex);
						if (token.Length == 0)
							throw new InvalidSyntaxException($"Expected class name, found '{text[index]}'.", index, line, lineIndex);
						scope[token] = ParseScope(text, ref index, ref line, ref lineIndex);
						currentToken = TokenType.None;
						break;
					}
					case TokenType.Array:
					case TokenType.Variable:
					{
						token = ParseToken(text, ref index, ref line, ref lineIndex, "=");
						if (token != "=")
							throw new InvalidSyntaxException($"Expected '=', found '{token}'.", index - token.Length, line, lineIndex - token.Length);
						currentToken = TokenType.ExpressionSign;
						break;
					}
					case TokenType.ExpressionSign:
					{
						if (isArray)
							scope[varName] = ParseArray(text, ref index, ref line, ref lineIndex);
						else
							scope[varName] = ParseValue(text, ref index, ref line, ref lineIndex);
						currentToken = TokenType.None;
						break;
					}
				}
			}

			if (currentToken == TokenType.ClassKeyword)
				throw new InvalidSyntaxException("Expected class name, found 'end of string'.", index, line, lineIndex);
			else if (currentToken == TokenType.Variable || currentToken == TokenType.Array)
				throw new InvalidSyntaxException("Expected '=', found 'end of string'.", index, line, lineIndex);
			else if (currentToken == TokenType.ExpressionSign)
				throw new InvalidSyntaxException("Expected expression value, found 'end of string'.", index, line, lineIndex);

			return scope;
		}

		protected static object ParseValue(ReadOnlySpan<char> text, ref int index, ref int line, ref int lineIndex)
		{
			//String
			if (text[index] == '\"')
			{
				index++;
				lineIndex++;
				string strValue = "";
				bool escapedSymbol = false;
				for ( ; index < text.Length; index++, lineIndex++)
				{
					if (text[index] == '\n')
						throw new InvalidSyntaxException($"Expected '\"', found new line.", index, line, lineIndex);
					else if (text[index] == '\"' && !escapedSymbol)
					{
						index++;
						lineIndex++;
						return strValue;
					}
					
					escapedSymbol = text[index] == '\\' && !escapedSymbol;
					strValue += text[index];
				}
				throw new InvalidSyntaxException($"Expected '\"', found string end.", index, line, lineIndex);
			}
			//Number
			else if (_NumberSymbols.Contains(text[index]))
			{
				string strValue = "";
				for ( ; index < text.Length && _NumberSymbols.Contains(text[index]); index++, lineIndex++)
					strValue += text[index];

				if (int.TryParse(strValue, CultureInfo.InvariantCulture, out int intValue))
					return intValue;
				else if (float.TryParse(strValue, CultureInfo.InvariantCulture,out float floatValue))
					return floatValue;
				else
					throw new InvalidSyntaxException($"Expected number, found '{strValue}'.", index - strValue.Length, line, lineIndex - strValue.Length);
			}
			else
				throw new InvalidSyntaxException($"Expected string or number, found '{text[index]}'.", index, line, lineIndex);
		}

		protected static object ParseArray(ReadOnlySpan<char> text, ref int index, ref int line, ref int lineIndex)
		{
			if (text[index] != '{')
				throw new InvalidSyntaxException($"Expected array, found '{text[index]}'.", index, line, lineIndex);

			index++;
			lineIndex++;

			List<object> array = new List<object>();

			bool expectingValue = true;
			for (; index < text.Length; index++)
			{
				SkipWhitespaces(text, ref index, ref line, ref lineIndex);

				if (expectingValue)
				{
					array.Add(ParseValue(text, ref index, ref line, ref lineIndex));
					expectingValue = false;
				}
				else if (text[index] == ',')
					expectingValue = true;
				else if (text[index] == '}')
					return array;
			}
			if (expectingValue)
				throw new InvalidSyntaxException("Expected value, found end of string", index, line, lineIndex);
			else
				throw new InvalidSyntaxException("Expected '}', found end of string", index, line, lineIndex);
		}

		protected static void SkipWhitespaces(ReadOnlySpan<char> text, ref int index, ref int line, ref int lineIndex)
		{
			for (; index < text.Length; index++)
			{
				lineIndex++;
				if (text[index] == ' ' || text[index] == '\t' || text[index] == ';')
					continue;
				else if (text[index] == '\n')
				{
					line++;
					lineIndex = -1;
					continue;
				}
				else
					break;
			}
		}

		protected static string ParseToken(ReadOnlySpan<char> text, ref int index, ref int line, ref int lineIndex, string validSymbols = _TokenSymbols)
		{
			string token = "";
			for (; index < text.Length; index++)
			{
				lineIndex++;
				if (text[index] == ' ' || text[index] == '\t' || !validSymbols.Contains(text[index]))
					return token;
				else if (text[index] == '\n')
				{
					lineIndex = -1;
					line++;
					return token;
				}
				token += text[index];
			}
			return token;
		}

		protected static bool ValidateToken(ReadOnlySpan<char> token, string validSymbols)
		{
			foreach (char c in token)
				if (!validSymbols.Contains(c))
					return false;

			return true;
		}
	}
}
