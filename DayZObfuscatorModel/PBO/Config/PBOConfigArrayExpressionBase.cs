using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.PBO.Config
{
	public class PBOConfigArrayExpressionBase : PBOConfigExpressionBase
	{
		public IList<object> Value { get; }

		public PBOConfigArrayExpressionBase(string identifier, IList<object> value) : base(identifier)
		{
			Value = value;
		}
	}
}
