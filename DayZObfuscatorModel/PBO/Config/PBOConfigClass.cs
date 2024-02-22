using System.Text;
using CSToolbox.Extensions;
using DayZObfuscatorModel.PBO.Packer;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigClass
	{
		public IList<PBOConfigExpressionBase> Expressions { get; } = new List<PBOConfigExpressionBase>();

		public IList<PBOConfigClass> Classes { get; } = new List<PBOConfigClass>();
		public IEnumerable<PBOConfigExpressionVariableAssignment> Variables => Expressions.OfType<PBOConfigExpressionVariableAssignment>();
		public IEnumerable<PBOConfigArrayExpressionBase> Arrays => Expressions.OfType<PBOConfigArrayExpressionBase>();
		public IEnumerable<PBOConfigExpressionDelete> Deletes => Expressions.OfType<PBOConfigExpressionDelete>();
		public IEnumerable<PBOConfigExternalClass> ExternalClasses => Expressions.OfType<PBOConfigExternalClass>();


		public string Identifier { get; set; }
		public string? Parent { get; set; }

		public PBOConfigClass(string identifier, string? parent)
		{
			Identifier = identifier;
			Parent = parent;
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
			sb.Append(Identifier);

			if (Parent != null)
			{
				sb.Append(" : ");
				sb.Append(Parent);
			}

			sb.AppendLine();

			sb.Append(tabs);
			sb.AppendLine("{");
			
			foreach (PBOConfigExpressionBase expression in Expressions)
			{
				sb.Append(tabsInner);
				sb.AppendLine(expression.ToString());
			}
			
			foreach (PBOConfigClass pboClass in Classes)
				sb.AppendLine(pboClass.ToString(tabBy+1));

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
			int hash = HashCode.Combine(base.GetHashCode(), "class");
			foreach (var expr in Expressions)
				hash = HashCode.Combine(hash, expr.GetHashCode());
			return hash;
		}

		public void BinarizeEntry(PBOWriter writer, uint offset)
		{
			writer.Write((byte)0);
			writer.Write(Identifier);
			writer.Write(offset);
		}

		public uint GetBinarizedEntrySize()
		{
			return (uint)Identifier.Length + 1 + 1 + 4; //1 for terminator, 1 for entry type, 4 for offset
		}

		public void BinarizeBody(PBOWriter writer, uint nextEntryOffset)
		{
			//Inherited class
			writer.Write(Parent ?? "");
			//Number of entries
			writer.Write((byte)(Expressions.Count + Classes.Count));

			//Write all entries except for classes
			foreach (PBOConfigExpressionBase expression in Expressions)
				expression.Binarize(writer);

			Dictionary<PBOConfigClass, uint> offsets = new Dictionary<PBOConfigClass, uint>();
			uint classOffset = (uint)writer.BaseStream.Position + (uint)Classes.Sum(x => x.GetBinarizedEntrySize()) + 4; //4 for offset to next class entry after entries of this class

			//Write classes' entries
			foreach (PBOConfigClass pboClass in Classes)
			{
				pboClass.BinarizeEntry(writer, classOffset);
				uint size = pboClass.GetBinarizedBodySize();
				classOffset += size;
				offsets.Add(pboClass, size);
			}
			
			//Write offset to next entry
			writer.Write(nextEntryOffset);
			
			classOffset = (uint)writer.BaseStream.Position;

			//Write classes' bodies
			foreach (PBOConfigClass pboClass in Classes)
			{
				classOffset += offsets[pboClass];
				pboClass.BinarizeBody(writer, classOffset);
			}
		}

		public uint GetBinarizedBodySize()
		{
			uint size = 1 + 1 + 4; //1 for parent terminator, 1 for number of entries, 4 for next entry offset
			if (Parent != null)
				size += (uint)Parent.Length;

			foreach (PBOConfigExpressionBase expr in Expressions)
				size += expr.GetBinarizedSize();

			foreach (PBOConfigClass pboClass in Classes)
				size += pboClass.GetBinarizedEntrySize() + pboClass.GetBinarizedBodySize();

			return size;
		}
	}
}