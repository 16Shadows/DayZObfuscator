namespace DayZObfuscatorModel.Parser
{
	public class StringInputReader : InputReaderBase
	{
		private int _ConsumedChars;
		private readonly string _Input;

		public StringInputReader(string input)
		{
			_Input = input;
			_ConsumedChars = 0;
		}

		override public char Consume() => _ConsumedChars < _Input.Length ? _Input[_ConsumedChars++] : '\0';

		override public IEnumerable<char> Consume(int count)
		{
			if (count < 1)
				throw new ArgumentOutOfRangeException(nameof(count));

			int consumed = _ConsumedChars;
			int available = _Input.Length - consumed;

			_ConsumedChars += count;

			return available < count ?
					_Input.Skip(consumed).Take(available).Union(Enumerable.Repeat('\0', count - available)) :
					_Input.Skip(consumed).Take(count);
		}

		override public char Peek() => _ConsumedChars < _Input.Length ? _Input[_ConsumedChars] : '\0';

		override public IEnumerable<char> Peek(int count)
		{
			if (count < 1)
				throw new ArgumentOutOfRangeException(nameof(count));

			int available = _Input.Length - _ConsumedChars;

			return available < count ?
					_Input.Skip(_ConsumedChars).Take(available).Union(Enumerable.Repeat('\0', count - available)) :
					_Input.Skip(_ConsumedChars).Take(count);
		}
	}
}
