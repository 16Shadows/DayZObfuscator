using DayZObfuscatorModel.PBO.Packer;
using System.Text;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfig
	{
		public IList<PBOConfigExpressionBase> Expressions { get; } = new List<PBOConfigExpressionBase>();

		public IEnumerable<PBOConfigClass> Classes => Expressions.OfType<PBOConfigClass>();
		public IEnumerable<PBOConfigExpressionVariableAssignment> Variables => Expressions.OfType<PBOConfigExpressionVariableAssignment>();
		public IEnumerable<PBOConfigArrayExpressionBase> Arrays => Expressions.OfType<PBOConfigArrayExpressionBase>();
		public IEnumerable<PBOConfigExpressionDelete> Deletes => Expressions.OfType<PBOConfigExpressionDelete>();
		public IEnumerable<PBOConfigExternalClass> ExternalClasses => Expressions.OfType<PBOConfigExternalClass>();

		public override string ToString()
		{
			//Just a guesstimate on possible aproximate size
			StringBuilder sb = new StringBuilder(50 * Expressions.Count);

			foreach (PBOConfigExpressionBase expression in Expressions)
			{
				if (expression is PBOConfigClass scope)
					sb.AppendLine(scope.ToString(0));
				else
					sb.AppendLine(expression.ToString());
			}

			return sb.ToString();
		}

		public void Binarize(PBOWriter writer)
		{
			writer.Write(0x50617200); //Par\0

			//Not sure about this one
			writer.Write(0u);
			writer.Write(8u);

			uint totalSize = 8 + 4 + 1 + 1 + 4; //8 bytes for previous two ints, 4 bytes for the size itself, 1 byte for root superclass (which is always none), 1 byte for number of classes, 4 ending zero-bytes

			foreach (PBOConfigClass pboClass in Classes)
				totalSize += (uint)pboClass.Identifier.Length + 1 + 1 + 4; //Identifier length + 1 for terminator + 1 for type + 4 bytes for offset

			foreach (PBOConfigExpressionVariableAssignment var in Variables)
			{
				totalSize += (uint)var.Identifier.Length + 1 + 1 + 1; //Identifier length + 1 for terminator + 1 for type + 1 for variable type
				totalSize += var.Value is PBOConfigValueString str ? (uint)str.Value.Length + 1 : 4; //If its a string, length + 1 byte for terminator. Otherwise its 4 bytes (for int or float)
			}

			foreach (PBOConfigArrayExpressionBase arr in Arrays)
			{
				totalSize += (uint)arr.Identifier.Length + 1 + 1 + 1 + (uint)arr.Value.Count; // Identifier length + 1 for terminator + 1 byte for number of items + 1 byte for type + 1 byte for variable type of each value

				if (arr is PBOConfigArrayExpressionAdd || arr is PBOConfigArrayExpressionSubtract)
					totalSize += 4; //Extra 4 for type

				foreach (object value in arr.Value)
					totalSize += value is PBOConfigValueString str ? (uint)str.Value.Length + 1 : 4; //If its a string, length + 1 byte for terminator. Otherwise its 4 bytes (for int or float)
			}

			foreach (PBOConfigExternalClass exClass in ExternalClasses)
				totalSize += (uint)exClass.Identifier.Length + 1 + 1; //Identifier length + 1 for terminator + 1 for type

			foreach (PBOConfigExpressionDelete delete in Deletes)
				totalSize += (uint)delete.Identifier.Length + 1 + 1; //Identifier length + 1 for terminator + 1 for type
		}
	}
}
