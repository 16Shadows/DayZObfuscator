using DayZObfuscatorModel.Parser;
using DayZObfuscatorModel.PBO.Config;
using DayZObfuscatorModel.PBO.Config.Parser;

namespace DayZObfuscatorModel.PBO
{
	public class PBOConfigDescriptor : PBOFile
	{
		public PBOConfig Config { get; }

		public IEnumerable<ParserErrorBase<ConfigParserErrors>>	Errors { get; }

		public bool IsValid => Errors.Any();

		public PBOConfigDescriptor(string pathInPBO, ParseResult<PBOConfig, ParserErrorBase<ConfigParserErrors>> parsedConfig) : base(pathInPBO)
		{
			ArgumentNullException.ThrowIfNull(parsedConfig);

			Config = parsedConfig.Result;
			Errors = parsedConfig.Errors;
		}
	}
}
