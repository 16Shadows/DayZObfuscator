using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigExpressionBase
	{
		public string Identifier { get; set; }

		public PBOConfigExpressionBase(string identifier)
		{
			Identifier = identifier;
		}
	}
}
