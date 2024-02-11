using DayZObfuscatorModel.Parser;
using DayZObfuscatorModel.PBO.Config;
using DayZObfuscatorModel.PBO.Config.Parser;

namespace DayZObfuscatorModel.PBO
{
    public class PBODescriptor
	{
		public IList<PBOFile> Files { get; } = new List<PBOFile>();

		public ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors>> Config { get; }

		public string DirectoryPath { get; }

		public PBODescriptor(string directoryPath, ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors>> config)
		{
			ArgumentNullException.ThrowIfNull(config);
			ArgumentNullException.ThrowIfNull(directoryPath);

			if (!Directory.Exists(directoryPath))
				throw new ArgumentException("Path should be a valid path to a directory.", nameof(directoryPath));

			DirectoryPath = directoryPath;
			Config = config;
		}
	}
}
