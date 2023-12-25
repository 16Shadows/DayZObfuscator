﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfig
	{
		public IList<PBOConfigExpressionBase> Expressions { get; } = new List<PBOConfigExpressionBase>();

		public IEnumerable<PBOConfigClass> Scopes => Expressions.OfType<PBOConfigClass>();
		public IEnumerable<PBOConfigExpressionVariableAssignment> Variables => Expressions.OfType<PBOConfigExpressionVariableAssignment>();
		public IEnumerable<PBOConfigArrayExpressionBase> Arrays => Expressions.OfType<PBOConfigArrayExpressionBase>();

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
	}
}
