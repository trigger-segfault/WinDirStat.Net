using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Utils {
	/// <summary>Extensions for the list class.</summary>
	public static class ListExtensions {
		/// <summary>
		/// Minimizes the memory usage of teh list by reducing the <see cref="List{T}.Capacity"/> to the
		/// current <see cref="List{T}.Count"/>.
		/// </summary>
		/// 
		/// <typeparam name="T">The element type of the list.</typeparam>
		/// <param name="source">The list to minimize.</param>
		public static void Minimize<T>(this List<T> source) {
			source.Capacity = source.Count;
		}

		/// <summary>Moves a single item in the list.</summary>
		/// 
		/// <typeparam name="T">The element type of the list.</typeparam>
		/// <param name="source">The list to move an item in.</param>
		/// <param name="index">The new index of the item.</param>
		/// <param name="oldIndex">The old index of the item.</param>
		public static void Move<T>(this List<T> source, int index, int oldIndex) {
			T item = source[oldIndex];
			source.RemoveAt(index);
			source.Insert(index, item);
		}

		/// <summary>Movess a range of items in the list.</summary>
		/// 
		/// <typeparam name="T">The element type of the list.</typeparam>
		/// <param name="source">The list to move items in.</param>
		/// <param name="index">The new index of the items.</param>
		/// <param name="oldIndex">The old index of the items.</param>
		/// <param name="count">The number of items to move.</param>
		public static void MoveRange<T>(this List<T> source, int index, int oldIndex, int count) {
			List<T> items = source.GetRange(oldIndex, count);
			source.RemoveRange(oldIndex, count);
			source.InsertRange(index, items);
		}

		/// <summary>
		/// Creates a copy of the list using a more efficient method than with <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// 
		/// <typeparam name="T">The element type of the list.</typeparam>
		/// <param name="source">The list to create a copy of.</param>
		public static List<T> GetFullRange<T>(this List<T> source) {
			return source.GetRange(0, source.Count);
		}
	}
}
