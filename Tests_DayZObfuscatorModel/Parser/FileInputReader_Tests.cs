using Microsoft.VisualStudio.TestTools.UnitTesting;
using DayZObfuscatorModel.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace DayZObfuscatorModel.Parser.Tests
{
	[TestClass()]
	public class FileInputReader_Tests
	{
		protected static string TestFileContent =	"class CfgMods\n" +
													"{\n" +
													"\tclass SuicideButton\n" +
													"\t{\n" +
													"\t\tdir=\"SuicideButton\";\n" +
													"\t\ttype = \"mod\";\n" +
													"\t\tauthor = \"16Shadows\";\n" +
													"\t\tversion = \"1.0.0\";\n" +
													"\t\tname=\"SuicideButton\";\n" +
													"\t\tdependencies[] = {\"Mission\"};\n" +
													"\t\tclass defs\n" +
													"\t\t{\n" +
													"\t\t\tclass missionScriptModule\n" +
													"\t\t\t{\n" +
													"\t\t\t\tvalue = \"\";\n" +
													"\t\t\t\tfiles[] = {\"SuicideButton/5_Mission\" };\n" +
													"\t\t\t};\n" +
													"\t\t};\n" +
													"\t};\n" +
													"};";

		protected void GenerateTestFile([CallerMemberName]string caller="")
		{
			string path = GetTestFileName(caller);

			if (File.Exists(path))
				File.Delete(path);

			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(path, TestFileContent);
		}

		protected string GetTestFileName([CallerMemberName]string caller="")
		{
			return $"FileInputReader_Tests/{caller}/config.cpp";
		}

		[TestMethod()]
		public void Consume_Test()
		{
			GenerateTestFile();

			FileInputReader reader = new FileInputReader(GetTestFileName());

			Assert.AreEqual(TestFileContent[0], reader.Consume());
			Assert.AreEqual(TestFileContent[1], reader.Consume());
			Assert.AreEqual(TestFileContent[2], reader.Consume());
			Assert.AreEqual(TestFileContent[3], reader.Consume());
			Assert.AreEqual(TestFileContent[4], reader.Consume());
			Assert.AreEqual(TestFileContent[5], reader.Consume());
		}

		[TestMethod()]
		public void ConsumeN_Test()
		{
			GenerateTestFile();

			FileInputReader reader = new FileInputReader(GetTestFileName());

			Assert.AreEqual(TestFileContent[0..6], reader.Consume(6));
		}

		[TestMethod()]
		public void ConsumePastEnd_Test()
		{
			GenerateTestFile();

			FileInputReader reader = new FileInputReader(GetTestFileName());

			reader.Consume(TestFileContent.Length);

			Assert.AreEqual('\0', reader.Consume());
		}

		[TestMethod()]
		public void Peek_Test()
		{
			GenerateTestFile();

			FileInputReader reader = new FileInputReader(GetTestFileName());

			Assert.AreEqual(TestFileContent[0], reader.Peek());
			Assert.AreEqual(TestFileContent[0], reader.Peek());
			Assert.AreEqual(TestFileContent[0], reader.Consume());
			Assert.AreEqual(TestFileContent[1], reader.Peek());
		}

		[TestMethod()]
		public void PeekN_Test()
		{
			GenerateTestFile();

			FileInputReader reader = new FileInputReader(GetTestFileName());

			Assert.AreEqual(TestFileContent[0..6], reader.Peek(6));
			Assert.AreEqual(TestFileContent[0..6], reader.Consume(6));
			Assert.AreEqual(TestFileContent[6..12], reader.Peek(6));
		}

		[TestMethod()]
		public void PeekPastEnd_Test()
		{
			GenerateTestFile();

			FileInputReader reader = new FileInputReader(GetTestFileName());

			reader.Consume(TestFileContent.Length);

			Assert.AreEqual('\0', reader.Peek());
		}
	}
}