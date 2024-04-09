using CSToolbox.Collection;

namespace DayZObfuscatorModel.PBO
{
    public class PBODescriptor
	{
		public IList<PBOFile> Files { get; }

		public IList<PBOConfigDescriptor> Configs { get; }

		public PBOConfigDescriptor? RootConfig => Configs.FirstOrDefault(x => x.PathInPBO == string.Empty);

		public string DirectoryPath { get; }

		public PBODescriptor(string directoryPath, IEnumerable<PBOFile> files)
		{
			ArgumentNullException.ThrowIfNull(files);
			ArgumentNullException.ThrowIfNull(directoryPath);

			if (!Directory.Exists(directoryPath))
				throw new ArgumentException("Path should be a valid path to a directory.", nameof(directoryPath));

			DirectoryPath = directoryPath;
			Files = files.ToList();
			Configs = new SublistAdapter<PBOFile, PBOConfigDescriptor>(Files);
		}
	}
}
