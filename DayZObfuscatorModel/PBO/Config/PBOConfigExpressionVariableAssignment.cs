namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigExpressionVariableAssignment : PBOConfigExpressionBase
	{
		private object _Value;

		public object Value { get => _Value; set => _Value = value ?? throw new ArgumentNullException(nameof(value)); }

		public PBOConfigExpressionVariableAssignment(string identifier, object value) : base(identifier)
		{
			_Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		public override string ToString()
		{
			return $"{Identifier} = {Value};";
		}

		public override bool Equals(object? obj)
		{
			return base.Equals(obj) && obj is PBOConfigExpressionVariableAssignment other && Value.Equals(other.Value);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(base.GetHashCode(), Value);
		}
	}
}
