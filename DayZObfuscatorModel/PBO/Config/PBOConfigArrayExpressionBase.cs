namespace DayZObfuscatorModel.PBO.Config
{
	public abstract class PBOConfigArrayExpressionBase : PBOConfigExpressionBase
	{
		private PBOConfigArray _Value;

		public PBOConfigArray Value { get => _Value; set => _Value = value ?? throw new ArgumentNullException(nameof(value)); }

		public PBOConfigArrayExpressionBase(string identifier, PBOConfigArray value) : base(identifier)
		{
			_Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		public override bool Equals(object? obj)
		{
			return base.Equals(obj) && obj is PBOConfigArrayExpressionBase other && Value.Equals(other.Value);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(base.GetHashCode(), _Value.GetHashCode());
		}
	}
}
