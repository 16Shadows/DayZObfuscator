using System.Text;
using CSToolbox.Extensions;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigClass : PBOConfigExpressionBase
	{
		public IList<PBOConfigExpressionBase> Expressions { get; } = new List<PBOConfigExpressionBase>();

		public IEnumerable<PBOConfigClass> Scopes => Expressions.OfType<PBOConfigClass>();
		public IEnumerable<PBOConfigExpressionVariableAssignment> Variables => Expressions.OfType<PBOConfigExpressionVariableAssignment>();
		public IEnumerable<PBOConfigArrayExpressionBase> Arrays => Expressions.OfType<PBOConfigArrayExpressionBase>();

		public PBOConfigClass(string identifier) : base(identifier)
		{
		}

		public override string ToString()
		{
			return ToString(0);
		}

		public string ToString(int tabBy = 0)
		{
			string tabs = tabBy > 0 ? string.Join("", Enumerable.Repeat('\t', tabBy)) : "";
			string tabsInner = tabs + '\t';

			//Just a guesstimate on possible aproximate size
			StringBuilder sb = new StringBuilder(50 * Expressions.Count + 50);

			sb.Append(tabs);
			sb.Append("class ");
			sb.AppendLine(Identifier);
			sb.Append(tabs);
			sb.AppendLine("{");
			
			foreach (PBOConfigExpressionBase expression in Expressions)
			{
				if (expression is PBOConfigClass scope)
					sb.AppendLine(scope.ToString(tabBy+1));
				else
				{
					sb.Append(tabsInner);
					sb.AppendLine(expression.ToString());
				}
			}

			sb.Append(tabs);
			sb.Append("};");

			return sb.ToString();
		}

		public override bool Equals(object? obj)
		{
			return base.Equals(obj) && obj is PBOConfigClass other && Expressions.SequenceEqualsOrderInvariant(other.Expressions);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(base.GetHashCode(), Identifier);
		}
	}
}