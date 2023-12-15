namespace DayZObfuscatorModel.PBO
{
	public class PBOFile
	{
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

		public PBOFile(string absolutePath, string pboPath)
		{
			ArgumentNullException.ThrowIfNull(absolutePath, nameof(absolutePath));
			ArgumentNullException.ThrowIfNull(pboPath, nameof(pboPath));

			_Filename = Path.GetFileName(pboPath);

			if (!Path.IsPathFullyQualified(absolutePath))
				throw new ArgumentException($"{nameof(absolutePath)} ({absolutePath}) should be a fully qualified path.");
			else if (Path.IsPathRooted(pboPath) || _Filename == null)
				throw new ArgumentException($"{nameof(pboPath)} ({pboPath}) should be a valid relaitve non-rooted path to a file.");

			AbsolutePath = absolutePath;
			_PathInPBO = Path.GetDirectoryName(pboPath) ?? "";
		}
	}
}
