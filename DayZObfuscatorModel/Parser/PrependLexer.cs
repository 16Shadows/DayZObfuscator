using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.Parser
{
	public class PrependLexer<LexerToken> : ILexer<LexerToken> where LexerToken : LexerTokenBase
	{
		protected readonly ILexer<LexerToken> _Lexer;
		protected readonly LexerToken _Prepend;
		protected bool _Consumed;

		public PrependLexer(ILexer<LexerToken> lexer, LexerToken prepend)
		{
			_Lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
			_Prepend = prepend ?? throw new ArgumentNullException(nameof(prepend));
			_Consumed = false;
		}

		public LexerToken Consume()
		{
			if (_Consumed)
				return _Lexer.Consume();
			else
			{
				_Consumed = true;
				return _Prepend;
			}
		}

		public IEnumerable<LexerToken> Consume(int count)
		{
			if (_Consumed)
				return _Lexer.Consume(count);
			else
			{
				_Consumed = true;
				return count > 1 ? _Lexer.Consume(count - 1).Prepend(_Prepend) : Enumerable.Repeat(_Prepend, 1);
			}
		}

		public LexerToken Peek()
		{
			if (_Consumed)
				return _Lexer.Peek();
			else
				return _Prepend;
		}

		public IEnumerable<LexerToken> Peek(int count)
		{
			if (_Consumed)
				return _Lexer.Peek(count);
			else
				return _Lexer.Peek(count - 1).Prepend(_Prepend);
		}
	}
}
