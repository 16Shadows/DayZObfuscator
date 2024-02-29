namespace DayZObfuscatorModel.Parser
{
	public class ConcatLexer<LexerToken> : ILexer<LexerToken> where LexerToken : LexerTokenBase
	{
		protected LexerToken[]? _Prepend { get; }
		protected int _PrependConsumedCount;

		protected LexerToken[]? _Append { get; }
		protected int _AppendConsumedCount;

		protected Func<LexerToken, bool>? _EndDetector { get; }
		protected Func<LexerToken>? _EndGenerator { get; }
		protected ILexer<LexerToken> _Lexer { get; }

		public ConcatLexer(ILexer<LexerToken> lexer, IEnumerable<LexerToken> prepend) : this(lexer, prepend ?? throw new ArgumentNullException(nameof(prepend)), null, null, null) {}
		public ConcatLexer(ILexer<LexerToken> lexer, IEnumerable<LexerToken> append, Func<LexerToken, bool> endDetector) : this(lexer, null, append ?? throw new ArgumentNullException(nameof(append)), endDetector ?? throw new ArgumentNullException(nameof(endDetector)), null) {}
		public ConcatLexer(ILexer<LexerToken> lexer, IEnumerable<LexerToken> append, Func<LexerToken, bool> endDetector, Func<LexerToken>? endGenerator) : this(lexer, null, append ?? throw new ArgumentNullException(nameof(append)), endDetector ?? throw new ArgumentNullException(nameof(endDetector)), endGenerator ?? throw new ArgumentNullException(nameof(endGenerator))) {}

		public ConcatLexer(ILexer<LexerToken> lexer, IEnumerable<LexerToken>? prepend, IEnumerable<LexerToken>? append, Func<LexerToken, bool>? endDetector, Func<LexerToken>? endGenerator)
		{
			_Lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
			_Prepend = prepend?.ToArray();
			_Append = append?.ToArray();
			
			if (endDetector != null)
				_EndDetector = t => !endDetector(t);
			
			_EndGenerator = endGenerator;
		}

		public LexerToken Peek()
		{
			if (_Prepend != null && _PrependConsumedCount < _Prepend.Length)
				return _Prepend[_PrependConsumedCount];
			else if (_EndDetector == null || !_EndDetector(_Lexer.Peek()))
				return _Lexer.Peek();
			else if (_Append != null && _AppendConsumedCount < _Append.Length)
				return _Append[_AppendConsumedCount];
			else if (_EndGenerator != null)
				return _EndGenerator();
			else
				return _Lexer.Peek();
		}

		public LexerToken Consume()
		{
			if (_Prepend != null && _PrependConsumedCount < _Prepend.Length)
				return _Prepend[_PrependConsumedCount++];
			else if (_EndDetector == null || !_EndDetector(_Lexer.Peek()))
				return _Lexer.Consume();
			else if (_Append != null && _AppendConsumedCount < _Append.Length)
				return _Append[_AppendConsumedCount++];
			else if (_EndGenerator != null)
				return _EndGenerator();
			else
				return _Lexer.Consume();
		}

		public IEnumerable<LexerToken> Peek(int count)
		{
			int countInitial = count;

			IEnumerable<LexerToken> tokens = Enumerable.Empty<LexerToken>();

			if (_Prepend != null && _PrependConsumedCount < _Prepend.Length)
			{
				int prepended = Math.Min(_Prepend.Length - _PrependConsumedCount, count);
				tokens = tokens.Concat( _Prepend.Take(prepended) );
				count -= prepended;
			}

			if (count <= 0)
				return tokens;
			else if (_EndDetector == null)
				return tokens.Concat( _Lexer.Peek(count) );

			LexerToken[] lexerSequence = _Lexer.Peek(count).TakeWhile(_EndDetector).ToArray();
			tokens = tokens.Concat( lexerSequence );
			count -= lexerSequence.Length;

			if (count < 0)
				return tokens;

			if (_Append != null && _AppendConsumedCount < _Append.Length)
			{
				int appended = Math.Min(_Append.Length - _AppendConsumedCount, count);
				tokens = tokens.Concat( _Append.Take(appended) );
				count -= appended;
			}

			if (count < 0)
				return tokens;
			else if (_EndGenerator == null)
				return tokens.Concat( _Lexer.Peek(countInitial).Skip(count) );
			else
				return tokens.Concat(Enumerable.Repeat(_EndGenerator(), count));
		}

		public IEnumerable<LexerToken> Consume(int count)
		{
			int countInitial = count;

			IEnumerable<LexerToken> tokens = Enumerable.Empty<LexerToken>();

			if (_Prepend != null && _PrependConsumedCount < _Prepend.Length)
			{
				int prepended = Math.Min(_Prepend.Length - _PrependConsumedCount, count);
				tokens = tokens.Concat( _Prepend.Take(prepended) );
				count -= prepended;
				_PrependConsumedCount += prepended;
			}

			if (count < 0)
				return tokens;
			else if (_EndDetector == null)
				return tokens.Concat( _Lexer.Consume(count) );

			LexerToken[] lexerSequence = _Lexer.Consume(count).TakeWhile(_EndDetector).ToArray();
			tokens = tokens.Concat( lexerSequence );
			count -= lexerSequence.Length;

			if (count < 0)
				return tokens;

			if (_Append != null && _AppendConsumedCount < _Append.Length)
			{
				int appended = Math.Min(_Append.Length - _AppendConsumedCount, count);
				tokens = tokens.Concat( _Append.Take(appended) );
				count -= appended;
				_AppendConsumedCount += appended;
			}

			if (count < 0)
				return tokens;
			else if (_EndGenerator == null)
				return tokens.Concat( _Lexer.Consume(count) );
			else
				return tokens.Concat(Enumerable.Repeat(_EndGenerator(), count));
		}
	}
}
