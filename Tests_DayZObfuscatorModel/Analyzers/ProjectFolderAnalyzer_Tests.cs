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
		public void LoadPBO_Test()
		{
			if (Directory.Exists("ProjectFolderAnalyzer_Tests/LoadPBO_Test"))
				Directory.Delete("ProjectFolderAnalyzer_Tests/LoadPBO_Test", true);

			Directory.CreateDirectory("ProjectFolderAnalyzer_Tests/LoadPBO_Test/SuicideButton");

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

			File.WriteAllText("ProjectFolderAnalyzer_Tests/LoadPBO_Test/SuicideButton/config.cpp", config);
			Directory.CreateDirectory("ProjectFolderAnalyzer_Tests/LoadPBO_Test/SuicideButton/5_Mission");

			string script = "modded class MissionServer\n" + 
							"{\n" +
							"\tvoid MissionServer()\n" +
							"\t{\n" +
							"\t\tPrint(\"Hello, world!\");\n" +
							"\t}\n" +
							"}";

			File.WriteAllText("ProjectFolderAnalyzer_Tests/LoadPBO_Test/SuicideButton/5_Mission/mission.c", script);

			PBODescriptor? pbo = ProjectFolderAnalyzer.LoadPBO("ProjectFolderAnalyzer_Tests/LoadPBO_Test/SuicideButton");

			Assert.IsNotNull(pbo);
			Assert.AreEqual(1, pbo.Configs.Count);
			Assert.AreEqual(Path.GetFullPath("ProjectFolderAnalyzer_Tests/LoadPBO_Test/SuicideButton"), pbo.DirectoryPath);
			Assert.AreEqual(2, pbo.Files.Count);
		}

		[TestMethod()]
		public void Analyze_SingleProject_Test()
		{
			if (Directory.Exists("ProjectFolderAnalyzer_Tests/Analyze_SingleProject_Test"))
				Directory.Delete("ProjectFolderAnalyzer_Tests/Analyze_SingleProject_Test", true);

			Directory.CreateDirectory("ProjectFolderAnalyzer_Tests/Analyze_SingleProject_Test/SuicideButton");

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

			File.WriteAllText("ProjectFolderAnalyzer_Tests/Analyze_SingleProject_Test/SuicideButton/config.cpp", config);
			Directory.CreateDirectory("ProjectFolderAnalyzer_Tests/Analyze_SingleProject_Test/SuicideButton/5_Mission");

			string script = "modded class MissionServer\n" + 
							"{\n" +
							"\tvoid MissionServer()\n" +
							"\t{\n" +
							"\t\tPrint(\"Hello, world!\");\n" +
							"\t}\n" +
							"}";

			File.WriteAllText("ProjectFolderAnalyzer_Tests/Analyze_SingleProject_Test/SuicideButton/5_Mission/mission.c", script);

			PBODescriptor? pbo = ProjectFolderAnalyzer.LoadPBO("ProjectFolderAnalyzer_Tests/Analyze_SingleProject_Test/SuicideButton");

			Assert.IsNotNull(pbo);
			Assert.AreEqual(1, pbo.Configs.Count);
			Assert.AreEqual(Path.GetFullPath("ProjectFolderAnalyzer_Tests/Analyze_SingleProject_Test/SuicideButton"), pbo.DirectoryPath);
		}

		[TestMethod()]
		public void Analyze_MultiProject_Test()
		{
			if (Directory.Exists("ProjectFolderAnalyzer_Tests/Analyze_MultiProject_Test"))
				Directory.Delete("ProjectFolderAnalyzer_Tests/Analyze_MultiProject_Test", true);

			{
				Directory.CreateDirectory("ProjectFolderAnalyzer_Tests/Analyze_MultiProject_Test/SuicideButton");

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

				File.WriteAllText("ProjectFolderAnalyzer_Tests/Analyze_MultiProject_Test/SuicideButton/config.cpp", config);
				Directory.CreateDirectory("ProjectFolderAnalyzer_Tests/Analyze_MultiProject_Test/SuicideButton/5_Mission");

				string script = "modded class MissionServer\n" + 
								"{\n" +
								"\tvoid MissionServer()\n" +
								"\t{\n" +
								"\t\tPrint(\"Hello, world!\");\n" +
								"\t}\n" +
								"}";

				File.WriteAllText("ProjectFolderAnalyzer_Tests/Analyze_MultiProject_Test/SuicideButton/5_Mission/mission.c", script);
			}

			{
				Directory.CreateDirectory("ProjectFolderAnalyzer_Tests/Analyze_MultiProject_Test/SuicideButton2");

				string config = "class CfgMods\n" +
								"{\n" +
								"\tclass SuicideButton2\n" +
								"\t{\n" +
								"\t\tdir=\"SuicideButton2\";\n" +
								"\t\ttype = \"mod\";\n" +
								"\t\tauthor = \"16Shadows\";\n" +
								"\t\tversion = \"1.0.0\";\n" +
								"\t\tname=\"SuicideButton2\";\n" +
								"\t\tdependencies[] = {\"Mission\"};\n" +
								"\t\tclass defs\n" +
								"\t\t{\n" +
								"\t\t\tclass missionScriptModule\n" +
								"\t\t\t{\n" +
								"\t\t\t\tvalue = \"\";\n" +
								"\t\t\t\tfiles[] = {\"SuicideButton2/5_Mission\" };\n" +
								"\t\t\t};\n" +
								"\t\t};\n" +
								"\t};\n" +
								"};";

				File.WriteAllText("ProjectFolderAnalyzer_Tests/Analyze_MultiProject_Test/SuicideButton2/config.cpp", config);
				Directory.CreateDirectory("ProjectFolderAnalyzer_Tests/Analyze_MultiProject_Test/SuicideButton2/5_Mission");

				string script = "modded class MissionServer\n" + 
								"{\n" +
								"\tvoid MissionServer()\n" +
								"\t{\n" +
								"\t\tPrint(\"Hello, world 2!\");\n" +
								"\t}\n" +
								"}";

				File.WriteAllText("ProjectFolderAnalyzer_Tests/Analyze_MultiProject_Test/SuicideButton2/5_Mission/mission.c", script);
			}

			PBODescriptor? pbos = ProjectFolderAnalyzer.LoadPBO("ProjectFolderAnalyzer_Tests/Analyze_MultiProject_Test");

			Assert.IsNotNull(pbos);
			Assert.AreEqual(2, pbos.Configs.Count);
			Assert.IsTrue(pbos.DirectoryPath == Path.GetFullPath("ProjectFolderAnalyzer_Tests/Analyze_MultiProject_Test"));
		}
	}
}