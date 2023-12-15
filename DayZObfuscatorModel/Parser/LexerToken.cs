namespace DayZObfuscatorModel.Parser
{
	public class LexerToken
	{
		public LexerToken(string token, int index, int line, int indexOnLine)
		{
			Token = token ?? throw new ArgumentNullException(nameof(token));
			Index = index;
			Line = line;
			IndexOnLine = indexOnLine;
		}

		/// <summary>
		/// The token itself.
		/// </summary>
		public string Token { get; }
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
	}
}
