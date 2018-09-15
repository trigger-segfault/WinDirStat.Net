using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.ViewModel.Files;

namespace WinDirStat.Net.ViewModel.Comparers {
	/// <summary>The sort mode for use in the file tree.</summary>
	[Serializable]
	public enum FileSortMode {
		None,
		Name,
		Subtree,
		Percent,
		Size,
		Items,
		Files,
		Subdirs,
		LastWrite,
		LastAccess,
		Creation,
		Attributes,
		[Browsable(false)]
		Count,
	}

	/// <summary>The comparer for <see cref="FileItemViewModel"/>s.</summary>
	public class FileComparer : SortComparer<FileItemViewModel, FileSortMode> {

		#region Constructors

		/// <summary>Constructs the default <see cref="FileComparer"/>.</summary>
		public FileComparer()
			: base(FileSortMode.Size, ListSortDirection.Descending)
		{
		}

		#endregion

		#region SortComparer Overrides

		/// <summary>A secondary comparison that's called when the sort mode comparison returns 0.</summary>
		/// 
		/// <param name="a">The first item to compare.</param>
		/// <param name="b">The second item to compare.</param>
		/// <returns>The comparison result.</returns>
		protected override Comparison<FileItemViewModel> GetSortComparison(FileSortMode mode) {
			switch (mode) {
			case FileSortMode.None:
			case FileSortMode.Size:
			case FileSortMode.Percent:
			case FileSortMode.Subtree: return SortBySize;
			case FileSortMode.Name: return SortByName;
			case FileSortMode.Items: return SortByItems;
			case FileSortMode.Files: return SortByFiles;
			case FileSortMode.Subdirs: return SortBySubdirs;
			case FileSortMode.LastWrite: return SortByLastWrite;
			//case FileSortMode.LastAccess: return SortByLastAccess;
			//case FileSortMode.Creation: return SortByCreation;
			case FileSortMode.Attributes: return SortByAttributes;
			default:
				throw new ArgumentException($"Invalid {typeof(FileSortMode).Name} ({mode})!", nameof(mode));
			}
		}

		/// <summary>Gets the comparison method for the specified sort mode.</summary>
		/// 
		/// <param name="mode">The mode to get the comparison of.</param>
		/// <returns>The comparison to use.</returns>
		protected override int SecondaryCompare(FileItemViewModel a, FileItemViewModel b) {
			return string.Compare(a.Model.Name, b.Model.Name, true);
		}

		#endregion

		#region Comparisons

		private static int SortByName(FileItemViewModel a, FileItemViewModel b) {
			return string.Compare(a.Model.Name, b.Model.Name, true);
		}
		private static int SortBySize(FileItemViewModel a, FileItemViewModel b) {
			return a.Model.Size.CompareTo(b.Model.Size);
		}
		private static int SortByItems(FileItemViewModel a, FileItemViewModel b) {
			return a.Model.ItemCount - b.Model.ItemCount;
		}
		private static int SortByFiles(FileItemViewModel a, FileItemViewModel b) {
			return a.Model.FileCount - b.Model.FileCount;
		}
		private static int SortBySubdirs(FileItemViewModel a, FileItemViewModel b) {
			return a.Model.SubdirCount - b.Model.SubdirCount;
		}
		private static int SortByAttributes(FileItemViewModel a, FileItemViewModel b) {
			return a.Model.SortAttributes.CompareTo(b.Model.SortAttributes);
		}
		private static int SortByLastWrite(FileItemViewModel a, FileItemViewModel b) {
			return a.Model.LastWriteTimeUtc.CompareTo(b.Model.LastWriteTimeUtc);
		}

		#endregion
	}
}
