using DayZObfuscatorModel.PBO.Packer;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigArrayExpressionAdd : PBOConfigArrayExpressionBase
	{
		public PBOConfigArrayExpressionAdd(string identifier, PBOConfigArray value) : base(identifier, value)
		{
		}

		public override string ToString()
		{
			return $"{Identifier} += {Value};";
		}

		public override bool Equals(object? obj)
		{
			return base.Equals(obj) && obj is PBOConfigArrayExpressionAdd;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine("+=", base.GetHashCode());
		}

		public override void Binarize(PBOWriter writer)
		{
			writer.Write((byte)5);
			writer.Write(2u);
			writer.Write(Identifier.TrimEnd('[', ']'));
			Value.Binarize(writer);
		}

		public override uint GetBinarizedSize()
		{
			return (uint)Identifier.TrimEnd('[', ']').Length + 1 + 1 + 4 + Value.GetBinarizedSize(); // Identifier length + 1 for terminator + 1 byte for type + 4 for modification type
		}
	}
}
