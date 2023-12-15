using Microsoft.VisualStudio.TestTools.UnitTesting;
using DayZObfuscatorModel.PBO.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.PBO.Config.Tests
{
	[TestClass()]
	public class PBOConfig_Tests
	{
		[TestMethod()]
		public void Parse_Test_Success()
		{
			string exampleConfig = 
			"class CfgMods\n" +
			"{\n" +
			"\tclass TestPBO\n" +
			"\t{\n" +
			"\t\tdir=\"TestPBO=\";\n" +
			"\t\ttype=\"mod\";\n" +
			"\t\tauthor=\"16Shadows\";\n" +
			"\t\tname=\"TestPBO\";\n" +
			"\t\tclass defs\n" +
			"\t\t{\n" +
			"\t\t\tclass missionScriptModule\n" +
			"\t\t\t{\n" +
			"\t\t\t\tvalue=\"\";\n" +
			"\t\t\t\tfiles[]={\"TestPBO\\5_Mission\"};\n" +
			"\t\t\t};\n" +
			"\t\t};\n" +
			"\t};\n" +
			"};";

			PBOConfig cfg = PBOConfig.Parse(exampleConfig.AsSpan());
			Assert.IsNotNull(cfg);
		}
	}
}