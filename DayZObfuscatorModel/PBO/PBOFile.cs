using DayZObfuscatorModel.PBO.Packer;

namespace DayZObfuscatorModel.PBO
{
	public class PBOFile
	{
		public static class MimeTypes
		{
			public const uint Uncompressed = 0x0;
			public const uint Properties = 0x56657273;
			public const uint Compressed = 0x43707273;
		}


		private string _Filename;
		/// <summary>
		/// The name if the file within the PBO.
		/// </summary>
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
		/// <summary>
		/// Path to the file within the PBO.
		/// </summary>
		public string PathInPBO
		{
			get => _PathInPBO;
			set
			{
				ArgumentNullException.ThrowIfNull(value, nameof(PathInPBO));
				_PathInPBO = value;
			}
		}

		/// <summary>
		/// Full path the the file within the PBO, as it will be written into the header.
		/// Composed of <see cref="PathInPBO"/> and <see cref="Filename"/>.
		/// </summary>
		public string FullPathInPBO => $"{PathInPBO}/{Filename}";

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
		/// Size of the file in the PBO.
		/// </summary>
		public uint DataSize { get; set; }
		
		/// <summary>
		/// The content of this file which will be written to the pbo when the file will be packed.
		/// The value is only used during the packing process and is initialy set by <see cref="PBOPacker"/>.
		/// The value may be null outside of pbo packing.
		/// </summary>
		public Stream? FileContent { get; set; }

		/// <summary>
		/// If this is set to true, the FileContent is assumed to be no longer modifiable.
		/// For example, it may be caused by compression.
		/// </summary>
		public bool FileContentSealed { get; set; }


		public PBOFile(string pboPath)
		{
			ArgumentNullException.ThrowIfNull(pboPath, nameof(pboPath));

			_Filename = Path.GetFileName(pboPath) ?? "";

			if (Path.IsPathRooted(pboPath))
				throw new ArgumentException($"{nameof(pboPath)} ({pboPath}) should be a valid relative non-rooted path to a file.");

			_PathInPBO = Path.GetDirectoryName(pboPath) ?? "";

			TimeStamp = 0;
			Offset = 0;
			MimeType = MimeTypes.Uncompressed;
		}
	}
}
