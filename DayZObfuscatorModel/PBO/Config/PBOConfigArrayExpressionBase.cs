namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigArrayExpressionBase : PBOConfigExpressionBase
	{
		private IList<object> _Value;

		public IList<object> Value { get => _Value; set => _Value = value ?? throw new ArgumentNullException(nameof(value)); }

		public PBOConfigArrayExpressionBase(string identifier, IList<object> value) : base(identifier)
		{
			_Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		public override bool Equals(object? obj)
		{
			return obj is PBOConfigArrayExpressionBase other && Value.SequenceEqual(other.Value);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(base.GetHashCode(), "array");
		}
	}
}
