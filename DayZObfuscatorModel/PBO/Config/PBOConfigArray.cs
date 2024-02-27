using DayZObfuscatorModel.PBO.Packer;
using CSToolbox.Extensions;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigArray : PBOConfigValueBase, IEquatable<PBOConfigArray>, IEquatable<IList<PBOConfigValueBase>>
	{
		private IList<PBOConfigValueBase> _Value;
		public IList<PBOConfigValueBase> Value
		{
			get => _Value;
			set => _Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		public PBOConfigArray(IList<PBOConfigValueBase> value)
		{
			ArgumentNullException.ThrowIfNull(value);

			_Value = value;
		}

		public override string ToString()
		{
			return $"{{ {Value.ToString(", ")} }}";
		}

		public override bool Equals(object? obj)
		{
			return (obj is PBOConfigArray other && Equals(other)) ||
					(obj is IList<PBOConfigValueBase> otherList && Equals(otherList));
		}

		public bool Equals(PBOConfigArray? other)
		{
			return Equals(other?.Value);
		}

		public bool Equals(IList<PBOConfigValueBase>? other)
		{
			return other?.SequenceEqual(Value) == true;
		}

		public override int GetHashCode()
		{
			int code = HashCode.Combine("array");
			foreach (var item in Value)
				code = HashCode.Combine(code, item.GetHashCode());
			return code;
		}

		public override void Binarize(PBOWriter writer)
		{
			writer.Write((byte)Value.Count);
			foreach (PBOConfigValueBase value in Value)
			{
				writer.Write(value.GetBinarizedType());
				value.Binarize(writer);
			}
		}

		public override uint GetBinarizedSize()
		{
			uint size = 1 + (uint)Value.Count; //1 for number of items + 1 byte for type of each item
			foreach (PBOConfigValueBase value in Value)
				size += value.GetBinarizedSize();
			return size;
		}

		public override byte GetBinarizedType()
		{
			return 3;
		}
	}
}
