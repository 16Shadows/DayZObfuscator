using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.PBO.Config
{
    public class PBOConfig : PBOConfigScope
    {
        public static PBOConfig Parse(ReadOnlySpan<char> text)
        {
            PBOConfig config = new PBOConfig();
            int index = 0, line = 0, lineIndex = 0;

			string token;
            TokenType currentToken = TokenType.None;

            for (; index < text.Length; index++)
            {
                SkipWhitespaces(text, ref index, ref line, ref lineIndex);

                switch (currentToken)
				{
					case TokenType.None:
					{
						token = ParseToken(text, ref index, ref line, ref lineIndex);
						if (token.ToLower() == "class")
							currentToken = TokenType.ClassKeyword;
						else
							throw new InvalidSyntaxException($"Expected 'class', found '{token}'.", index - token.Length, line, lineIndex - token.Length);
						break;
					}
					case TokenType.ClassKeyword:
					{
						token = ParseToken(text, ref index, ref line, ref lineIndex);
						if (token.Length == 0)
							throw new InvalidSyntaxException($"Expected class name, found '{text[index]}'.", index, line, lineIndex);
						config[token] = ParseScope(text, ref index, ref line, ref lineIndex);
						currentToken = TokenType.None;
						break;
					}
				}
            }

            return config;
        }
    }
}
