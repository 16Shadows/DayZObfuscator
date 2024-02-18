using System.Globalization;
using System.Runtime.CompilerServices;

namespace DayZObfuscatorModel.PBO.Packer
{
	public class PBOPackerComponentProperties : Dictionary<string, object>
	{
		public void GetProperty<T>(out T value, T defaultValue, [CallerArgumentExpression(nameof(value))]string propName = "")
		{
			if (TryGetValue(propName, out object? v))
			{
				try
				{
					value = (T)Convert.ChangeType(v, typeof(T), CultureInfo.InvariantCulture);
				}
				catch
				{
					value = defaultValue;
				}
			}
			else
				value = defaultValue;
		}
	}
}
