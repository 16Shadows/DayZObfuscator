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
				_Filename = value.ToLower();
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
				_PathInPBO = value.Replace('/', '\\').Trim().Trim('\\').ToLower();
			}
		}

		/// <summary>
		/// Full path the the file within the PBO, as it will be written into the header.
		/// Composed of <see cref="PathInPBO"/> and <see cref="Filename"/>.
		/// </summary>
		public string FullPathInPBO => $"{(PathInPBO?.Length > 0 ? $"{PathInPBO}\\" : "")}{Filename}";

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

		private readonly List<Func<Stream?, Stream?>> _FileContentMutations = new List<Func<Stream?, Stream?>>();
		private readonly List<Stream?> _FileContentStreams = new List<Stream?>();
		private Stream? _FileContent;
		private Stream? _FinalizedFileContent;

		/// <summary>
		/// The initial content which will be mutated.
		/// </summary>
		public Stream? FileContentSource
		{
			get => _FileContent;
			set
			{
				DisposeFileContent();
				_FileContent = value;
			}
		}

		/// <summary>
		/// The contents of the file after all mutations have been applied.
		/// </summary>
		public Stream? FileContent
		{
			get
			{
				if (_FinalizedFileContent != null)
					return _FinalizedFileContent;
				
				if (_FileContent == null)
					return null;

				_FileContent.Seek(0, SeekOrigin.Begin);

				Stream? output = _FileContent;
				foreach (Func<Stream?, Stream?> mutate in  _FileContentMutations)
					_FileContentStreams.Add(output = mutate(output));

				_FinalizedFileContent = output;

				return output;
			}
		}


		/// <summary>
		/// If this is set to true, the FileContent is assumed to be no longer modifiable.
		/// For example, it may be caused by compression.
		/// </summary>
		public bool FileContentSealed { get; private set; }


		public PBOFile(string pboPath)
		{
			ArgumentNullException.ThrowIfNull(pboPath, nameof(pboPath));

			_Filename = (Path.GetFileName(pboPath) ?? "").ToLower();

			if (Path.IsPathRooted(pboPath))
				throw new ArgumentException($"{nameof(pboPath)} ({pboPath}) should be a valid relative non-rooted path to a file.");

			_PathInPBO = Path.GetDirectoryName(pboPath)?.Replace('/', '\\').Trim().Trim('\\').ToLower() ?? "";

			TimeStamp = 0;
			Offset = 0;
			MimeType = MimeTypes.Uncompressed;
		}

		/// <summary>
		/// Ensures that the properties of this file are up to date
		/// </summary>
		public void ValidateProperties()
		{
			//Just make sure that mutations have been applied at least once
			_ = FileContent;
		}

		/// <summary>
		/// Adds a mutation to the content which will affect the content written to PBO
		/// </summary>
		/// <param name="mutator">
		/// The function to generate mutation.
		/// It takes a single argument - Stream (which may be null) - content at the current stage (with all previous mutations applied).
		/// It should return a Stream containing the mutated content.
		/// </param>
		/// <param name="sealContent">If set to true, sets <see cref="FileContentSealed"/> to true and prevents additiong of new mutations.</param>
		/// <exception cref="InvalidOperationException">Is thrown if <see cref="=FileContentSealed"/> is set to true.</exception>
		public void AddContentMutation(Func<Stream?, Stream?> mutator, bool sealContent)
		{
			if (FileContentSealed)
				throw new InvalidOperationException("File content is sealed, no mutations can be added.");

			_FileContentMutations.Add(mutator);

			FileContentSealed = sealContent;
		}

		/// <summary>
		/// Removes all mutations and sets <see cref="FileContentSealed"/> to false.
		/// </summary>
		public void ClearMutations()
		{
			DisposeFileContent();
			_FileContentMutations.Clear();
			FileContentSealed = false;
		}

		/// <summary>
		/// Resets <see cref="FileContent"/>.
		/// </summary>
		public void DisposeFileContent()
		{
			_FileContent?.Dispose();
			_FinalizedFileContent?.Dispose();

			_FinalizedFileContent = null;

			foreach (Stream? stream in _FileContentStreams)
				stream?.Dispose();

			_FileContentStreams.Clear();
		}
	}
}
