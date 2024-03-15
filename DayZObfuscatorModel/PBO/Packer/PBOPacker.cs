using DayZObfuscatorModel.PBO.Config;
using CSToolbox.Extensions;
using System.Security.Cryptography;
using System.Text;
using CSToolbox.IO;

namespace DayZObfuscatorModel.PBO.Packer
{
	public enum PBOPackerErrors
	{
		Success,
		AccessToOutputDenied
	}

	public class PBOPacker
	{
		/// <summary>
		/// Ordered collection of components which will be used when packing the PBO.
		/// If a component appears after another in the list, it may overwrite the effects of the first component.
		/// </summary>
		public IList<PBOPackerComponent> Components { get; } = new List<PBOPackerComponent>();

		/// <summary>
		/// If set, overrides the pbo's prefix with the one specified
		/// </summary>
		public string? Prefix { get; set; }

		/// <summary>
		/// If set, config will be binarized
		/// </summary>
		public bool BinarizeConfig { get; set; }

		private readonly Dictionary<string, string> Properties = new Dictionary<string, string>();

		public PBOPackerErrors Pack(PBODescriptor pbo, string outputDirectory)
		{
			ArgumentNullException.ThrowIfNull(pbo);
			ArgumentException.ThrowIfNullOrEmpty(outputDirectory);

			Properties.Clear();

			outputDirectory = Path.GetFullPath(outputDirectory);
			Directory.CreateDirectory(outputDirectory);

			string? prefix = Prefix;
			
			PBOConfigDescriptor? rootConfig = pbo.RootConfig;
			if (prefix == null)
			{
				PBODriveFile? prefixFile = pbo.Files.FirstOrDefault(x => x.Filename.ToLower() == "$prefix$") as PBODriveFile;
				PBOConfigClass? patchesClass = rootConfig?.Config
										  .Classes
										  .FirstOrDefault(x => x.Identifier == "CfgPatches")
										  ?.Classes
										  .FirstOrDefault();
				
				if (prefixFile != null)
				{
					prefix = File.ReadAllText(prefixFile.AbsolutePath);
					pbo.Files.Remove(prefixFile);
				}
				else if (patchesClass != null)
					prefix = patchesClass.Identifier;
				else
					prefix = "UnnamedPBO";
			}

			FileStream outputFile;
			try
			{
				outputFile = new FileStream(Path.Combine(outputDirectory, $"{prefix.Replace(Path.GetInvalidFileNameChars(), '_')}.pbo"), FileMode.Create);
			}
			catch
			{
				return PBOPackerErrors.AccessToOutputDenied;
			}

			PBOWriter writer = new PBOWriter(outputFile);

			//Initialize properties
			Properties.Add("prefix", prefix);

			//Open handles to actual files
			foreach (PBODriveFile file in pbo.Files.Where(x => x is PBODriveFile).Cast<PBODriveFile>())
			{
				Stream content = File.OpenRead(file.AbsolutePath);
				file.FileContentSource = content;
				file.DataSize = file.OriginalSize = (uint)content.Length;
			}

			//Initialize configs' data
			foreach (PBOConfigDescriptor config in pbo.Configs)
				config.Filename = $"config.{(BinarizeConfig ? "bin" : "cpp")}";

			foreach (PBOPackerComponent component in Components)
				component.Apply(pbo, Properties);

			//Properties
			{
				writer.Write('\0');
				writer.Write(PBOFile.MimeTypes.Properties);
				writer.Write(0u);
				writer.Write(0u);
				writer.Write(0u);
				writer.Write(0u);

				foreach (var kvp in Properties)
				{
					writer.Write(kvp.Key);
					writer.Write(kvp.Value);
				}

				writer.Write('\0');
			}

			//Initialize config's content source after configs have been finalized
			{
				foreach (PBOConfigDescriptor config in pbo.Configs)
				{
					TempFileStream configContent = new TempFileStream();

					if (BinarizeConfig)
					{
						using PBOWriter configWriter = new PBOWriter(configContent, true);
						config.Config.Binarize(configWriter);
					}
					else
						Encoding.ASCII.GetBytes(config.Config.ToString().AsSpan(), configContent);
					
					configContent.Position = 0;
					configContent.Flush();
					config.FileContentSource = configContent;
					config.OriginalSize = config.DataSize = (uint)configContent.Length;
				}
			}
			
			//Write files headers
			{
				foreach (PBOFile file in pbo.Files)
				{
					file.ValidateProperties();
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

			//Writes files' contents
			{
				foreach (PBOFile file in pbo.Files)
				{
					Stream? content = file.FileContent;
					if (content == null)
						continue;
					writer.Write(content);
					file.DisposeFileContent();
					file.ClearMutations();
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
