namespace DayZObfuscatorModel.Parser
{
	public class StringInputReader : IInputReader
	{
		private int _ConsumedChars;
		private readonly string _Input;

		public StringInputReader(string input)
		{
			_Input = input;
			_ConsumedChars = 0;
		}

		public char Consume() => _ConsumedChars < _Input.Length ? _Input[_ConsumedChars++] : '\0';

		public string Consume(int count)
		{
			if (count < 1)
				throw new ArgumentOutOfRangeException(nameof(count));

			int consumed = _ConsumedChars;
			int available = _Input.Length - consumed;

			_ConsumedChars += count;

			return available < count ?
					string.Concat(_Input.AsSpan(consumed, available), string.Join("", Enumerable.Repeat('\0', count - available))) :
					_Input.Substring(consumed, count);
		}

		public char Peek() => _ConsumedChars < _Input.Length ? _Input[_ConsumedChars] : '\0';

		public string Peek(int count)
		{
			if (count < 1)
				throw new ArgumentOutOfRangeException(nameof(count));

			int available = _Input.Length - _ConsumedChars;

			return available < count ?
					string.Concat(_Input.AsSpan(_ConsumedChars, available), string.Join("", Enumerable.Repeat('\0', count - available))) :
					_Input.Substring(_ConsumedChars, count);
		}

		public static implicit operator StringInputReader(string text)
		{
			return new StringInputReader(text);
		}
	}
}
