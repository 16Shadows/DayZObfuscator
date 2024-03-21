using DayZObfuscatorModel.Parser;
using DayZObfuscatorModel.PBO.Config;
using DayZObfuscatorModel.PBO.Config.Parser;
using DayZObfuscatorModel.PBO.Config.Parser.Lexer;

namespace DayZObfuscatorModel.PBO
{
	public class PBOConfigDescriptor : PBODriveFile
	{
		public PBOConfig Config { get; }

		public IEnumerable<ParserErrorBase<ConfigParserErrors, ConfigToken>> Errors { get; }

		public bool IsValid => !Errors.Any();

		public PBOConfigDescriptor(string absolutePath, string pathInPBO, ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors, ConfigToken>> parsedConfig) : base(absolutePath, pathInPBO)
		{
			ArgumentNullException.ThrowIfNull(parsedConfig);

			Config = parsedConfig.Result;
			Errors = parsedConfig.Errors;
		}
	}
}
