namespace DayZObfuscatorModel.Parser
{
	public abstract class InputReaderBase
	{
		public abstract char Peek();
		public abstract char Consume();
		public abstract IEnumerable<char> Peek(int count);
		public abstract IEnumerable<char> Consume(int count);
	}
}
