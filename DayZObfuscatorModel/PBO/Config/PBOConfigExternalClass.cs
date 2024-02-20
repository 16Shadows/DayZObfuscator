namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigExternalClass : PBOConfigExpressionBase
	{
		public PBOConfigExternalClass(string identifier) : base(identifier)
		{
		}

		public override string ToString()
		{
			return $"class {Identifier};";
		}

		public override bool Equals(object? obj)
		{
			return base.Equals(obj) && obj is PBOConfigExternalClass other && other.Identifier == Identifier;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(base.GetHashCode(), "exclass");
		}
	}
}
