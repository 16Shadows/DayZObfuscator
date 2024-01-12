using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.Parser
{
	internal class PreviewLexer<LexerToken> : ILexer<LexerToken> where LexerToken : LexerTokenBase
	{
		protected readonly ILexer<LexerToken> _Lexer;
		protected int _ConsumedCount;

		public PreviewLexer(ILexer<LexerToken> lexer)
		{
			_Lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
		}

		public LexerToken Consume()
		{
			LexerToken token = _ConsumedCount > 0 ? _Lexer.Peek(_ConsumedCount + 1).Skip(_ConsumedCount).First() : _Lexer.Peek();

			_ConsumedCount++;

			return token;
		}

		public IEnumerable<LexerToken> Consume(int count)
		{
			if (count < 1)
				throw new ArgumentOutOfRangeException(nameof(count));

			IEnumerable<LexerToken> tokens = _ConsumedCount > 0 ? _Lexer.Peek(_ConsumedCount + count).Skip(_ConsumedCount) : _Lexer.Peek(count);

			_ConsumedCount += count;

			return tokens;
		}

		public LexerToken Peek()
		{
			return _ConsumedCount > 0 ? _Lexer.Peek(_ConsumedCount + 1).Skip(_ConsumedCount).First() : _Lexer.Peek();
		}

		public IEnumerable<LexerToken> Peek(int count)
		{
			if (count < 1)
				throw new ArgumentOutOfRangeException(nameof(count));

			return _ConsumedCount > 0 ? _Lexer.Peek(_ConsumedCount + count).Skip(_ConsumedCount) : _Lexer.Peek(count);
		}
	}
}
