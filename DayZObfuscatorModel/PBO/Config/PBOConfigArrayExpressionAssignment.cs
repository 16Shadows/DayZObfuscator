using CSToolbox.Extensions;
using DayZObfuscatorModel.PBO.Packer;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigArrayExpressionAssignment : PBOConfigArrayExpressionBase
	{
		public PBOConfigArrayExpressionAssignment(string identifier, IList<PBOConfigValueBase> value) : base(identifier, value)
		{
		}

		public override string ToString()
		{
			return $"{Identifier} = {{{Value.ToString(", ")}}};";
		}

		public override bool Equals(object? obj)
		{
			return base.Equals(obj) && obj is PBOConfigArrayExpressionAssignment;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine("=", base.GetHashCode());
		}

		public override void Binarize(PBOWriter writer)
		{
			writer.Write((byte)2);
			writer.Write(Identifier.TrimEnd('[', ']'));
			writer.Write((byte)Value.Count);
			foreach(PBOConfigValueBase value in Value)
			{
				writer.Write(value.GetBinarizedType());
				value.Binarize(writer);
			}
		}

		public override uint GetBinarizedSize()
		{
			uint size = (uint)Identifier.TrimEnd('[', ']').Length + 1 + 1 + 1 + (uint)Value.Count; // Identifier length + 1 for terminator + 1 byte for number of items + 1 byte for type + 1 byte for variable type of each value
			foreach (PBOConfigValueBase value in Value)
				size += value.GetBinarizedSize(); //If its a string, length + 1 byte for terminator. Otherwise its 4 bytes (for int or float)

			return size;
		}
	}
}
