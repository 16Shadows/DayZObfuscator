using DayZObfuscatorModel.PBO.Packer;

namespace DayZObfuscatorModel.PBO.Config
{
	public abstract class PBOConfigValueBase
	{
		public abstract byte GetBinarizedType();
		public abstract void Binarize(PBOWriter writer);
		public abstract uint GetBinarizedSize();
	}
}
