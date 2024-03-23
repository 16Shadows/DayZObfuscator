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

		public static PBODescriptor LoadPBO(string pathToRoot, bool includeHiddenDirectories = false, bool includeHiddenFiles = false)
		{
			if (!Directory.Exists(pathToRoot))
				throw new ArgumentException("Path should be a valid path to a directory.", nameof(pathToRoot));

			pathToRoot = Path.GetFullPath(pathToRoot);

			return new PBODescriptor(
				pathToRoot,
				EnumerateFiles(pathToRoot, "", includeHiddenDirectories, includeHiddenFiles)
			);
		}

		private static IEnumerable<PBOFile> EnumerateFiles(string path, string relativePath, bool includeHiddenDirectories = false, bool includeHiddenFiles = false)
		{
			//Enumerate all files in this directory
			IEnumerable<string> files = Directory.EnumerateFiles(path);
			if (!includeHiddenFiles)
				files = files.Where(x => !PathEx.HasAttribute(x, FileAttributes.Hidden));

			IEnumerable<PBOFile> result = files.Select(x => InstantiateFileDescriptor(x, relativePath));

			//Enumerate all files in child directories
			IEnumerable<string> subdirs = Directory.EnumerateDirectories(path);
			if (!includeHiddenDirectories)
				subdirs = subdirs.Where(x => !PathEx.HasAttribute(x, FileAttributes.Hidden));

			foreach (string subdir in subdirs)
				result = result.Concat(EnumerateFiles( subdir, relativePath + Path.GetFileName(subdir) + "/", includeHiddenDirectories, includeHiddenFiles ));

			return result;
		}

		private static PBOFile InstantiateFileDescriptor(string path, string relativePath)
		{
			path = Path.GetFullPath(path);
			relativePath = relativePath + (relativePath.Length > 0 ? "\\" : "") + Path.GetFileName(path);

			string name = Path.GetFileName(path);
			
			//For future
			//string extension = Path.GetExtension(name);

			if (name == "config.cpp")
				return new PBOConfigDescriptor(
					path,
					relativePath,
					_ConfigParser.Parse(new ConfigLexer(new FileInputReader(path)), _ConfigErrorResolver)
				);
			else
				return new PBODriveFile(path, relativePath);
		}
	}
}
