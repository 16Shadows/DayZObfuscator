namespace DayZObfuscatorModel.Parser
{
	public abstract class ParserBase<TokenFamily> where TokenFamily : ParserTokenBase
	{
		protected readonly LexerBase _Lexer;

		public ParserBase(LexerBase lexer)
		{
			_Lexer = lexer;
		}

		public abstract TokenFamily Parse();
	}
}
