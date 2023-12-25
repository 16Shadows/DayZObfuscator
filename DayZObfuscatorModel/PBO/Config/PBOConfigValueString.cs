using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigValueString
	{
		public string Value { get; set; }
		public PBOConfigValueString(string value)
		{
			Value = value;
		}

		public override string ToString()
		{
			return $"\"{Value.Escape()}\"";
		}
	}
}
