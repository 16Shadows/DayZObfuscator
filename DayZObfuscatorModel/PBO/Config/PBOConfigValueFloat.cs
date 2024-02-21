using DayZObfuscatorModel.PBO.Packer;
using System.Globalization;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigValueFloat : PBOConfigValueBase, IEquatable<PBOConfigValueFloat>, IEquatable<float>
	{
		public float Value { get; set; }

		public PBOConfigValueFloat(float value)
		{
			Value = value;
		}

		public override string ToString()
		{
			return Value.ToString(CultureInfo.InvariantCulture);
		}

		public override bool Equals(object? obj)
		{
			return (obj is PBOConfigValueFloat other && Equals(other)) ||
					(obj is float otherFloat && Equals(otherFloat));
		}

		public bool Equals(PBOConfigValueFloat? other)
		{
			return other?.Value == Value;
		}

		public bool Equals(float other)
		{
			return Value == other;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
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
			return 1;
		}
	}
}
