﻿using DayZObfuscatorModel.PBO.Packer;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigExpressionVariableAssignment : PBOConfigExpressionBase, IEquatable<PBOConfigExpressionVariableAssignment>
	{
		private PBOConfigVariableValue _Value;

		public PBOConfigVariableValue Value { get => _Value; set => _Value = value ?? throw new ArgumentNullException(nameof(value)); }

		public PBOConfigExpressionVariableAssignment(string identifier, PBOConfigVariableValue value) : base(identifier)
		{
			_Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		public override string ToString()
		{
			return $"{Identifier} = {Value};";
		}

		public override bool Equals(object? obj)
		{
			return base.Equals(obj) && (obj is PBOConfigExpressionVariableAssignment var && Equals(var));
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(base.GetHashCode(), Value);
		}

		public bool Equals(PBOConfigExpressionVariableAssignment? other)
		{
			return base.Equals(other) && Value.Equals(other?.Value);
		}

		public override void Binarize(PBOWriter writer)
		{
			writer.Write((byte)1);
			writer.Write(Value.GetBinarizedType());
			writer.Write(Identifier);
			Value.Binarize(writer);
		}

		public override uint GetBinarizedSize()
		{
			return (uint)Identifier.Length + 1 + 1 + 1 + Value.GetBinarizedSize(); //Identifier length + 1 for terminator + 1 for type + 1 for variable type + value size
		}
	}
}
