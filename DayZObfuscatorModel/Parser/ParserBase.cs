namespace DayZObfuscatorModel.Parser
{
	public abstract class ParserBase<LexerToken, TokenFamily> where TokenFamily : ParserTokenBase where LexerToken : LexerTokenBase
	{
		protected readonly LexerBase<LexerToken> _Lexer;

		public ParserBase(LexerBase<LexerToken> lexer)
		{
			_Lexer = lexer;
		}

		public abstract TokenFamily Parse();
	}
}
