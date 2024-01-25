using System.Data;
using DayZObfuscatorModel.PBO.Packer;

namespace DayZObfuscatorModel.PBO
{
	public class PBOFile
	{
		public static class MimeTypes
		{
			public const uint Uncompressed = 0x0;
			public const uint Properties = 0x73726556;
			public const uint Compressed = 0x73727043;
		}


		private string _Filename;
		public string Filename
		{
			get => _Filename;
			set
			{
				ArgumentNullException.ThrowIfNull(value, nameof(Filename));
				_Filename = value;
			}
		}

		private string _PathInPBO;
		public string PathInPBO
		{
			get => _PathInPBO;
			set
			{
				ArgumentNullException.ThrowIfNull(value, nameof(PathInPBO));
				_PathInPBO = value;
			}
		}

		public string FullPathInPBO => Path.Combine(PathInPBO, Filename);

		public string AbsolutePath { get; }

		/// <summary>
		/// Mime type if this file. Default constants are available in <see cref="MimeTypes"/>.
		/// </summary>
		public uint MimeType { get; set; }
		/// <summary>
		/// If <see cref="MimeType"/> is <see cref="MimeTypes.Uncompressed"/>, 0 or same as <see cref="DataSize"/>.
		/// If <see cref="MimeType"/> is <see cref="MimeTypes.Compressed"/>, size of file after unpacking.
		/// </summary>
		public uint OriginalSize { get; set; }
		/// <summary>
		/// Unused by default.
		/// </summary>
		public uint Offset { get; set; }
		/// <summary>
		/// File timestamp. May be 0.
		/// </summary>
		public uint TimeStamp { get; set; }
		/// <summary>
		/// Non-packed size of the file.
		/// </summary>
		public uint DataSize { get; set; }
		
		/// <summary>
		/// The content of this file which will be written to the pbo when the file will be packed.
		/// The value is only used during the packing process and is initialy set by <see cref="PBOPacker"/>.
		/// The value may be null outside of pbo packing.
		/// </summary>
		public Stream? FileContent { get; set; }


		public PBOFile(string absolutePath, string pboPath)
		{
			ArgumentNullException.ThrowIfNull(absolutePath, nameof(absolutePath));
			ArgumentNullException.ThrowIfNull(pboPath, nameof(pboPath));

			_Filename = Path.GetFileName(pboPath);

			if (!Path.IsPathFullyQualified(absolutePath))
				throw new ArgumentException($"{nameof(absolutePath)} ({absolutePath}) should be a fully qualified path.");
			else if (Path.IsPathRooted(pboPath) || _Filename == null)
				throw new ArgumentException($"{nameof(pboPath)} ({pboPath}) should be a valid relative non-rooted path to a file.");

			AbsolutePath = absolutePath;
			_PathInPBO = Path.GetDirectoryName(pboPath) ?? "";

			TimeStamp = 0;
			Offset = 0;
			MimeType = MimeTypes.Uncompressed;

			try
			{
				FileInfo info = new FileInfo(AbsolutePath);
				DataSize = OriginalSize = (uint)info.Length;
			}
			catch { }
		}
	}
}
