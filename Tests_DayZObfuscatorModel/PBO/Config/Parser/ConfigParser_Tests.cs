using Microsoft.VisualStudio.TestTools.UnitTesting;
using DayZObfuscatorModel.Parser;
using DayZObfuscatorModel.PBO.Config.Parser.Lexer;
using CSToolbox.Extensions;

namespace DayZObfuscatorModel.PBO.Config.Parser.Tests
{
	[TestClass()]
	public class ConfigParser_Tests
	{
		class TestErrorResolver : IParserErrorResolver<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors>, ConfigParserStates>
		{
			public ILexer<ConfigToken> Resolve(ILexer<ConfigToken> lexer, IParser<ConfigToken, PBOConfig, ParserErrorBase<ConfigParserErrors>, ConfigParserStates> parser, ParserState<ConfigToken, ConfigParserStates> state, ParserErrorBase<ConfigParserErrors> e)
			{
				throw new InvalidSyntaxException($"Syntax error occured with token '{state.CurrentToken}'", state.CurrentToken.Index, state.CurrentToken.Line, state.CurrentToken.IndexOnLine);
			}
		}

		[TestMethod()]
		public void Parse_Test()
		{
			StringInputReader document = "class Test\n" +
										 "{\n" +
										 "\tstr = \"test text\";\n" +
										 "\tarr[] = { 52 };\n" +
										 "\tarr[] += { 2, 3, 4 };\n" +
										 "\tarr[] -= { 52 };\n" +
										 "\tclass TestInner\n" +
										 "\t{\n" +
										 "\t\tvar = 5;\n" +
										 "\t};\n" +
										 "};";

			ConfigParser parser = new ConfigParser(new TestErrorResolver());

			ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors>> result = parser.Parse(new ConfigLexer(document));
			PBOConfig config = result.Result;

			Assert.AreEqual(0, result.Errors.Count());

			Assert.IsNotNull(config);
			Assert.AreEqual(1, config.Scopes.Count());


			Assert.AreEqual("Test", config.Scopes.First().Identifier);
			
			Assert.AreEqual(5, config.Scopes.First().Expressions.Count);
			Assert.AreEqual(1, config.Scopes.First().Variables.Count());
			Assert.AreEqual(3, config.Scopes.First().Arrays.Count());
			Assert.AreEqual(1, config.Scopes.First().Scopes.Count());

			Assert.AreEqual("str", config.Scopes.First().Variables.First().Identifier);
			Assert.AreEqual(new PBOConfigValueString("test text"), config.Scopes.First().Variables.First().Value);

			List<PBOConfigArrayExpressionBase> arrays = new List<PBOConfigArrayExpressionBase>()
			{
				new PBOConfigArrayExpressionAssignment("arr[]", new List<object>() { 52 }),
				new PBOConfigArrayExpressionAdd("arr[]", new List<object>() { 2, 3, 4 }),
				new PBOConfigArrayExpressionSubtract("arr[]", new List<object>() { 52 })
			};

			Assert.IsTrue(arrays.SequenceEqualsOrderInvariant(config.Scopes.First().Arrays));

			Assert.AreEqual("TestInner", config.Scopes.First().Scopes.First().Identifier);
			
			Assert.AreEqual(1, config.Scopes.First().Scopes.First().Expressions.Count);
			Assert.AreEqual(1, config.Scopes.First().Scopes.First().Variables.Count());
			Assert.AreEqual(0, config.Scopes.First().Scopes.First().Arrays.Count());
			Assert.AreEqual(0, config.Scopes.First().Scopes.First().Scopes.Count());

			Assert.AreEqual("var", config.Scopes.First().Scopes.First().Variables.First().Identifier);
			Assert.AreEqual(5, config.Scopes.First().Scopes.First().Variables.First().Value);
		}

		[TestMethod()]
		public void ParseError_Test()
		{
			StringInputReader document = "class Test\n" +
										 "{\n" +
										 "\tstr = \"test text;\n" +
										 "\tarr[] = { 52 }\n" +
										 "\tarr += { 2, 3, 4 };\n" +
										 "\tarr[] = 52 ;\n" +
										 "\tclasS TestInner\n" +
										 "\t{\n" +
										 "\t\tvar = 5;\n" +
										 "\t};\n" +
										 "};";
			 
			ConfigParser parser = new ConfigParser(new TestErrorResolver());

			Assert.ThrowsException<InvalidSyntaxException>(() =>
			{
				ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors>> result = parser.Parse(new ConfigLexer(document));
			});
		}

