namespace DayZObfuscatorModel.Parser
{
	public abstract class LexerBase
	{
		protected readonly InputReaderBase _Document;

		public LexerBase(InputReaderBase document)
		{
			_Document = document ?? throw new ArgumentNullException(nameof(document));
		}

		public abstract LexerToken Peek();
		public abstract LexerToken Consume();
		public abstract IEnumerable<LexerToken> Peek(int count);
		public abstract IEnumerable<LexerToken> Consume(int count);
	}
}
