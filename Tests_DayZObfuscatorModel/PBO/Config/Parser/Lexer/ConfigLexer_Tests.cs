using Microsoft.VisualStudio.TestTools.UnitTesting;
using DayZObfuscatorModel.PBO.Config.Parser.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayZObfuscatorModel.Parser;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DayZObfuscatorModel.PBO.Config.Parser.Lexer.Tests
{
	[TestClass()]
	public class ConfigLexer_Tests
	{
		[TestMethod()]
		public void Consume_Test()
		{
			ConfigToken[] document_lexemes = new ConfigToken[]
			{
				new ConfigToken(ConfigToken.ConfigTokenType.Keyword_Class, "class", 0, 0, 0),
				new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "Test", 6, 0, 6),
				new ConfigToken(ConfigToken.ConfigTokenType.EndOfDocument, "", 10, 0, 10)
			};

			StringInputReader document = "class Test";
			LexerBase<ConfigToken> lexer = new ConfigLexer(document);

			//While the test can be implemented without the second array, having it allows as to properly look into what the lexer has produced when debugging.
			ConfigToken[] lexer_result = new ConfigToken[3];

			for (int i = 0; i < lexer_result.Length; i++)
				lexer_result[i] = lexer.Consume();

			for (int i = 0; i < lexer_result.Length; i++)
				Assert.AreEqual(document_lexemes[i], lexer_result[i]);
		}

		[TestMethod()]
		public void ConsumeN_Test()
		{
			ConfigToken[] document_lexemes = new ConfigToken[]
			{
				new ConfigToken(ConfigToken.ConfigTokenType.Keyword_Class, "class", 0, 0, 0),
				new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "Test", 6, 0, 6)
			};

			StringInputReader document = "class Test";
			LexerBase<ConfigToken> lexer = new ConfigLexer(document);

			document_lexemes.SequenceEqual(lexer.Consume(2));
		}

		[TestMethod()]
		public void ConsumeNPastEnd_Test()
		{
			ConfigToken[] document_lexemes = new ConfigToken[]
			{
				new ConfigToken(ConfigToken.ConfigTokenType.Keyword_Class, "class", 0, 0, 0),
				new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "Test", 6, 0, 6),
				new ConfigToken(ConfigToken.ConfigTokenType.EndOfDocument, "", 10, 0, 10)
			};

			StringInputReader document = "class Test";
			LexerBase<ConfigToken> lexer = new ConfigLexer(document);

			Assert.IsTrue(document_lexemes.SequenceEqual(lexer.Consume(5)));
		}

		[TestMethod()]
		public void ConsumeNInvalid_Test()
		{
			StringInputReader document = "class Test";
			LexerBase<ConfigToken> lexer = new ConfigLexer(document);

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => lexer.Consume(-5));
		}

		[TestMethod()]
		public void Peek_Test()
		{
			ConfigToken[] document_lexemes = new ConfigToken[]
			{
				new ConfigToken(ConfigToken.ConfigTokenType.Keyword_Class, "class", 0, 0, 0),
				new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "Test", 6, 0, 6),
				new ConfigToken(ConfigToken.ConfigTokenType.EndOfDocument, "", 10, 0, 10)
			};

			StringInputReader document = "class Test";
			LexerBase<ConfigToken> lexer = new ConfigLexer(document);

			Assert.AreEqual(document_lexemes[0], lexer.Peek());
			Assert.AreEqual(document_lexemes[0], lexer.Peek());
			lexer.Consume();
			Assert.AreEqual(document_lexemes[1], lexer.Peek());
			lexer.Consume();
			Assert.AreEqual(document_lexemes[2], lexer.Peek());
		}

		[TestMethod()]
		public void PeekN_Test()
		{
			ConfigToken[] document_lexemes = new ConfigToken[]
			{
				new ConfigToken(ConfigToken.ConfigTokenType.Keyword_Class, "class", 0, 0, 0),
				new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "Test", 6, 0, 6)
			};

			StringInputReader document = "class Test";
			LexerBase<ConfigToken> lexer = new ConfigLexer(document);

			Assert.IsTrue(document_lexemes.SequenceEqual(lexer.Peek(2)));
		}

		[TestMethod()]
		public void PeekNInvalid_Test()
		{
			StringInputReader document = "class Test";
			LexerBase<ConfigToken> lexer = new ConfigLexer(document);

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => lexer.Peek(-5));
		}

		[TestMethod()]
		public void PeekNPastEnd_Test()
		{
			ConfigToken[] document_lexemes = new ConfigToken[]
			{
				new ConfigToken(ConfigToken.ConfigTokenType.Keyword_Class, "class", 0, 0, 0),
				new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "Test", 6, 0, 6),
				new ConfigToken(ConfigToken.ConfigTokenType.EndOfDocument, "", 10, 0, 10)
			};

			StringInputReader document = "class Test";
			LexerBase<ConfigToken> lexer = new ConfigLexer(document);

			Assert.IsTrue(document_lexemes.SequenceEqual(lexer.Peek(5)));
		}

		[TestMethod()]
		public void FullTokenRange_Test()
		{
			ConfigToken[] document_lexemes = new ConfigToken[]
			{
				new ConfigToken(ConfigToken.ConfigTokenType.Keyword_Class, "class", 0, 0, 0),
				new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "Test", 6, 0, 6),
				new ConfigToken(ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft, "{", 11, 1, 0),
				new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "str", 14, 2, 1),
				new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Assign, "=", 18, 2, 5),
				new ConfigToken(ConfigToken.ConfigTokenType.String, "\"test text\"", 20, 2, 7),
				new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Semicolumn, ";", 31, 2, 18),
				new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "str_broken", 34, 3, 1),
				new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Assign, "=", 44, 3, 11),
				new ConfigToken(ConfigToken.ConfigTokenType.BrokenString, "\"broken", 45, 3, 12),
				new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "arr[]", 54, 4, 1),
				new ConfigToken(ConfigToken.ConfigTokenType.Symbol_PlusAssign, "+=", 60, 4, 7),
				new ConfigToken(ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft, "{", 63, 4, 10),
				new ConfigToken(ConfigToken.ConfigTokenType.Number, "2", 65, 4, 12),
				new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Comma, ",", 66, 4, 13),
				new ConfigToken(ConfigToken.ConfigTokenType.Number, "3", 68, 4, 15),
				new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Comma, ",", 69, 4, 16),
				new ConfigToken(ConfigToken.ConfigTokenType.Number, "4", 71, 4, 18),
				new ConfigToken(ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight, "}", 73, 4, 20),
				new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Semicolumn, ";", 74, 4, 21),
				new ConfigToken(ConfigToken.ConfigTokenType.Identifier, "arr[]", 77, 5, 1),
				new ConfigToken(ConfigToken.ConfigTokenType.Symbol_MinusAssign, "-=", 83, 5, 7),
				new ConfigToken(ConfigToken.ConfigTokenType.Symbol_CurlyBracketLeft, "{", 86, 5, 10),
				new ConfigToken(ConfigToken.ConfigTokenType.Number, "52", 88, 5, 12),
				new ConfigToken(ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight, "}", 91, 5, 15),
				new ConfigToken(ConfigToken.ConfigTokenType.Symbol_Semicolumn, ";", 92, 5, 16),
				new ConfigToken(ConfigToken.ConfigTokenType.Symbol_CurlyBracketRight, "}", 94, 6, 0),
				new ConfigToken(ConfigToken.ConfigTokenType.EndOfDocument, "", 95, 6, 1)
			};

			StringInputReader document = "class Test\n" +
										 "{\n" +
										 "\tstr = \"test text\";\n" +
										 "\tstr_broken=\"broken\n" +
										 "\tarr[] += { 2, 3, 4 };\n" +
										 "\tarr[] -= { 52 };\n" +
										 "}";

			LexerBase<ConfigToken> lexer = new ConfigLexer(document);
			ConfigToken[] lexer_result = lexer.Consume(document_lexemes.Length).ToArray();

			for (int i = 0; i < lexer_result.Length; i++)
				Assert.AreEqual(document_lexemes[i], lexer_result[i], i.ToString());
		}

		[TestMethod()]
		public void BrokenString_Test()
		{
			ConfigToken[] document_lexemes = new ConfigToken[]
			{
				new ConfigToken(ConfigToken.ConfigTokenType.BrokenString, "\"Hello, world!", 0, 0, 0)
			};

			StringInputReader document = "\"Hello, world!";
			LexerBase<ConfigToken> lexer = new ConfigLexer(document);

			ConfigToken[] lexer_result = lexer.Consume(document_lexemes.Length).ToArray();
			for (int i = 0; i < lexer_result.Length; i++)
				Assert.AreEqual(document_lexemes[i], lexer_result[i], i.ToString());
		}

		[TestMethod()]
		public void String_Test()
		{
			ConfigToken[] document_lexemes = new ConfigToken[]
			{
				new ConfigToken(ConfigToken.ConfigTokenType.String, "\"Hello, world!\"", 0, 0, 0)
			};

			StringInputReader document = "\"Hello, world!\"";
			LexerBase<ConfigToken> lexer = new ConfigLexer(document);

			ConfigToken[] lexer_result = lexer.Consume(document_lexemes.Length).ToArray();
			for (int i = 0; i < lexer_result.Length; i++)
				Assert.AreEqual(document_lexemes[i], lexer_result[i], i.ToString());
		}

		[TestMethod()]
		public void Number_Test()
		{
			ConfigToken[] document_lexemes = new ConfigToken[]
			{
				new ConfigToken(ConfigToken.ConfigTokenType.Number, "522", 0, 0, 0)
			};

			StringInputReader document = "522";
			LexerBase<ConfigToken> lexer = new ConfigLexer(document);

			ConfigToken[] lexer_result = lexer.Consume(document_lexemes.Length).ToArray();
			for (int i = 0; i < lexer_result.Length; i++)
				Assert.AreEqual(document_lexemes[i], lexer_result[i], i.ToString());
		}
	}
}