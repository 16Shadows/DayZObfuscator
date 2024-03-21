namespace DayZObfuscatorModel.PBO
{
	public class PBODriveFile : PBOFile
	{
		/// <summary>
		/// Absolute path to the actual file which will be packed into PBO.
		/// </summary>
		public string AbsolutePath { get; }


		public PBODriveFile(string absolutePath, string pboPath) : base(pboPath)
		{
			ArgumentNullException.ThrowIfNull(absolutePath, nameof(absolutePath));
			
			if (!Path.IsPathFullyQualified(absolutePath))
				throw new ArgumentException($"{nameof(absolutePath)} ({absolutePath}) should be a fully qualified path.");

			AbsolutePath = absolutePath;
			
			try
			{
				FileInfo info = new FileInfo(AbsolutePath);
				DataSize = OriginalSize = (uint)info.Length;
			}
			catch { }
		}
	}
}
