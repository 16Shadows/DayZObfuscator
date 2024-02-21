using DayZObfuscatorModel.PBO.Packer;

namespace DayZObfuscatorModel.PBO.Config
{
	public abstract class PBOConfigExpressionBase
	{
		public string Identifier { get; set; }

		public PBOConfigExpressionBase(string identifier)
		{
			Identifier = identifier;
		}

		public override bool Equals(object? obj)
		{
			return obj is PBOConfigExpressionBase other && Identifier == other.Identifier;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Identifier);
		}

		public abstract void Binarize(PBOWriter writer);
		public abstract uint GetBinarizedSize();
	}
}
