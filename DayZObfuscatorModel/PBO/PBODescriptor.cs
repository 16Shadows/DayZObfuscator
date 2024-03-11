namespace DayZObfuscatorModel.PBO
{
    public class PBODescriptor
	{
		public IList<PBOFile> Files { get; } = new List<PBOFile>();

		public IReadOnlyList<PBOConfigDescriptor> Configs { get; }

		public PBOConfigDescriptor? RootConfig => Configs.FirstOrDefault(x => x.PathInPBO == string.Empty);

		public string DirectoryPath { get; }

		public PBODescriptor(string directoryPath, IEnumerable<PBOConfigDescriptor> configs)
		{
			ArgumentNullException.ThrowIfNull(configs);
			ArgumentNullException.ThrowIfNull(directoryPath);

			if (!Directory.Exists(directoryPath))
				throw new ArgumentException("Path should be a valid path to a directory.", nameof(directoryPath));

			DirectoryPath = directoryPath;
			Configs = configs.ToList();
		}
	}
}
