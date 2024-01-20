using DayZObfuscatorModel.Parser;
using DayZObfuscatorModel.PBO;
using DayZObfuscatorModel.PBO.Config.Parser;
using DayZObfuscatorModel.PBO.Config.Parser.Lexer;
using System.IO;

namespace DayZObfuscatorModel.Analyzers
{
	public static class ProjectFolderAnalyzer
	{
		private static readonly ConfigParser _ConfigParser = new ConfigParser( new ConfigParserErrorResolver() );

		public static PBODescriptor LoadPBO(string pathToRoot)
		{
			if (!Directory.Exists(pathToRoot))
				throw new ArgumentException("Path should be a valid path to a directory.", nameof(pathToRoot));

			pathToRoot = Path.GetFullPath(pathToRoot);

			if (!File.Exists(pathToRoot + "\\config.cpp"))
				throw new ArgumentException("Target directory doesn't contain config.cpp - not a valid PBO directory", nameof(pathToRoot));

			var result = _ConfigParser.Parse( new ConfigLexer( new FileInputReader(pathToRoot + "\\config.cpp") ) );

			var descriptor = new PBODescriptor(pathToRoot, result);

			foreach (PBOFile file in EnumerateFiles(pathToRoot, "").Where(x => x.Filename != "config.cpp"))
				descriptor.Files.Add(file);

			return descriptor;
		}

		public static IEnumerable<PBODescriptor> Analyze(string path)
		{
			if (!Directory.Exists(path))
				throw new ArgumentException("Path should be a valid path to a directory.", nameof(path));

			path = Path.GetFullPath(path);

			IEnumerable<PBODescriptor> descriptors = Enumerable.Empty<PBODescriptor>();

			foreach (string subdir in Directory.EnumerateDirectories(path))
				descriptors = descriptors.Concat(Analyze(subdir));

			HashSet<string> ignoreFolders = descriptors.Select(x => x.DirectoryPath).ToHashSet();

			if (File.Exists(path + "\\config.cpp"))
			{
				var result = _ConfigParser.Parse( new ConfigLexer( new FileInputReader(path + "\\config.cpp") ) );

				var descriptor = new PBODescriptor(path, result);

				foreach (PBOFile file in EnumerateFiles(path, "", ignoreFolders).Where(x => x.Filename != "config.cpp"))
					descriptor.Files.Add(file);

				descriptors = Enumerable.Repeat(descriptor, 1);
			}


			return descriptors;
		}

		private static IEnumerable<PBOFile> EnumerateFiles(string path, string relativePath, HashSet<string>? dirFilter = null)
		{
			foreach (string file in Directory.EnumerateFiles(path))
				yield return new PBOFile(Path.GetFullPath(file), relativePath + (relativePath.Length > 0 ? "/" : "") + Path.GetFileName(file));

			foreach (string subdir in Directory.EnumerateDirectories(path).Where(x => dirFilter?.Contains(x) != true))
				foreach (PBOFile file in EnumerateFiles( subdir, relativePath + Path.GetFileName(subdir), dirFilter ) )
					yield return file;

			yield break;
		}
	}
}
