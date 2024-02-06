namespace DayZObfuscatorModel
{
	public static class PathEx
	{
		public static bool HasAttribute(string path, FileAttributes attribute)
		{
			FileSystemInfo info;
			if (File.Exists(path))
				info = new FileInfo(path);
			else if (Directory.Exists(path))
				info = new DirectoryInfo(path);
			else
				throw new FileNotFoundException();

			return info.Attributes.HasFlag(attribute);
		}
	}
}
