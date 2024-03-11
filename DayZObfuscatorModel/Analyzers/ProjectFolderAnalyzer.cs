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

			IEnumerable<PBODriveFile> files = EnumerateFiles(pathToRoot, "", null, includeHiddenDirectories, includeHiddenFiles);

			var descriptor = new PBODescriptor(
										pathToRoot,
										files.Where(x => x.Filename.ToLower() == "config.cpp").Select(x => 
											new PBOConfigDescriptor(
												x.FullPathInPBO,
												_ConfigParser.Parse(
														new ConfigLexer( new FileInputReader(x.AbsolutePath) ),
														_ConfigErrorResolver
													)
											)
										)
								);

			foreach (PBOFile file in files.Where(x => x.Filename.ToLower() != "config.cpp"))
				descriptor.Files.Add(file);

			foreach (PBOFile file in descriptor.Configs)
				descriptor.Files.Add(file);

			return descriptor;
		}

		private static IEnumerable<PBODriveFile> EnumerateFiles(string path, string relativePath, HashSet<string>? dirFilter = null, bool includeHiddenDirectories = false, bool includeHiddenFiles = false)
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
				foreach (PBODriveFile file in EnumerateFiles( subdir, relativePath + Path.GetFileName(subdir) + "/", dirFilter ) )
					yield return file;

			yield break;
		}
	}
}
