using DayZObfuscatorModel.PBO.Packer;
using System.Globalization;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigValueInt : PBOConfigVariableValue, IEquatable<PBOConfigValueInt>, IEquatable<int>
	{
		public int Value { get; set; }

		public PBOConfigValueInt(int value)
		{
			Value = value;
		}

		public override string ToString()
		{
			return Value.ToString(CultureInfo.InvariantCulture);
		}

		public override bool Equals(object? obj)
		{
			return (obj is PBOConfigValueInt other && Equals(other)) ||
					(obj is int otherInt && Equals(otherInt));
		}

		public bool Equals(PBOConfigValueInt? other)
		{
			return other?.Value == Value;
		}

		public bool Equals(int other)
		{
			return Value == other;
		}

		public override void Binarize(PBOWriter writer)
		{
			writer.Write(Value);
		}

		public override uint GetBinarizedSize()
		{
			return 4;
		}

		public override byte GetBinarizedType()
		{
			return 2;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}
}
