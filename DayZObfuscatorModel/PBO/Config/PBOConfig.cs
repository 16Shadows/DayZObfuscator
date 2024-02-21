using DayZObfuscatorModel.PBO.Packer;
using System.Text;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfig
	{
		public IList<PBOConfigExpressionBase> Expressions { get; } = new List<PBOConfigExpressionBase>();

		public IList<PBOConfigClass> Classes { get; } = new List<PBOConfigClass>();
		public IEnumerable<PBOConfigExpressionVariableAssignment> Variables => Expressions.OfType<PBOConfigExpressionVariableAssignment>();
		public IEnumerable<PBOConfigArrayExpressionBase> Arrays => Expressions.OfType<PBOConfigArrayExpressionBase>();
		public IEnumerable<PBOConfigExpressionDelete> Deletes => Expressions.OfType<PBOConfigExpressionDelete>();
		public IEnumerable<PBOConfigExternalClass> ExternalClasses => Expressions.OfType<PBOConfigExternalClass>();

		public override string ToString()
		{
			//Just a guesstimate on possible aproximate size
			StringBuilder sb = new StringBuilder(50 * Expressions.Count);

			foreach (PBOConfigExpressionBase expression in Expressions)
				sb.AppendLine(expression.ToString());
			
			foreach (PBOConfigClass pboClass in Classes)
				sb.AppendLine(pboClass.ToString(0));

			return sb.ToString();
		}

		public void Binarize(PBOWriter writer)
		{
			writer.Write(0x50617200); //Par\0

			//Not sure about this one
			writer.Write(0u);
			writer.Write(8u);

			uint totalSize = 4 + 8 + 4 + 1 + 1 + 4; //4 for rap, 8 for last 2 ints + 4 for offset to next entry, 1 for this parent (always empty), 1 for number of entries in this + 4 for another offset to last entry

			//Precompute total size of binarized config
			foreach (PBOConfigExpressionBase expression in Expressions)
				totalSize += expression.GetBinarizedSize();		

			Dictionary<PBOConfigClass, uint> classSizes = new Dictionary<PBOConfigClass, uint>();

			foreach (PBOConfigClass pboClass in Classes)
			{
				uint size = pboClass.GetBinarizedBodySize();
				classSizes.Add(pboClass, size);
				totalSize += size + pboClass.GetBinarizedEntrySize();
			}	

			//Total size
			writer.Write(totalSize);

			//Root class descriptor follows
			
			//Inherited class (none)
			writer.Write('\0');
			//Number of entries
			writer.Write((byte)(Expressions.Count + Classes.Count));

			//Write all entries except for classes
			foreach (PBOConfigExpressionBase expression in Expressions)
				expression.Binarize(writer);

			uint classOffset = (uint)writer.BaseStream.Position + (uint)Classes.Sum(x => x.GetBinarizedEntrySize()) + 4; //4 bytes will later be needed to write offset to last entry

			//Write classes' entries
			foreach (PBOConfigClass pboClass in Classes)
			{
				pboClass.BinarizeEntry(writer, classOffset);
				classOffset += classSizes[pboClass];
			}

			//Offset to next entry
			writer.Write(totalSize); 
			
			classOffset = (uint)writer.BaseStream.Position;

			//Write classes' bodies
			foreach (PBOConfigClass pboClass in Classes)
			{
				classOffset += classSizes[pboClass];
				pboClass.BinarizeBody(writer, classOffset);
			}

			//4 zero-bytes - end of config
			writer.Write(0u);
		}
	}
}
