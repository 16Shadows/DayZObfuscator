using DayZObfuscatorModel.PBO.Config;
using System.Security.Cryptography;
using System.Text;

namespace DayZObfuscatorModel.PBO.Packer
{
	public enum PBOPackerErrors
	{
		Success,
		AccessToOutputDenied,
		FailedToFindModClass
	}

	public class PBOPacker
	{
		public class InfoProvider
		{
			private readonly PBOPacker Packer;

			public InfoProvider(PBOPacker packer)
			{
				Packer = packer;
			}

			/// <summary>
			/// This property contains the PBO's properties and is only available in <see cref="PBOPackerComponent.ProcessConfig(PBOConfig, InfoProvider)"/> and <see cref="PBOPackerComponent.ProcessFile(PBOFile, InfoProvider)"/>.
			/// </summary>
			public IDictionary<string, string>? Properties
			{
				get
				{
					return Packer.CurrentStep <= PBOPackerStep.Properties ?
						   null :
						   Packer.Properties;
				}
			}

			/// <summary>
			/// This property contains the PBO's properties and is only available in <see cref="PBOPackerComponent.ProcessFile(PBOFile, InfoProvider)"/>.
			/// </summary>
			public PBOConfig? Config
			{
				get
				{
					return Packer.CurrentStep <= PBOPackerStep.Config ?
						   null :
						   Packer.Config;
				}
			}
		}


		/// <summary>
		/// Ordered collection of components which will be used when packing the PBO.
		/// If a component appears after another in the list, it may overwrite the effects of the first component.
		/// </summary>
		public IList<PBOPackerComponent> Components { get; } = new List<PBOPackerComponent>();

		/// <summary>
		/// If set, overrides the pbo's prefix with the one specified
		/// </summary>
		public string? Prefix { get; set; }


		//Internal data used by InfoProvider
		private enum PBOPackerStep
		{
			Initialization = 0,
			Properties = 1,
			Config = 2,
			Files = 3,
			WritingOutput = 4
		}

		private PBOPackerStep CurrentStep { get; set; }
		private readonly Dictionary<string, string> Properties = new Dictionary<string, string>();
		private PBOConfig? Config;

		public PBOPackerErrors Pack(PBODescriptor pbo, string outputDirectory)
		{
			ArgumentNullException.ThrowIfNull(pbo);
			ArgumentNullException.ThrowIfNull(outputDirectory);

			CurrentStep = PBOPackerStep.Initialization;

			Properties.Clear();
			Config = pbo.Config.Result;			

			outputDirectory = Path.GetFullPath(outputDirectory);
			Directory.CreateDirectory(outputDirectory);

			PBOConfigClass? patchesClass = pbo.Config
										  .Result
										  .Classes
										  .FirstOrDefault(x => x.Identifier == "CfgPatches")
										  ?.Classes
										  .FirstOrDefault();

			if (patchesClass == null)
				return PBOPackerErrors.FailedToFindModClass;

			string outputFileName = $"{outputDirectory}\\{patchesClass.Identifier}.pbo";

			FileStream outputFile;
			try
			{
				outputFile = new FileStream(outputFileName, FileMode.Create);
			}
			catch
			{
				return PBOPackerErrors.AccessToOutputDenied;
			}

			//Fix possible inconsistencies in config
			{
				PBOConfigClass? modClass = pbo.Config
										  .Result
										  .Classes
										  .FirstOrDefault(x => x.Identifier == "CfgMods")
										  ?.Classes
										  .FirstOrDefault(x => 
											x.Variables
											.Any(x => x.Identifier == "type" && x.Value.Equals("mod")));

				if (modClass != null)
				{
					PBOConfigExpressionVariableAssignment? dir = modClass.Variables.FirstOrDefault(x => x.Identifier == "dir");
					if (dir == null)
						modClass.Expressions.Add(new PBOConfigExpressionVariableAssignment("dir", new PBOConfigValueString(modClass.Identifier)));
					else if (dir.Value is not PBOConfigValueString str)
						dir.Value = new PBOConfigValueString(modClass.Identifier);
					else if (str.Value != modClass.Identifier)
						str.Value = modClass.Identifier;
				}				
			}

			foreach (PBOPackerComponent comp in Components)
				comp.ResetState();

			PBOWriter writer = new PBOWriter(outputFile);
			InfoProvider provider = new InfoProvider(this);

			//Properties
			{
				CurrentStep = PBOPackerStep.Properties;

				writer.Write('\0');
				writer.Write(PBOFile.MimeTypes.Properties);
				writer.Write(0u);
				writer.Write(0u);
				writer.Write(0u);
				writer.Write(0u);

				Properties.Add("prefix", Prefix ?? patchesClass.Identifier);

				foreach (PBOPackerComponent comp in Components)
					comp.SetProperties(Properties);

				foreach (var kvp in Properties)
				{
					writer.Write(kvp.Key);
					writer.Write(kvp.Value);
				}

				writer.Write('\0');
			}

			//Preprocess config
			{
				CurrentStep = PBOPackerStep.Config;
				foreach (PBOPackerComponent comp in Components)
					comp.ProcessConfig(pbo.Config.Result, provider);
			}

			byte[] configData = Encoding.UTF8.GetBytes(pbo.Config.Result.ToString());

			//Preprocess files
			{
				CurrentStep = PBOPackerStep.Files;
				foreach (PBODriveFile file in pbo.Files.Where(x => x is PBODriveFile).Cast<PBODriveFile>())
				{
					file.FileContent = File.OpenRead(file.AbsolutePath);
					file.DataSize = file.OriginalSize = (uint)file.FileContent.Length;
				}

				foreach (PBOPackerComponent comp in Components)
					comp.ProcessFiles(pbo.Files, provider);
			}

			CurrentStep = PBOPackerStep.WritingOutput;

			//Write config header
			{
				writer.Write("config.cpp");
				writer.Write(PBOFile.MimeTypes.Uncompressed);
				writer.Write(0u);
				writer.Write(0u);
				writer.Write(0u);
				writer.Write((uint)configData.Length);
			}

			
			//Write files headers
			{
				foreach (PBOFile file in pbo.Files)
				{
					if (file.FileContent == null)
						continue;
					writer.Write(file.FullPathInPBO);
					writer.Write(file.MimeType);
					writer.Write(file.OriginalSize);
					writer.Write(file.Offset);
					writer.Write(file.TimeStamp);
					writer.Write(file.DataSize);
				}
			}

			//Write end of header
			{
				writer.Write('\0');
				writer.Write(PBOFile.MimeTypes.Uncompressed);
				writer.Write(0u);
				writer.Write(0u);
				writer.Write(0u);
				writer.Write(0u);
			}

			//Write config content
			{
				writer.Write(configData);
			}

			//Writes files' contents
			{
				foreach (PBOFile file in pbo.Files)
				{
					if (file.FileContent == null)
						continue;
					writer.Write(file.FileContent);
				}
			}

			writer.Flush();			

			//Write hashsum
			outputFile.Seek(0, SeekOrigin.Begin);
			using SHA1 sha1 = SHA1.Create();
			byte[] hash = sha1.ComputeHash(outputFile);
			outputFile.WriteByte(0);
			outputFile.Write(hash, 0, 20);
			
			writer.Dispose();
			outputFile.Dispose();

			return PBOPackerErrors.Success;
		}
	}
}
