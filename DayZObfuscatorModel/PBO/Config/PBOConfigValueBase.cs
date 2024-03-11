using DayZObfuscatorModel.PBO.Packer;

namespace DayZObfuscatorModel.PBO.Config
{
	public abstract class PBOConfigValueBase : IEquatable<PBOConfigValueBase>
	{
		public abstract byte GetBinarizedType();
		public abstract void Binarize(PBOWriter writer);
		public abstract uint GetBinarizedSize();
		public abstract bool Equals(PBOConfigValueBase? other);
	}
}
