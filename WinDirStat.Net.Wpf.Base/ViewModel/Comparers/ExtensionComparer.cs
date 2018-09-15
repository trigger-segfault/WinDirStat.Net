using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.ViewModel.Extensions;

namespace WinDirStat.Net.ViewModel.Comparers {
	/// <summary>The sort mode for use in the file types list.</summary>
	[Serializable]
	public enum ExtensionSortMode {
		None,
		Extension,
		Color,
		Description,
		Bytes,
		Percent,
		Files,
		[Browsable(false)]
		Count,
	}

	/// <summary>The comparer for <see cref="ExtensionItemViewModel"/>s.</summary>
	public class ExtensionComparer : SortComparer<ExtensionItemViewModel, ExtensionSortMode> {

		#region Constructors

		/// <summary>Constructs the default <see cref="ExtensionComparer"/>.</summary>
		public ExtensionComparer()
			: base(ExtensionSortMode.Bytes, ListSortDirection.Descending)
		{
		}

		#endregion

		#region SortComparer Overrides

		/// <summary>A secondary comparison that's called when the sort mode comparison returns 0.</summary>
		/// 
		/// <param name="a">The first item to compare.</param>
		/// <param name="b">The second item to compare.</param>
		/// <returns>The comparison result.</returns>
		protected override Comparison<ExtensionItemViewModel> GetSortComparison(ExtensionSortMode mode) {
			switch (mode) {
			case ExtensionSortMode.None:
			case ExtensionSortMode.Bytes:
			case ExtensionSortMode.Percent:
			case ExtensionSortMode.Color: return SortBySize;
			case ExtensionSortMode.Extension: return SortByExtension;
			case ExtensionSortMode.Description: return SortByDescription;
			case ExtensionSortMode.Files: return SortByFiles;
			default:
				throw new ArgumentException($"Invalid {typeof(ExtensionSortMode).Name} ({mode})!", nameof(mode));
			}
		}

		/// <summary>Gets the comparison method for the specified sort mode.</summary>
		/// 
		/// <param name="mode">The mode to get the comparison of.</param>
		/// <returns>The comparison to use.</returns>
		protected override int SecondaryCompare(ExtensionItemViewModel a, ExtensionItemViewModel b) {
			return string.Compare(a.Model.Extension, b.Model.Extension, true);
		}

		#endregion

		#region Comparisons

		private static int SortByExtension(ExtensionItemViewModel a, ExtensionItemViewModel b) {
			return string.Compare(a.Model.Extension, b.Model.Extension, true);
		}
		private static int SortByDescription(ExtensionItemViewModel a, ExtensionItemViewModel b) {
			return string.Compare(a.TypeName, b.TypeName, true);
		}
		private static int SortBySize(ExtensionItemViewModel a, ExtensionItemViewModel b) {
			return a.Model.Size.CompareTo(b.Model.Size);
		}
		private static int SortByFiles(ExtensionItemViewModel a, ExtensionItemViewModel b) {
			return a.Model.FileCount - b.Model.FileCount;
		}

		#endregion
	}
}
