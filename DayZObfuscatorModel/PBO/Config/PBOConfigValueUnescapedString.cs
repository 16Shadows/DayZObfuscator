namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigValueUnescapedString : PBOConfigValueString
	{
		private static readonly Dictionary<string, string> _EscapeTable = new Dictionary<string, string>() { {@"""", @"\"""} };

		public PBOConfigValueUnescapedString(string value) : base(value) {}

		public override string ToString()
		{
			return $"\"{Value.Escape(_EscapeTable)}\"";
		}
	}
}
