﻿using CSToolbox.Extensions;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigArrayExpressionSubtract : PBOConfigArrayExpressionBase
	{
		public PBOConfigArrayExpressionSubtract(string identifier, IList<object> value) : base(identifier, value)
		{
		}

		public override string ToString()
		{
			return $"{Identifier} -= {{{Value.ToString(", ")}}};";
		}

		public override bool Equals(object? obj)
		{
			return base.Equals(obj) && obj is PBOConfigArrayExpressionSubtract;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine("-=", base.GetHashCode());
		}
	}
}
