using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigArrayExpressionUnion : PBOConfigArrayExpressionBase
	{
		public PBOConfigArrayExpressionUnion(string identifier, IEnumerable<object> value) : base(identifier, value)
		{
		}

		public override string ToString()
		{
			return $"{Identifier} += {Value};";
		}
	}
}
