using DayZObfuscatorModel.PBO.Config;

namespace DayZObfuscatorModel.PBO
{
    public class PBODescriptor
	{
		public ICollection<PBOFile> Files { get; } = new HashSet<PBOFile>();

		public PBOConfigClass Config { get; }

		public PBODescriptor(PBOConfigClass config)
		{
			ArgumentNullException.ThrowIfNull(config);

			Config = config;
		}
	}
}
