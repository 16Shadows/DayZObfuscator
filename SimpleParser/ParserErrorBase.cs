namespace DayZObfuscatorModel.Parser
{
	public class ParserErrorBase<MessageType, LexerToken> where LexerToken : LexerTokenBase
	{
		public ParserErrorBase(LexerToken erroneousToken, MessageType message)
		{
			ErroneousToken = erroneousToken ?? throw new ArgumentNullException(nameof(erroneousToken));
			Message = message ?? throw new ArgumentNullException(nameof(message));
		}

		public LexerToken ErroneousToken { get; }
		public MessageType Message { get; }
	}
}
