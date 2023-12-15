namespace DayZObfuscatorModel.Parser
{
    public abstract class ParserTokenBase
    {
        public LexerToken LexerToken { get; }

        public ParserTokenBase(LexerToken lexerToken)
        {
            LexerToken = lexerToken ?? throw new ArgumentNullException(nameof(lexerToken));
        }
    }
}
