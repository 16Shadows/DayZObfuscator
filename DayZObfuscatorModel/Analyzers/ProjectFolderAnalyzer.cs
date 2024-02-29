using DayZObfuscatorModel.Parser;
using DayZObfuscatorModel.PBO;
using DayZObfuscatorModel.PBO.Config.Parser;
using DayZObfuscatorModel.PBO.Config.Parser.Lexer;
using CSToolbox.IO;

namespace DayZObfuscatorModel.Analyzers
{
	public static class ProjectFolderAnalyzer
	{
		private static readonly ConfigParserErrorResolver _ConfigErrorResolver = new ConfigParserErrorResolver();
		private static readonly ConfigParser _ConfigParser = new ConfigParser();

		public static PBODescriptor? LoadPBO(string pathToRoot, bool includeHiddenDirectories = false, bool includeHiddenFiles = false)
		{
			if (!Directory.Exists(pathToRoot))
				throw new ArgumentException("Path should be a valid path to a directory.", nameof(pathToRoot));

			pathToRoot = Path.GetFullPath(pathToRoot);

			if (!File.Exists(pathToRoot + "\\config.cpp"))
				return null;

			var result = _ConfigParser.Parse( new ConfigLexer( new FileInputReader(pathToRoot + "\\config.cpp") ), _ConfigErrorResolver );

			var descriptor = new PBODescriptor(pathToRoot, result);

			foreach (PBOFile file in EnumerateFiles(pathToRoot, "", null, includeHiddenDirectories, includeHiddenFiles).Where(x => x.Filename != "config.cpp"))
				descriptor.Files.Add(file);

			return descriptor;
		}

		public static IEnumerable<PBODescriptor> Analyze(string path, bool includeHiddenDirectories = false, bool includeHiddenFiles = false)
		{
			if (!Directory.Exists(path))
				throw new ArgumentException("Path should be a valid path to a directory.", nameof(path));

			path = Path.GetFullPath(path);

			IEnumerable<PBODescriptor> descriptors = Enumerable.Empty<PBODescriptor>();

			IEnumerable<string> subdirs = Directory.EnumerateDirectories(path);
			if (!includeHiddenDirectories)
				subdirs = subdirs.Where(x => !PathEx.HasAttribute(x, FileAttributes.Hidden));

			foreach (string subdir in subdirs)
				descriptors = descriptors.Concat(Analyze(subdir));

			HashSet<string> ignoreFolders = descriptors.Select(x => x.DirectoryPath).ToHashSet();

			if (File.Exists(path + "\\config.cpp"))
			{
				var result = _ConfigParser.Parse( new ConfigLexer( new FileInputReader(path + "\\config.cpp") ), _ConfigErrorResolver);

				var descriptor = new PBODescriptor(path, result);

				foreach (PBOFile file in EnumerateFiles(path, "", ignoreFolders, includeHiddenDirectories, includeHiddenFiles).Where(x => x.Filename != "config.cpp"))
					descriptor.Files.Add(file);

				descriptors = descriptors.Concat(Enumerable.Repeat(descriptor, 1));
			}

			return descriptors;
		}

		private static IEnumerable<PBOFile> EnumerateFiles(string path, string relativePath, HashSet<string>? dirFilter = null, bool includeHiddenDirectories = false, bool includeHiddenFiles = false)
		{
			IEnumerable<string> files = Directory.EnumerateFiles(path);
			if (!includeHiddenFiles)
				files = files.Where(x => !PathEx.HasAttribute(x, FileAttributes.Hidden));

			foreach (string file in files)
				yield return new PBODriveFile(Path.GetFullPath(file), relativePath + (relativePath.Length > 0 ? "/" : "") + Path.GetFileName(file));

			IEnumerable<string> subdirs = Directory.EnumerateDirectories(path).Where(x => dirFilter?.Contains(x) != true);
			if (!includeHiddenDirectories)
				subdirs = subdirs.Where(x => !PathEx.HasAttribute(x, FileAttributes.Hidden));

			foreach (string subdir in subdirs)
				foreach (PBOFile file in EnumerateFiles( subdir, relativePath + Path.GetFileName(subdir) + "/", dirFilter ) )
					yield return file;

			yield break;
		}
	}
}
