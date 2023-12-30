using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigExpressionVariableAssignment : PBOConfigExpressionBase
	{
		public object Value { get; }

		public PBOConfigExpressionVariableAssignment(string identifier, object value) : base(identifier)
		{
			Value = value;
		}

		public override string ToString()
		{
			return $"{Identifier} = {Value};";
		}

		public override bool Equals(object? obj)
		{
			return base.Equals(obj) && obj is PBOConfigExpressionVariableAssignment other && Value.Equals(other.Value);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(base.GetHashCode(), Value);
		}
	}
}
