using Microsoft.VisualStudio.TestTools.UnitTesting;
using DayZObfuscatorModel.Analyzers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayZObfuscatorModel.PBO;

namespace DayZObfuscatorModel.Analyzers.Tests
{
	[TestClass()]
	public class ProjectFolderAnalyzer_Tests
	{
		[TestMethod()]
		public void Analyze_Test1()
		{
			if (Directory.Exists("analyze_test1_sample"))
				Directory.Delete("analyze_test1_sample", true);

			Directory.CreateDirectory("analyze_test1_sample");
			Directory.CreateDirectory("analyze_test1_sample/SuicideButton");

			string config = "class CfgMods\n" +
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

			File.WriteAllText("analyze_test1_sample/SuicideButton/config.cpp", config);
			Directory.CreateDirectory("analyze_test1_sample/SuicideButton/5_Mission");

			string script = "modded class MissionServer\n" + 
							"{\n" +
							"\tvoid MissionServer()\n" +
							"\t{\n" +
							"\t\tPrint(\"Hello, world!\");\n" +
							"\t}\n" +
							"}";

			File.WriteAllText("analyze_test1_sample/SuicideButton/5_Mission/mission.c", script);

			List<PBODescriptor> pbos = ProjectFolderAnalyzer.Analyze("analyze_test1_sample").ToList();

			Assert.AreEqual(1, pbos.Count);
		}
	}
}