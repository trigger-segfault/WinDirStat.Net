using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WinDirStat.Net.Model.Data.Nodes;
using WinDirStat.Net.Model.View.Nodes;

namespace WinDirStat.Net.Model.View.Comparers {
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
		LastChange,
		LastAccess,
		Creation,
		Attributes,
		[Browsable(false)]
		Count,
	}

	public class FileComparer : SortComparer<FileNodeViewModel, FileSortMode> {
		
		public FileComparer()
			: base(FileSortMode.Size, ListSortDirection.Descending)
		{
		}
		
		protected override Comparison<FileNodeViewModel> GetSortComparison(FileSortMode mode) {
			switch (mode) {
			case FileSortMode.None:
			case FileSortMode.Size:
			case FileSortMode.Percent:
			case FileSortMode.Subtree: return SortBySize;
			case FileSortMode.Name: return SortByName;
			case FileSortMode.Items: return SortByItems;
			case FileSortMode.Files: return SortByFiles;
			case FileSortMode.Subdirs: return SortBySubdirs;
			case FileSortMode.LastChange: return SortByLastChange;
			//case WinDirSort.LastAccess: return SortByLastAccess;
			//case WinDirSort.Creation: return SortByCreation;
			case FileSortMode.Attributes: return SortByAttributes;
			default:
				throw new ArgumentException($"Invalid {typeof(FileSortMode).Name} ({mode})!", nameof(mode));
			}
		}

		protected override int SecondaryCompare(FileNodeViewModel a, FileNodeViewModel b) {
			return string.Compare(a.Model.Name, b.Model.Name, true);
		}

		private static int SortByName(FileNodeViewModel a, FileNodeViewModel b) {
			return string.Compare(a.Model.Name, b.Model.Name, true);
		}
		private static int SortBySize(FileNodeViewModel a, FileNodeViewModel b) {
			return a.Model.Size.CompareTo(b.Model.Size);
		}
		private static int SortByItems(FileNodeViewModel a, FileNodeViewModel b) {
			return a.Model.ItemCount - b.Model.ItemCount;
		}
		private static int SortByFiles(FileNodeViewModel a, FileNodeViewModel b) {
			return a.Model.FileCount - b.Model.FileCount;
		}
		private static int SortBySubdirs(FileNodeViewModel a, FileNodeViewModel b) {
			return a.Model.SubdirCount - b.Model.SubdirCount;
		}
		private static int SortByAttributes(FileNodeViewModel a, FileNodeViewModel b) {
			return a.Model.SortAttributes.CompareTo(b.Model.SortAttributes);
		}
		private static int SortByLastChange(FileNodeViewModel a, FileNodeViewModel b) {
			return a.Model.LastChangeTime.CompareTo(b.Model.LastChangeTime);
		}
	}
}
