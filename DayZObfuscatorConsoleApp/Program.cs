using CommandLine;
using CSToolbox.Logger;
using DayZObfuscatorModel.Analyzers;
using DayZObfuscatorModel.Parser;
using DayZObfuscatorModel.PBO;
using DayZObfuscatorModel.PBO.Config.Parser;
using DayZObfuscatorModel.PBO.Config.Parser.Lexer;
using DayZObfuscatorModel.PBO.Packer;
using Newtonsoft.Json;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DayZObfuscatorConsoleApp
{
	internal class Program
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		class BaseArguments
		{
			[Option('s', "source", Default = "", HelpText = "Path to the folder", MetaValue = "path", Required = true)]

			public string TargetDirectory { get; set; }

			[Option("hidden-files", Default = false, HelpText = "If this flag is set, hidden files will be included in the PBO", Required = false)]
			public bool IncludeHiddenFiles { get; set; }

			[Option("hidden-dirs", Default = false, HelpText = "If this flag is set, hidden files will be included in the PBO", Required = false)]
			public bool IncludeHiddenDirectories { get; set; }

			[Option('m', "modules", Default = null, HelpText = "Specifies path to the module configuration which allows loading extra PBOPackerComponents and configuring them.", Required = false)]
			public string? ModuleConfigurationPath { get; set; }

			[Option('l', "log", Default = null, HelpText = "Specifies path to a file into which logs will be written. If no file is specified, logs will be written to console window instead.", Required = false)]
			public string? LogPath { get; set; }

			[Option('x', "exclude", Separator = ';', HelpText = "Specifies a semicolumn-separated list of files to be excluded from PBO. Paths are specified relative to mod folder (e.g. 'script.c' will exclude file 'script.c' located in mod folder). Wildcards can be used to exclude patterns (e.g. '*.jpg' excludes all files with 'jpg' extension).")]
			public IEnumerable<string>? ExclusionPatterns { get; set; }
		}

		[Verb("analyze", HelpText = "Detect PBO in the target folder and output its info.")]
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
			[Option('o', "output", Default = ".", HelpText = "Path to output directory", MetaValue = "path", Required = false)]
			public string OutputDirectory { get; set; }
			
			[Option('w', "warn", Default = false, HelpText = "All errors which can be recovered from will be treated as warnings", Required = false)]
			public bool ErrorsAsWarnings { get; set; }

			[Option('p', "prefix", Default = null, HelpText = "Overrides pbo's prefix. If multiple pbos are to be packed, their prefixes will be overriden.", Required = false)]
			public string? Prefix { get; set; }

			[Option('b', "binarize", Default = false, HelpText = "If this flag is set, config.cpp will be binarized.", Required = false)]
			public bool BinarizeConfig { get; set; }
		}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		static void Main(string[] args)
		{
			Program prog = new Program();

			Parser.Default.ParseArguments<AnalyzeArgs, BuildArgs>(args)
				.WithParsed<AnalyzeArgs>(prog.Analyze)
				.WithParsed<BuildArgs>(prog.Build);
		}

		private AnalyzeArgs? AnalyzerArgs => BaseArgs as AnalyzeArgs;
		private BuildArgs? BuilderArgs => BaseArgs as BuildArgs;
		private BaseArguments? BaseArgs { get; set; }
		private LoggerBase? Logger { get; set; }
		private readonly List<PBOPackerComponent> Components = new List<PBOPackerComponent>();
		private static readonly Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>();

		void Initialize(BaseArguments args)
		{
			ArgumentNullException.ThrowIfNull(args);
			BaseArgs = args;

			Logger = BaseArgs.LogPath == null ? new ConsoleLogger() : new FileLogger(BaseArgs.LogPath);

			if (BaseArgs.ModuleConfigurationPath != null)
			{
				if (File.Exists(BaseArgs.ModuleConfigurationPath))
				{
					using StreamReader reader = File.OpenText(BaseArgs.ModuleConfigurationPath);
					using JsonReader jReader = new JsonTextReader(reader);
					JsonSerializer serializer = new JsonSerializer()
					{
						MissingMemberHandling = MissingMemberHandling.Error
					};
	
					try
					{
						var moduleConfig = serializer.Deserialize<ModuleConfigurationFile>(jReader);
						if (moduleConfig != null)
							LoadModules(moduleConfig);
						else
							Logger.WriteLine($"Failed to load module configuration file ({BaseArgs.ModuleConfigurationPath}). It may be empty.");
					}
					catch (Exception e)
					{
						Logger.WriteLine($"Failed to load module configuration file ({BaseArgs.ModuleConfigurationPath}).\nError: {e}.");
					}
				}
				else
					Logger.WriteLine($"Specified module configuration ({BaseArgs.ModuleConfigurationPath}) doesn't exist!");
			}

			if (BaseArgs.ExclusionPatterns != null)
			{
				/*
				 * Build regex patterns from provided patterns with wildcards
				 * Escape provided pattern so it is treated literally (no accidental regex expressions in it)
				 * Replace / and \ with patten matching either of them
				 * Replace wildcards with pattern matching anything
				 */
				BaseArgs.ExclusionPatterns = BaseArgs.ExclusionPatterns.Select(x => Regex.Escape(x.Trim())
														 .Replace(Regex.Escape("/"), "[/\\\\]")
														 .Replace(Regex.Escape("\\"), "[/\\\\]")
														 .Replace(Regex.Escape("*"), ".+")).ToArray();
			}
		}

		void LoadModules(ModuleConfigurationFile modules)
		{
			ArgumentNullException.ThrowIfNull(modules);

			Type[] RawPropertiesConstructorTypes = new Type[] { typeof(Dictionary<string, object>) };

			Assembly? assembly;
			foreach (ModuleConfiguration module in modules.Modules)
			{
				Logger?.WriteLine($"Loading module {module.ModuleName} from {module.AssemblyPath}...");
				try
				{
					string fullPath = Path.GetFullPath(module.AssemblyPath);
					if (!File.Exists(fullPath))
					{
						Logger?.WriteLine($"Failed to find assembly '{module.AssemblyPath}' for module {module.ModuleName}.");
						continue;
					}
					else if (!LoadedAssemblies.TryGetValue(fullPath, out assembly))
						LoadedAssemblies.Add(fullPath, assembly = Assembly.LoadFrom(fullPath));
				
					Type? type = assembly.ExportedTypes.FirstOrDefault(x => x.Name == module.ModuleName && x.IsAssignableTo(typeof(PBOPackerComponent)));
					if (type == null)
					{
						Logger?.WriteLine($"Failed to find assembly type {module.ModuleName} derived from {typeof(PBOPackerComponent)} in assembly '{module.AssemblyPath}'.");
						continue;
					}

					//Check if the type has a nested type called 'Propertes' (by-convention initialization)
					Type? propsType = type.GetNestedType("Properties", BindingFlags.Public);
					ConstructorInfo? constructor;
					if (propsType != null)
					{
						//Check if it also has a constructor which accepts such type
						constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, new Type[] { propsType });
						if (constructor != null)
						{
							try
							{
								object? propsInstance = Activator.CreateInstance(propsType, false);

								if (propsInstance != null)
								{
									if (module.Properties != null)
										PopulateTypeFromDictionary(propsInstance, module.Properties);
									
									Components.Add((PBOPackerComponent)constructor.Invoke(new object?[] { propsInstance }));
									Logger?.WriteLine($"Module {module.ModuleName} from {module.AssemblyPath} has been loaded.");
									continue;
								}
							}
							catch { /*Just fall back to other options.*/ }
						}
					}

					//Try to fall back to passing plain dictionary
					constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, RawPropertiesConstructorTypes);

					if (constructor != null)
					{
						Logger?.WriteLine($"Module {module.ModuleName} from {module.AssemblyPath} has been loaded.");
						Components.Add((PBOPackerComponent)constructor.Invoke(new object?[] { module.Properties }));
						continue;
					}

					//Fall back to default constructor
					constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, Type.EmptyTypes);

					if (constructor != null)
					{
						Logger?.WriteLine($"Module {module.ModuleName} from {module.AssemblyPath} has been loaded.");
						Components.Add((PBOPackerComponent)constructor.Invoke(null));
						continue;
					}
					

					Logger?.WriteLine($"Failed to a find valid constructor (default, accepting {typeof(Dictionary<string, object>).Name} or defining nested type 'Properties' and accepting it) for type {module.ModuleName} from assembly '{module.AssemblyPath}'.");
				}
				catch (Exception ex)
				{
					Logger?.WriteLine($"Failed to load module {module.ModuleName} from assembly '{module.AssemblyPath}'.\nError: {ex}");
				}
			}
		}

		void FilterPBOFiles(PBODescriptor descriptor)
		{
			ArgumentNullException.ThrowIfNull(descriptor);
			ArgumentNullException.ThrowIfNull(BaseArgs);

			if (BaseArgs.ExclusionPatterns == null)
				return;

			foreach (string pattern in BaseArgs.ExclusionPatterns)
			{
				Regex matcher = new Regex(pattern);
				for (int i = 0; i < descriptor.Files.Count; )
				{
					Match match = matcher.Match(descriptor.Files[i].FullPathInPBO);
					if (match.Success && match.Length == descriptor.Files[i].FullPathInPBO.Length)
						descriptor.Files.RemoveAt(i);
					else
						i++;
				}
			}
		}

		private static void PopulateTypeFromDictionary(object instance, Dictionary<string, object> values)
		{
			ArgumentNullException.ThrowIfNull(instance);
			ArgumentNullException.ThrowIfNull(values);

			Type t = instance.GetType();

			PropertyInfo? propInfo;
			foreach (KeyValuePair<string, object> prop in values)
			{
				propInfo = t.GetProperty(prop.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (propInfo == null || !propInfo.CanWrite)
					continue;
				try
				{
					propInfo.SetValue(instance, Convert.ChangeType(prop.Value, propInfo.PropertyType));
				}
				catch { /*Can't set this property, move on*/ }
			}
		}

		void Dispose()
		{
			Logger?.Dispose();
			Components.Clear();
			BaseArgs = null;
		}

		void Analyze(AnalyzeArgs args)
		{
			Initialize(args);
			ArgumentNullException.ThrowIfNull(AnalyzerArgs);
			ArgumentNullException.ThrowIfNull(AnalyzerArgs.TargetDirectory);
			
			if (!Directory.Exists(AnalyzerArgs.TargetDirectory))
			{
				Logger?.WriteLine($"'{AnalyzerArgs.TargetDirectory}' is not a valid directory.");
				return;
			}
			
			PBODescriptor descriptor = ProjectFolderAnalyzer.LoadPBO(AnalyzerArgs.TargetDirectory, AnalyzerArgs.IncludeHiddenDirectories, AnalyzerArgs.IncludeHiddenFiles);

			FilterPBOFiles(descriptor);
			OutputPBOInfo(descriptor);
			
			Dispose();
		}

		void OutputPBOInfo(PBODescriptor descriptor)
		{
			ArgumentNullException.ThrowIfNull(AnalyzerArgs);
			ArgumentNullException.ThrowIfNull(descriptor);

			Logger?.WriteLine($"Found PBO located in: {descriptor.DirectoryPath}");
			
			if (AnalyzerArgs.DetectConfigErrors)
			{
				foreach (PBOConfigDescriptor config in descriptor.Configs)
				{
					Logger?.WriteLine();
					Logger?.WriteLine($"Found a config at '{config.FullPathInPBO}'.");
					if (config.IsValid)
						Logger?.WriteLine("No errors were detected in the config file.");
					else
					{
						foreach (var error in config.Errors)
							Logger?.WriteLine(FormatConfigError(error));
					}
				}
			}

			if (AnalyzerArgs.OutputFilesList)
			{
				Logger?.WriteLine();
				Logger?.WriteLine("File list:");
				if (AnalyzerArgs.DetailedFileList)
				{
					Logger?.WriteLine(GetDetailedFileListHeader());
					foreach (PBOFile file in descriptor.Files)
						Logger?.WriteLine(FormatFileEntry(file, true));
				}
				else
				{
					foreach (PBOFile file in descriptor.Files)
						Logger?.WriteLine(FormatFileEntry(file, false));	
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
				return $"{file.FullPathInPBO} - {(file is PBODriveFile dFile ? dFile.AbsolutePath : "")} - {file.DataSize} bytes.";
			else
				return file.FullPathInPBO;
		}

		static string FormatConfigError(ParserErrorBase<ConfigParserErrors, ConfigToken> error)
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

		static string FormatTokenLocation(ConfigToken token, bool atEndOfToken = false)
		{
			return $"line {token.Line + 1} at position {((atEndOfToken ? token.IndexOnLine : token.IndexOnLine + token.Token.Length) + 1)}";
		}

		void Build(BuildArgs args)
		{
			Initialize(args);
			ArgumentNullException.ThrowIfNull(BuilderArgs);
			ArgumentNullException.ThrowIfNull(BuilderArgs.TargetDirectory);
			PBOPacker packer = new PBOPacker();

			//Configure packer here
			packer.Prefix = BuilderArgs.Prefix;
			packer.BinarizeConfig = BuilderArgs.BinarizeConfig;

			foreach (PBOPackerComponent comp in Components)
				packer.Components.Add(comp);

			PBODescriptor descriptor = ProjectFolderAnalyzer.LoadPBO(BuilderArgs.TargetDirectory, BuilderArgs.IncludeHiddenDirectories, BuilderArgs.IncludeHiddenFiles);

			FilterPBOFiles(descriptor);
			BuildPBO(descriptor, packer);

			Dispose();
		}

		void BuildPBO(PBODescriptor descriptor, PBOPacker packer)
		{
			ArgumentNullException.ThrowIfNull(BuilderArgs);

			if (descriptor.Configs.Any(x => x.Errors.Any()))
			{
				foreach (PBOConfigDescriptor config in descriptor.Configs.Where(x => x.Errors.Any()))
				{
					Logger?.WriteLine($"Errors in config at '{config.FullPathInPBO}'.");
					foreach (var error in config.Errors)
						Logger?.WriteLine(FormatConfigError(error));
					Logger?.WriteLine();
				}

				if (!BuilderArgs.ErrorsAsWarnings)
				{
					Logger?.WriteLine("Aborting packing...");
					return;
				}
				else
					Logger?.WriteLine("-warn is set, ignoring config errors");
			}

			switch (packer.Pack(descriptor, BuilderArgs.OutputDirectory))
			{
				case PBOPackerErrors.Success:
					Logger?.WriteLine($"Packing completed.");
					break;
				case PBOPackerErrors.AccessToOutputDenied:
					Logger?.WriteLine($"Failed to pack PBO - access to output file has been denied.");
					break;
			}
		}
	}
}