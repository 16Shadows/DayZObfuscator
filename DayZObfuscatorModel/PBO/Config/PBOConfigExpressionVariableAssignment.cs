namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigExpressionVariableAssignment : PBOConfigExpressionBase, IEquatable<PBOConfigExpressionVariableAssignment>
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
			return (obj is PBOConfigExpressionVariableAssignment var && Equals(var));
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(base.GetHashCode(), Value);
		}

		public bool Equals(PBOConfigExpressionVariableAssignment? other)
		{
			return Value.Equals(other?.Value);
		}
	}
}
