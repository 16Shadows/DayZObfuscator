using DayZObfuscatorModel.PBO.Config;

namespace DayZObfuscatorModel.PBO
{
    public class PBODescriptor
	{
		public ICollection<PBOFile> Files { get; } = new HashSet<PBOFile>();

		public PBOConfig Config { get; }

		public PBODescriptor(PBOConfig config)
		{
			ArgumentNullException.ThrowIfNull(config);

			Config = config;
		}
	}
}
