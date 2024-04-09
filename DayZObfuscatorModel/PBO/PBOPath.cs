namespace DayZObfuscatorModel.PBO
{
	public class PBOPath
	{
        public static readonly string PathSeparator = "\\";

		/// <summary>
        /// Converts an unescaped path to a standard form used by PBOPacker.
        /// The standard form means using only '\' as path separators, no separators or whitespaces at the beginning or the end of the path, all characters are lowercase.
        /// </summary>
        /// <param name="path">The unescaped path to convert to standard form</param>
        /// <returns>The standard form unescaped path.</returns>
        public static string ToStandardForm(string? path)
        {
            return path == null ? string.Empty : path.Replace('/', '\\')
                                                     .Trim()
                                                     .Trim('\\')
                                                     .ToLowerInvariant();
        }

        public static string Combine(params string[] components)
        {
            return ToStandardForm(string.Join(PathSeparator, components.Select(ToStandardForm)));
        }
	}
}
