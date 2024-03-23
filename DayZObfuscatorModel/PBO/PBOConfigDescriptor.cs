using DayZObfuscatorModel.Parser;
using DayZObfuscatorModel.PBO.Config;
using DayZObfuscatorModel.PBO.Config.Parser;
using DayZObfuscatorModel.PBO.Config.Parser.Lexer;
using System.Text;
using CSToolbox.Extensions;
using DayZObfuscatorModel.PBO.Packer;

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

			_ConvertToNonBinarized = (cfg, output) => Encoding.ASCII.GetBytes(cfg.ToString().AsSpan(), output);
			_ConvertToBinarized = (cfg, output) =>
			{
				using PBOWriter configWriter = new PBOWriter(output, true);
				cfg.Binarize(configWriter);
			};
		}

		private Action<PBOConfig, Stream> _ConvertToNonBinarized;
		public Action<PBOConfig, Stream> ConvertToNonBinarized
		{
			get => _ConvertToNonBinarized;
			set => _ConvertToNonBinarized = value ?? throw new ArgumentNullException();
		}

		private Action<PBOConfig, Stream> _ConvertToBinarized;
		public Action<PBOConfig, Stream> ConvertToBinarized
		{
			get => _ConvertToBinarized;
			set => _ConvertToBinarized = value ?? throw new ArgumentNullException();
		}

		public void GetContent(Stream output)
		{
			ConvertToNonBinarized(Config, output);
		}

		public void GetBinarizedContent(Stream output)
		{
			ConvertToBinarized(Config, output);
		}
	}
}
