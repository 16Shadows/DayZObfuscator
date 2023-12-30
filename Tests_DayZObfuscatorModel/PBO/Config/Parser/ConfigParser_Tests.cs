using Microsoft.VisualStudio.TestTools.UnitTesting;
using DayZObfuscatorModel.Parser;
using DayZObfuscatorModel.PBO.Config.Parser.Lexer;

namespace DayZObfuscatorModel.PBO.Config.Parser.Tests
{
	[TestClass()]
	public class ConfigParser_Tests
	{
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

			ConfigParser parser = new ConfigParser();

			PBOConfig config = parser.Parse(new ConfigLexer(document));

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
	}
}