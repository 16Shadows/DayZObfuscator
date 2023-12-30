using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigArrayExpressionBase : PBOConfigExpressionBase
	{
		public IList<object> Value { get; }

		public PBOConfigArrayExpressionBase(string identifier, IList<object> value) : base(identifier)
		{
			Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		public override bool Equals(object? obj)
		{
			return obj is PBOConfigArrayExpressionBase other && Value.SequenceEqual(other.Value);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(base.GetHashCode(), "array");
		}
	}
}
