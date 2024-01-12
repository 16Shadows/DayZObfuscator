using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.Parser
{
	public static class ILexerExtensions
	{
		public static ILexer<LexerToken> Prepend<LexerToken>(this ILexer<LexerToken> lexer, IEnumerable<LexerToken> sequence) where LexerToken : LexerTokenBase => new ConcatLexer<LexerToken>(lexer, sequence);
		
		public static ILexer<LexerToken> Prepend<LexerToken>(this ILexer<LexerToken> lexer, LexerToken item) where LexerToken : LexerTokenBase => new PrependLexer<LexerToken>(lexer, item);
		
		public static ILexer<LexerToken> Append<LexerToken>(this ILexer<LexerToken> lexer, IEnumerable<LexerToken> sequence, Func<LexerToken, bool> endDetector) where LexerToken : LexerTokenBase => new ConcatLexer<LexerToken>(lexer, sequence, endDetector);
		
		public static ILexer<LexerToken> Append<LexerToken>(this ILexer<LexerToken> lexer, IEnumerable<LexerToken> sequence, Func<LexerToken, bool> endDetector, Func<LexerToken> endGenerator) where LexerToken : LexerTokenBase => new ConcatLexer<LexerToken>(lexer, sequence, endDetector, endGenerator);
		
		public static ILexer<LexerToken> AsPreview<LexerToken>(this ILexer<LexerToken> lexer) where LexerToken : LexerTokenBase => new PreviewLexer<LexerToken>(lexer);
	}
}