		[TestMethod()]
		public void Parse_ValueError_BrokenString_Test()
		{
			StringInputReader document = "class Test\n" +
										 "{\n" +
										 "\tstr = \"test text;\n" +
										 "};";
			 
			ConfigParser parser = new ConfigParser(new ConfigParserErrorResolver());
			ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors>> result = parser.Parse(new ConfigLexer(document));

			Assert.AreEqual(2, result.Errors.Count());
			Assert.AreEqual(ConfigParserErrors.BrokenString, result.Errors.First().Message);
			Assert.AreEqual(ConfigParserErrors.ExpectedSemicolumn, result.Errors.Skip(1).First().Message);
		}

		[TestMethod()]
		public void Parse_ValueError_InvalidNumber_Test()
		{
			StringInputReader document = "class Test\n" +
										 "{\n" +
										 "\tstr = -22.32.33;\n" +
										 "};";
			 
			ConfigParser parser = new ConfigParser(new ConfigParserErrorResolver());
			ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors>> result = parser.Parse(new ConfigLexer(document));

			Assert.AreEqual(1, result.Errors.Count());
			Assert.AreEqual(ConfigParserErrors.InvalidNumber, result.Errors.First().Message);
		}

		[TestMethod()]
		public void Parse_ValueError_InvalidToken_Test()
		{
			StringInputReader document = "class Test\n" +
										 "{\n" +
										 "\tstr = var22;\n" +
										 "};";
			 
			ConfigParser parser = new ConfigParser(new ConfigParserErrorResolver());
			ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors>> result = parser.Parse(new ConfigLexer(document));

			Assert.AreEqual(1, result.Errors.Count());
			Assert.AreEqual(ConfigParserErrors.UnexpectedToken, result.Errors.First().Message);
		}

		[TestMethod()]
		public void Parse_ArrayError_MissingLeftBracket()
		{
			StringInputReader document = "class Test\n" +
										 "{\n" +
										 "\tarr[] = 22, 33 };\n" +
										 "};";
			 
			ConfigParser parser = new ConfigParser(new ConfigParserErrorResolver());
			ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors>> result = parser.Parse(new ConfigLexer(document));

			Assert.AreEqual(1, result.Errors.Count());
			Assert.AreEqual(ConfigParserErrors.ExpectedLeftCurlyBracket, result.Errors.First().Message);
		}

		[TestMethod()]
		public void Parse_ArrayError_MissingRightBracket()
		{
			StringInputReader document = "class Test\n" +
										 "{\n" +
										 "\tarr[] = { 22, 33 ;\n" +
										 "};";
			 
			ConfigParser parser = new ConfigParser(new ConfigParserErrorResolver());
			ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors>> result = parser.Parse(new ConfigLexer(document));

			Assert.AreEqual(1, result.Errors.Count());
			Assert.AreEqual(ConfigParserErrors.ExpectedCommaOrRightCurlyBracket, result.Errors.First().Message);
		}

		[TestMethod()]
		public void Parse_ArrayError_Comma()
		{
			StringInputReader document = "class Test\n" +
										 "{\n" +
										 "\tarr[] = { 22 33 };\n" +
										 "};";
			 
			ConfigParser parser = new ConfigParser(new ConfigParserErrorResolver());
			ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors>> result = parser.Parse(new ConfigLexer(document));

			Assert.AreEqual(1, result.Errors.Count());
			Assert.AreEqual(ConfigParserErrors.ExpectedCommaOrRightCurlyBracket, result.Errors.First().Message);
		}

		[TestMethod()]
		public void Parse_ArrayError_MissingValue()
		{
			StringInputReader document = "class Test\n" +
										 "{\n" +
										 "\tarr[] = { 22, };\n" +
										 "};";
			 
			ConfigParser parser = new ConfigParser(new ConfigParserErrorResolver());
			ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors>> result = parser.Parse(new ConfigLexer(document));

			Assert.AreNotEqual(0, result.Errors.Count());
		}
	}
}