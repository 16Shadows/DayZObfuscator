using DayZObfuscatorModel.PBO.Config.Parser.Lexer;

namespace DayZObfuscatorModel.Parser
{
	public interface IParser<LexerToken, ParserResult> where LexerToken : LexerTokenBase
	{
		public abstract ParserResult Parse(ILexer<LexerToken> lexer);
	}
}
