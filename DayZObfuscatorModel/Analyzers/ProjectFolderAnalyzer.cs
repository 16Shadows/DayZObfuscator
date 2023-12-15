using DayZObfuscatorModel.PBO;

namespace DayZObfuscatorModel.Analyzers
{
	public static class ProjectFolderAnalyzer
	{
		public static IEnumerable<PBODescriptor> Analyze(string path)
		{
			if (!Path.Exists(path) || Path.GetFileName(path) != null)
				throw new ArgumentException("Path should be a valid path to a directory.", nameof(path));

			return null;
		}
	}
}
