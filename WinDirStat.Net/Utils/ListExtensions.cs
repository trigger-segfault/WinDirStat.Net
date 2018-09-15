using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Utils {
	public static class ListExtensions {
		public static void Minimize<T>(this List<T> source) {
			source.Capacity = source.Count;
		}

		public static void Move<T>(this List<T> source, int index, int oldIndex) {
			T item = source[oldIndex];
			source.RemoveAt(index);
			source.Insert(index, item);
		}

		public static void MoveRange<T>(this List<T> source, int index, int oldIndex, int count) {
			List<T> items = source.GetRange(oldIndex, count);
			source.RemoveRange(oldIndex, count);
			source.InsertRange(index, items);
		}

		public static List<T> GetFullRange<T>(this List<T> source) {
			return source.GetRange(0, source.Count);
		}
	}
}
