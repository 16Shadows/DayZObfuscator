using CommandLine;
using DayZObfuscatorModel.Analyzers;
using DayZObfuscatorModel.Parser;
using DayZObfuscatorModel.PBO;
using DayZObfuscatorModel.PBO.Config.Parser;
using DayZObfuscatorModel.PBO.Config.Parser.Lexer;

namespace DayZObfuscatorConsoleApp
{
	internal class Program
	{
		class BaseArguments
		{
			[Option('s', "source", Default = "", HelpText = "Path to the folder", MetaValue = "String", Required = true)]
			public string TargetDirectory { get; set; }

			[Option('r', "recursive", Default = false, HelpText = "If this flag is set, the command will be applied to all possible PBOs in the folder", Required = false)]
			public bool Recursive { get; set; }
		}

		[Verb("analyze", HelpText = "Detect all possible PBOs in the target folder")]
		class AnalyzeArgs : BaseArguments
		{
			[Option('e', "config-errors", Default = false, HelpText = "If this flag is set, a list of all errors found in the config file will be output.")]
			public bool DetectConfigErrors { get; set; }

			[Option('f', "files-list", Default = false, HelpText = "If this flag is set, a list of all files which are part of the PBO will be output.")]
			public bool OutputFilesList { get; set; }

			[Option('d', "detailed-files-list", Default = false, HelpText = "If this flag is set and -f is set, the file list will include extra data.")]
			public bool DetailedFileList { get; set; }
		}

		[Verb("build", HelpText = "Build pbo(s) in the target folder")]
		class BuildArgs : BaseArguments
		{

		}

		static void Main(string[] args)
		{
			Parser.Default.ParseArguments<AnalyzeArgs, BuildArgs>(args)
				.WithParsed<AnalyzeArgs>(Analyze)
				.WithParsed<BuildArgs>(Build);
		}

		static void Analyze(AnalyzeArgs args)
		{
			ArgumentNullException.ThrowIfNull(args);
			ArgumentNullException.ThrowIfNull(args.TargetDirectory);

			if (!Directory.Exists(args.TargetDirectory))
			{
				Console.WriteLine($"'{args.TargetDirectory}' is not a valid directory.");
				return;
			}

			if (args.Recursive)
			{
				IEnumerable<PBODescriptor> descriptors = ProjectFolderAnalyzer.Analyze(args.TargetDirectory);
				foreach (var descriptor in descriptors)
					OutputPBOInfo(descriptor, args);
			}
			else
			{
				PBODescriptor? descriptor = ProjectFolderAnalyzer.LoadPBO(args.TargetDirectory);
				if (descriptor == null)
				{
					Console.WriteLine("No PBO found directly in the target directory. Try using -recursive.");
					return;
				}

				OutputPBOInfo(descriptor, args);
			}
		}

		static void OutputPBOInfo(PBODescriptor descriptor, AnalyzeArgs args)
		{
			ArgumentNullException.ThrowIfNull(descriptor);
			ArgumentNullException.ThrowIfNull(args);

			Console.WriteLine($"Found PBO located in: {descriptor.DirectoryPath}");
			Console.WriteLine();	

			if (args.DetectConfigErrors)
			{
				if (descriptor.Config.Success)
					Console.WriteLine("No errors were detected in the config file.");
				else
				{
					Console.WriteLine("Errors in config.cpp:");
					foreach (var error in descriptor.Config.Errors)
						Console.WriteLine(FormatConfigError(error));
				}
				Console.WriteLine();
			}

			if (args.OutputFilesList)
			{
				Console.WriteLine("File list:");
				if (args.DetailedFileList)
				{
					Console.WriteLine(GetDetailedFileListHeader());
					foreach (PBOFile file in descriptor.Files)
						Console.WriteLine(FormatFileEntry(file, true));
				}
				else
				{
					foreach (PBOFile file in descriptor.Files)
						Console.WriteLine(FormatFileEntry(file, false));	
				}
				Console.WriteLine();
			}
		}

		static string GetDetailedFileListHeader()
		{
			return "PathInPBO - AbsolutePath - File Size";
		}

		static string FormatFileEntry(PBOFile file, bool detailed)
		{
			if (detailed)
				return $"{file.FullPathInPBO} - {file.AbsolutePath} - {file.DataSize}";
			else
				return file.FullPathInPBO;
		}

		static string FormatConfigError(ParserErrorBase<ConfigParserErrors> error)
		{
			switch (error.Message)
			{
				case ConfigParserErrors.UnexpectedToken:
					return $"Unexpected token '{error.ErroneousToken.TokenTrimmed}' at {FormatTokenLocation(error.ErroneousToken)}.";
				case ConfigParserErrors.ExpectedArrayIdentifier:
					return $"Expected array identifier, found '{error.ErroneousToken.TokenTrimmed}' at {FormatTokenLocation(error.ErroneousToken)}. Missing '[]'?";
				case ConfigParserErrors.ExpectedIdentifier:
					return $"Expected identifier, found '{error.ErroneousToken.TokenTrimmed}' at {FormatTokenLocation(error.ErroneousToken)}. Wrongly placed '[]' or incorrect identifier?";
				case ConfigParserErrors.ExpectedOperator:
					return $"Expected operator (=, +=, -=), found '{error.ErroneousToken.TokenTrimmed}' at {FormatTokenLocation(error.ErroneousToken)}.";
				case ConfigParserErrors.ExpectedSemicolumn:
					return $"Missing ';' at {FormatTokenLocation(error.ErroneousToken)}.";
				case ConfigParserErrors.ExpectedLeftCurlyBracket:
					return $"Expected '{{' but found '{error.ErroneousToken.TokenTrimmed}' at {FormatTokenLocation(error.ErroneousToken)}.";
				case ConfigParserErrors.ExpectedRightCurlyBracket:
					return $"Expected '}}' but found '{error.ErroneousToken.TokenTrimmed}' at {FormatTokenLocation(error.ErroneousToken)}.";
				case ConfigParserErrors.ExpectedCommaOrRightCurlyBracket:
					return $"Expected '}}' or ',' but found '{error.ErroneousToken.TokenTrimmed}' at {FormatTokenLocation(error.ErroneousToken)}.";
				case ConfigParserErrors.InvalidNumber:
					return $"'{error.ErroneousToken.TokenTrimmed}' is not a valid number at {FormatTokenLocation(error.ErroneousToken)}.";
				case ConfigParserErrors.ExpectedClassKeyword:
					return $"Expected 'class' but found '{error.ErroneousToken.TokenTrimmed}' at {FormatTokenLocation(error.ErroneousToken)}.";
				case ConfigParserErrors.BrokenString:
					return $"Missing '\"' at {FormatTokenLocation(error.ErroneousToken, true)}.";
				default:
					return $"Unknown error with token '{error.ErroneousToken.TokenTrimmed}' at {FormatTokenLocation(error.ErroneousToken)}.";
			}
		}

		static string FormatTokenLocation(LexerTokenBase token, bool atEndOfToken = false)
		{
			return $"line {token.Line} at position {(atEndOfToken ? token.IndexOnLine : token.IndexOnLine + token.Token.Length)}";
		}

		static void Build(BuildArgs args)
		{

		}
	}
}