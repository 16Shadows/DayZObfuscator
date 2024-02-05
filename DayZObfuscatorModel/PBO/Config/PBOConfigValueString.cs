namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigValueString
	{
		private string _Value;

		public string Value { get => _Value; set => _Value = value ?? throw new ArgumentNullException(nameof(value)); }

		public PBOConfigValueString(string value)
		{
			_Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		public override string ToString()
		{
			return $"\"{Value.Escape()}\"";
		}

		public override bool Equals(object? obj)
		{
			return obj is PBOConfigValueString other && Value.Equals(other.Value);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Value);
		}
	}
}
