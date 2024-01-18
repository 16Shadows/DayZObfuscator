using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigArrayExpressionAdd : PBOConfigArrayExpressionBase
	{
		public PBOConfigArrayExpressionAdd(string identifier, IList<object> value) : base(identifier, value)
		{
		}

		public override string ToString()
		{
			return $"{Identifier} += {Value.ToString(", ")};";
		}

		public override bool Equals(object? obj)
		{
			return base.Equals(obj) && obj is PBOConfigArrayExpressionAdd;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine("+=", base.GetHashCode());
		}
	}
}
