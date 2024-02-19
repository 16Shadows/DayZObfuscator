namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigExpressionDelete : PBOConfigExpressionBase
	{
		public PBOConfigExpressionDelete(string identifier) : base(identifier)
		{
		}

		public override string ToString()
		{
			return $"delete {Identifier};";
		}

		public override bool Equals(object? obj)
		{
			return base.Equals(obj) && obj is PBOConfigExpressionDelete other && other.Identifier == Identifier;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(base.GetHashCode(), "delete");
		}
	}
}
