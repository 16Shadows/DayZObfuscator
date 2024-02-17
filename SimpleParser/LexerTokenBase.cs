namespace DayZObfuscatorModel.Parser
{
	public class LexerTokenBase
	{
		public LexerTokenBase(string token, int index, int line, int indexOnLine)
		{
			Token = token ?? throw new ArgumentNullException(nameof(token));
			TokenTrimmed = token.Trim();
			Index = index;
			Line = line;
			IndexOnLine = indexOnLine;
		}

		/// <summary>
		/// The token itself.
		/// </summary>
		public string Token { get; }
		/// <summary>
		/// Trimmed version of token.
		/// </summary>
		public string TokenTrimmed { get; }
		/// <summary>
		/// The index of the first character of this token in the document.
		/// </summary>
		public int Index { get; }
		/// <summary>
		/// The line at which the token is located in the document.
		/// </summary>
		public int Line { get; }
		/// <summary>
		/// The index of the first character of this token from the start of the line.
		/// </summary>
		public int IndexOnLine { get; }

		public override bool Equals(object? obj)
		{
			return obj is LexerTokenBase other && other.Token == Token && other.Line == Line && other.IndexOnLine == IndexOnLine && other.Index == Index;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Token, Index, Line, IndexOnLine);
		}
	}
}
