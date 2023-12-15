namespace DayZObfuscatorModel.Parser
{
    public abstract class ParserTokenBase
    {
        public LexerTokenBase LexerToken { get; }

        public ParserTokenBase(LexerTokenBase lexerToken)
        {
            LexerToken = lexerToken ?? throw new ArgumentNullException(nameof(lexerToken));
        }
    }
}
