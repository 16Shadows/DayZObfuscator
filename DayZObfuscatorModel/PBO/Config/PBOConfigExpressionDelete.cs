using DayZObfuscatorModel.PBO.Packer;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigExpressionDelete : PBOConfigExpressionBase
	{
		public PBOConfigExpressionDelete(string identifier) : base(identifier)
		{
		}

		public override string ToString()
		{
			return $"delete {Identifier};";
		}

		public override bool Equals(object? obj)
		{
			return base.Equals(obj) && obj is PBOConfigExpressionDelete other && other.Identifier == Identifier;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(base.GetHashCode(), "delete");
		}

		public override void Binarize(PBOWriter writer)
		{
			writer.Write((byte)4);
			writer.Write(Identifier);
		}

		public override uint GetBinarizedSize()
		{
			return (uint)Identifier.Length + 1 + 1; //Identifier length + 1 for terminator + 1 for type
		}
	}
}
