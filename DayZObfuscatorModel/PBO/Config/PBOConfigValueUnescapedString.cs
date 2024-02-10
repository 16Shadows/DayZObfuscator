namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigValueUnescapedString : IEquatable<PBOConfigValueString>, IEquatable<string>
	{
		private static readonly Dictionary<string, string> _EscapeTable = new Dictionary<string, string>() { {@"""", @"\"""} };

		private string _Value;

		public string Value { get => _Value; set => _Value = value ?? throw new ArgumentNullException(nameof(value)); }

		public PBOConfigValueUnescapedString(string value)
		{
			_Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		public override string ToString()
		{
			return $"\"{Value.Escape(_EscapeTable)}\"";
		}

		public override bool Equals(object? obj)
		{
			return (obj is string str && Equals(str)) ||
				   (obj is PBOConfigValueString other && Equals(other));
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Value);
		}

		public bool Equals(PBOConfigValueString? other)
		{
			return Value.Equals(other?.Value);
		}

		public bool Equals(string? other)
		{
			return Value.Equals(other);
		}
	}
}
