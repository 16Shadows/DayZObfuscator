namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigExternalClass : PBOConfigExpressionBase
	{
		public string? Parent { get; set; }

		public PBOConfigExternalClass(string identifier, string? parent) : base(identifier)
		{
			Parent = parent;
		}

		public override string ToString()
		{
			return Parent == null ? $"class {Identifier};" : $"class {Identifier} : {Parent};";
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
