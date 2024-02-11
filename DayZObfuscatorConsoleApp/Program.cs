using CommandLine;
using DayZObfuscatorComponents;
using DayZObfuscatorModel.Analyzers;
using DayZObfuscatorModel.Parser;
using DayZObfuscatorModel.PBO;
using DayZObfuscatorModel.PBO.Config.Parser;
using DayZObfuscatorModel.PBO.Config.Parser.Lexer;
using DayZObfuscatorModel.PBO.Packer;
using System.Net.Http.Headers;

namespace DayZObfuscatorConsoleApp
{
	internal class Program
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		class BaseArguments
		{
			[Option('s', "source", Default = "", HelpText = "Path to the folder", MetaValue = "path", Required = true)]

			public string TargetDirectory { get; set; }

			[Option('r', "recursive", Default = false, HelpText = "If this flag is set, the command will be applied to all possible PBOs in the folder", Required = false)]
			public bool Recursive { get; set; }

			[Option("hidden-files", Default = false, HelpText = "If this flag is set, hidden files will be included in the PBO", Required = false)]
			public bool IncludeHiddenFiles { get; set; }

			[Option("hidden-dirs", Default = false, HelpText = "If this flag is set, hidden files will be included in the PBO", Required = false)]
			public bool IncludeHiddenDirectories { get; set; }
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
			[Option('o', "output", Default = "", HelpText = "Path to output directory", MetaValue = "path", Required = false)]
			public string OutputDirectory { get; set; }
			
			[Option('w', "warn", Default = false, HelpText = "All errors which can be recovered from will be treated as warnings", Required = false)]
			public bool ErrorsAsWarnings { get; set; }

			[Option('p', "prefix", Default = null, HelpText = "Overrides pbo's prefix. If multiple pbos are to be packed, their prefixes will be overriden.", Required = false)]
			public string? Prefix { get; set; }
		}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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
				IEnumerable<PBODescriptor> descriptors = ProjectFolderAnalyzer.Analyze(args.TargetDirectory, args.IncludeHiddenDirectories, args.IncludeHiddenFiles);
				foreach (var descriptor in descriptors)
				{
					OutputPBOInfo(descriptor, args);
					Console.WriteLine("-------");
				}
			}
			else
			{
				PBODescriptor? descriptor = ProjectFolderAnalyzer.LoadPBO(args.TargetDirectory, args.IncludeHiddenDirectories, args.IncludeHiddenFiles);
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
			
			if (args.DetectConfigErrors)
			{
				Console.WriteLine();
				if (descriptor.Config.Success)
					Console.WriteLine("No errors were detected in the config file.");
				else
				{
					Console.WriteLine("Errors in config.cpp:");
					foreach (var error in descriptor.Config.Errors)
						Console.WriteLine(FormatConfigError(error));
				}	
			}

			if (args.OutputFilesList)
			{
				Console.WriteLine();
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
			}
		}

		static string GetDetailedFileListHeader()
		{
			return "PathInPBO - AbsolutePath - File Size";
		}

		static string FormatFileEntry(PBOFile file, bool detailed)
		{
			if (detailed)
				return $"{file.FullPathInPBO} - {(file is PBODriveFile dFile ? dFile.AbsolutePath : "")} - {file.DataSize}";
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
			PBOPacker packer = new PBOPacker();

			//Configure packer here
			packer.Prefix = args.Prefix;

			packer.Components.Add(new PBOScriptFilenameMangler());

			if (args.Recursive)
			{
				IEnumerable<PBODescriptor> descriptors = ProjectFolderAnalyzer.Analyze(args.TargetDirectory, args.IncludeHiddenDirectories, args.IncludeHiddenFiles);
				foreach (var descriptor in descriptors)
					BuildPBO(descriptor, packer, args);
			}
			else
			{
				PBODescriptor? descriptor = ProjectFolderAnalyzer.LoadPBO(args.TargetDirectory, args.IncludeHiddenDirectories, args.IncludeHiddenFiles);
				if (descriptor == null)
				{
					Console.WriteLine("No PBO found directly in the target directory. Try using -recursive.");
					return;
				}

				BuildPBO(descriptor, packer, args);
			}
		}

		static void BuildPBO(PBODescriptor descriptor, PBOPacker packer, BuildArgs args)
		{
			if (descriptor.Config.Errors.Any())
			{
				Console.WriteLine("Errors in config.cpp:");
				foreach (var error in descriptor.Config.Errors)
					Console.WriteLine(FormatConfigError(error));

				if (!args.ErrorsAsWarnings)
				{
					Console.WriteLine("Aborting packing...");
					return;
				}
				else
					Console.WriteLine("-warn is set, ignoring config errors");
			}

			packer.Pack(descriptor, args.OutputDirectory);
		}
	}
}