using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel
{
	public static class IEnumerableExtensions
	{
		public static bool SequenceEqualsOrderInvariant<T>(this IEnumerable<T> first, IEnumerable<T> second) => SequenceEqualsOrderInvariant(first, second, EqualityComparer<T>.Default);

		public static bool SequenceEqualsOrderInvariant<T>(this IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> equalityComparer)
		{
			//Code analyzer complains here because TKey shouldn't be nullable (makes sense). However, we need to allow T to be nullable. We are never actually storing nulls in the dictionary.
#pragma warning disable CS8714
			Dictionary<T, int> counters = new Dictionary<T, int>(equalityComparer);
#pragma warning restore CS8714
			int nullCounter = 0;

			int count = 0;

			foreach (var item in first)
			{
				if (item is null)
					nullCounter++;
				else if (counters.TryGetValue(item, out count))
					counters[item] = count + 1;
				else
					counters.Add(item, 1);
			}

			foreach (var item in second)
			{
				if (item is null)
					nullCounter--;
				else if (counters.TryGetValue(item, out count))
				{
					if (count == 1)
						counters.Remove(item);
					else
						counters[item] = count - 1;
				}
				else
					counters.Add(item, -1);
			}

			return nullCounter == 0 && counters.Count == 0;
		}
	}
}
