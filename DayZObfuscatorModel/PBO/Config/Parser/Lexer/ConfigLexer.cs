using DayZObfuscatorModel.Parser;
using System.Globalization;
using System.Text;

namespace DayZObfuscatorModel.PBO.Config.Parser.Lexer
{
    public class ConfigLexer : ILexer<ConfigToken>
    {
        protected readonly IInputReader _Document;
        protected readonly DynamicRingBuffer<ConfigToken> _ParsedTokens = new DynamicRingBuffer<ConfigToken>(10);
        protected readonly StringBuilder _TokenBuffer = new StringBuilder(50);
        protected int _IndexCounter = -1;
        protected int _LineCounter = 0;
        protected int _IndexOnLineCounter = -1;

        public ConfigLexer(IInputReader document)
        {
            _Document = document;
        }

        public ConfigToken Consume()
        {
            return _ParsedTokens.Count == 0 && Parse(1) == 0 ?
                   GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.EndOfDocument) :
                   _ParsedTokens.Pop();
        }

        public IEnumerable<ConfigToken> Consume(int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count));

            int missingTokens = count - _ParsedTokens.Count;

            return _ParsedTokens.Count < count && Parse(missingTokens) < missingTokens ?
                   _ParsedTokens.Pop(_ParsedTokens.Count).Concat(Enumerable.Repeat(GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.EndOfDocument), count - _ParsedTokens.Count)) :
                   _ParsedTokens.Pop(count);
        }

        public ConfigToken Peek()
        {
            return _ParsedTokens.Count == 0 && Parse(1) == 0 ?
                   GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.EndOfDocument) :
                   _ParsedTokens[0];
        }

        public IEnumerable<ConfigToken> Peek(int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count));

            int missingTokens = count - _ParsedTokens.Count;

            return _ParsedTokens.Count < count && Parse(missingTokens) < missingTokens ?
                   _ParsedTokens.Take(_ParsedTokens.Count).Concat(Enumerable.Repeat(GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.EndOfDocument), count - _ParsedTokens.Count)) :
                   _ParsedTokens.Take(count);
        }

        /// <summary>
        /// Parses next <paramref name="count"/> tokens from document.
        /// </summary>
        /// <param name="count"></param>
        protected int Parse(int count)
        {
            const string _ValidIdentifierSymbol = "_abcdefghijklmnopqrstuvwxyz1234567890[]";

            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count)); 

            _TokenBuffer.Clear();

            int successfulTokens = 0;
            char symbol;
            while (successfulTokens < count)
            {
                symbol = _Document.Peek();

                if (symbol == '\0')
                    break;
                else if (char.IsWhiteSpace(symbol))
                {
                    _Document.Consume();
                    AdvanceIndex();

                    if (IsLineBreak(symbol))
                        AdvanceLine();

                    continue;
                }
                else if (_Document.Peek(5) == "class")
                {
                    AdvanceIndex(5);
                    _TokenBuffer.Append(_Document.Consume(5));
                    _ParsedTokens.Add(GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.Keyword_Class));
                }
                else if (symbol == '{')
                {
                    AdvanceIndex();
                    _TokenBuffer.Append(_Document.Consume());
                    _ParsedTokens.Add(GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft));
                }
                else if (symbol == '}')
                {
                    AdvanceIndex();
                    _TokenBuffer.Append(_Document.Consume());
                    _ParsedTokens.Add(GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight));
                }
                else if (symbol == '=')
                {
                    AdvanceIndex();
                    _TokenBuffer.Append(_Document.Consume());
                    _ParsedTokens.Add(GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.Symbol_Assign));
                }
                else if (_Document.Peek(2) == "+=")
                {
                    AdvanceIndex(2);
                    _TokenBuffer.Append(_Document.Consume(2));
                    _ParsedTokens.Add(GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.Symbol_PlusAssign));
                }
                else if (_Document.Peek(2) == "-=")
                {
                    AdvanceIndex(2);
                    _TokenBuffer.Append(_Document.Consume(2));
                    _ParsedTokens.Add(GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.Symbol_MinusAssign));
                }
                else if (symbol == ',')
                {
                    AdvanceIndex();
                    _TokenBuffer.Append(_Document.Consume());
                    _ParsedTokens.Add(GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.Symbol_Comma));
                }
                else if (symbol == ';')
                {
                    AdvanceIndex();
                    _TokenBuffer.Append(_Document.Consume());
                    _ParsedTokens.Add(GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.Symbol_Semicolumn));
                }
                else if (symbol == '\"')
                {
                    AdvanceIndex();
                    _TokenBuffer.Append(_Document.Consume());

                    while (true)
                    {
                        symbol = _Document.Consume();
                        if (symbol == '\0')
                        {
                            _ParsedTokens.Add(GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.BrokenString));
                            break;
                        }
                        
                        if (IsLineBreak(symbol))
                        {
                            _ParsedTokens.Add(GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.BrokenString));
                            AdvanceIndex();
                            AdvanceLine();
                            break;
                        }
                        
                        AdvanceIndex();
                        _TokenBuffer.Append(symbol);
                        
                        if (symbol == '\"' &&  (_TokenBuffer.Length < 2 || _TokenBuffer[^1] != '\\'))
                        {
                            _ParsedTokens.Add(GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.String));
                            break;
                        }
                    }
                }
                else if (char.IsNumber(symbol) || symbol == '-')
                {
                    AdvanceIndex();
                    _TokenBuffer.Append(_Document.Consume());

                    while( (symbol = _Document.Peek()) != '\0' && (char.IsNumber(symbol) || symbol == '.') )
                    {
                        _TokenBuffer.Append(_Document.Consume());
                        AdvanceIndex();
                    }

                    _ParsedTokens.Add(GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.Number));
                }
                else
                {
                    AdvanceIndex();
                    _TokenBuffer.Append(_Document.Consume());

                    while( (symbol = _Document.Peek()) != '\0' && _ValidIdentifierSymbol.Contains(char.ToLower(symbol)) )
                    {
                        _TokenBuffer.Append(_Document.Consume());
                        AdvanceIndex();
                    }

                    _ParsedTokens.Add(GenerateTokenFromBuffer(ConfigToken.ConfigTokenType.Identifier));
                }

                successfulTokens++;
            }

            return successfulTokens;
        }

        protected ConfigToken GenerateTokenFromBuffer(ConfigToken.ConfigTokenType type)
        {
            string token_str = _TokenBuffer.ToString();
            _TokenBuffer.Clear();
            return new ConfigToken(type, token_str, _IndexCounter - token_str.Length + 1, _LineCounter, _IndexOnLineCounter - token_str.Length + 1);
        }

        protected static bool IsLineBreak(char symbol) => symbol == '\n' || CharUnicodeInfo.GetUnicodeCategory(symbol) == UnicodeCategory.LineSeparator || CharUnicodeInfo.GetUnicodeCategory(symbol) == UnicodeCategory.ParagraphSeparator;

        protected void AdvanceIndex(int count = 1)
        {
            _IndexCounter += count;
            _IndexOnLineCounter += count;
        }

        protected void AdvanceLine()
        {
            _LineCounter++;
            _IndexOnLineCounter = -1;
        }
    }
}
