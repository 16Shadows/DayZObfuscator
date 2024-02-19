using DayZObfuscatorModel.Parser;

namespace DayZObfuscatorModel.PBO.Config.Parser.Lexer
{
	public class ConfigToken : LexerTokenBase
	{
		public enum ConfigTokenType
		{
			Unknown,

			Keyword_Class,
			Keyword_Delete,

			Symbol_CurlyBracketLeft,
			Symbol_CurlyBracketRight,
			Symbol_Assign,
			Symbol_PlusAssign,
			Symbol_MinusAssign,
			Symbol_Comma,
			Symbol_Semicolumn,
			Symbol_Column,

			Identifier,
			String,
			BrokenString,
			Number,

			EndOfDocument
		}

		public ConfigTokenType TokenType { get; }

		public ConfigToken(ConfigTokenType type, string token, int index, int line, int indexOnLine) : base(token, index, line, indexOnLine)
		{
			TokenType = type;
		}

		public override bool Equals(object? obj)
		{
			return obj is ConfigToken other && other.TokenType == TokenType && base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(base.GetHashCode(), TokenType);
		}
	}
}
