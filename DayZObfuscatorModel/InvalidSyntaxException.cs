namespace DayZObfuscatorModel
{
    public class InvalidSyntaxException : Exception
    {
        public int Index { get; }
        public int Line { get; }
        public int LineIndex { get; }

        public InvalidSyntaxException(string message, int index, int line, int lineIndex) : base($"{line}:{lineIndex}: {message}") {}
    }
}
