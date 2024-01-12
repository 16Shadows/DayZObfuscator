using DayZObfuscatorModel.PBO;
using DayZObfuscatorModel.PBO.Config;
using DayZObfuscatorModel.PBO.Config.Parser;

namespace DayZObfuscatorModel.Analyzers
{
	public static class ProjectFolderAnalyzer
	{
		/*
		private static readonly ConfigParser _ConfigParser = new ConfigParser();

		public static IEnumerable<PBODescriptor> Analyze(string path)
		{
			if (!Path.Exists(path) || Path.GetFileName(path) != null)
				throw new ArgumentException("Path should be a valid path to a directory.", nameof(path));

			path = Path.GetFullPath(path);

			IEnumerable<PBODescriptor> descriptors;

			if (Path.Exists(path + "config.cpp"))
			{
				
			}

			foreach (string subdir in Directory.EnumerateDirectories(path))
				descriptors = descriptors.Concat(Analyze(subdir));
		}*/
	}
}
