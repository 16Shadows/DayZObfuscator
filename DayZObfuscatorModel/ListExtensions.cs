using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel
{
	public static class ListExtensions
	{
		public static string ToString<T>(this IList<T> list, char separator) => ToString<T>(list, separator.ToString());

		public static string ToString<T>(this IList<T> list, string separator)
		{
			StringBuilder sb = new StringBuilder(list.Count * 10);

			sb.Append(list[0]);
			for (int i = 1;  i < list.Count; i++)
			{
				sb.Append(separator);
				sb.Append(list[i]);
			}

			return sb.ToString();
		}
	}
}
