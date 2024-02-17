using CSToolbox.Extensions;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigValueString : IEquatable<PBOConfigValueString>, IEquatable<string>
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
