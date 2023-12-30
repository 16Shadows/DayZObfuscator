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
			Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		public override string ToString()
		{
			return $"\"{Value.Escape()}\"";
		}

		public override bool Equals(object? obj)
		{
			return obj is PBOConfigValueString other && Value.Equals(other.Value);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Value);
		}
	}
}
