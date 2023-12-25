namespace DayZObfuscatorModel.Parser
{
	public interface ILexer<LexerToken> where LexerToken : LexerTokenBase
	{
		public abstract LexerToken Peek();
		public abstract LexerToken Consume();
		public abstract IEnumerable<LexerToken> Peek(int count);
		public abstract IEnumerable<LexerToken> Consume(int count);
	}
}
