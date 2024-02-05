using DayZObfuscatorModel.PBO.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
		public PBOPackerErrors Pack(PBODescriptor pbo, string outputDirectory)
		{
			ArgumentNullException.ThrowIfNull(pbo);
			ArgumentNullException.ThrowIfNull(outputDirectory);

			outputDirectory = Path.GetFullPath(outputDirectory);
			Directory.CreateDirectory(outputDirectory);

			PBOConfigClass? modClass = pbo.Config
										  .Result
										  .Scopes
										  .Where(x => x.Identifier == "CfgMods")
										  .FirstOrDefault()
										  ?.Scopes
										  .Where(x => x.Variables.Any(x => x.Identifier == "type" && x.Value is PBOConfigValueString str && str.Value == "mod"))
										  .FirstOrDefault();

			if (modClass == null)
				return PBOPackerErrors.FailedToFindModClass;

			string outputFileName = $"{outputDirectory}\\{modClass.Identifier}.pbo";

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
				PBOConfigExpressionVariableAssignment dir = modClass.Variables.First(x => x.Identifier == "dir");
				if (dir == null)
					modClass.Expressions.Add(new PBOConfigExpressionVariableAssignment("dir", new PBOConfigValueString(modClass.Identifier)));
				else if (dir.Value is not PBOConfigValueString str)
					dir.Value = new PBOConfigValueString(modClass.Identifier);
				else if (str.Value != modClass.Identifier)
					str.Value = modClass.Identifier;
			}

			PBOWriter writer = new PBOWriter(outputFile);

			//Properties
			{
				writer.Write('\0');
				writer.Write(PBOFile.MimeTypes.Properties);
				writer.Write(0u);
				writer.Write(0u);
				writer.Write(0u);
				writer.Write(0u);

				//Dummy
				Dictionary<string, string> properties = new Dictionary<string, string>()
				{
					{ "prefix", modClass.Identifier}
				};

				foreach (var kvp in properties)
				{
					writer.Write(kvp.Key);
					writer.Write(kvp.Value);
				}

				writer.Write('\0');
			}

			//Preprocess config
			{

			}

			List<PBOFile> fileList = new List<PBOFile>(pbo.Files.Count);

			//Preprocess files
			{
				foreach (PBOFile file in pbo.Files)
				{
					fileList.Add(file);
					file.FileContent = File.OpenRead(file.AbsolutePath);
				}
			}

			byte[] configData = Encoding.UTF8.GetBytes(pbo.Config.Result.ToString());

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
				foreach (PBOFile file in fileList)
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
				foreach (PBOFile file in fileList)
				{
					if (file.FileContent == null)
						continue;
					writer.Write(file.FileContent);
				}
			}

			writer.Flush();			

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
