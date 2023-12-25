namespace DayZObfuscatorModel.Parser
{
	public interface IInputReader
	{
		public char Peek();
		public char Consume();
		public string Peek(int count);
		public string Consume(int count);
	}
}
