namespace DayZObfuscatorModel.Parser
{
	public abstract class InputReaderBase
	{
		public abstract char Peek();
		public abstract char Consume();
		public abstract string Peek(int count);
		public abstract string Consume(int count);
	}
}
