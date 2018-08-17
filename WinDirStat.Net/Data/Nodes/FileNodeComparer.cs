using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WinDirStat.Net.Data.Nodes {
	internal class FileNodeComparer : IComparer<FileNodeBase>, IComparer {

		public static readonly FileNodeComparer Default = new FileNodeComparer();

		private delegate int FileSortComparer(FileNodeBase a, FileNodeBase b);
		private FileSortMethod sortMethod;
		private ListSortDirection sortDirection;
		private FileSortComparer sortComparer;

		public FileNodeComparer() {
			sortMethod = FileSortMethod.Size;
			sortDirection = ListSortDirection.Descending;
			sortComparer = SortBySize;
		}

		public FileSortMethod SortMethod {
			get => sortMethod;
			set {
				if (sortMethod != value) {
					sortMethod = value;
					sortComparer = GetSortComparer(sortMethod);
				}
			}
		}
		public ListSortDirection SortDirection {
			get => sortDirection;
			set => sortDirection = value;
		}

		public int Compare(FileNodeBase a, FileNodeBase b) {
			int diff = sortComparer(a, b);
			// Always sort alphabetically after initial sort
			if (diff == 0)
				return SortByName(a, b);
			if (SortDirection == ListSortDirection.Ascending)
				return diff;
			else
				return -diff;
		}

		public bool ShouldSort(FileNodeBase a, FileNodeBase b) {
			int diff = sortComparer(a, b);
			// Always sort alphabetically after initial sort
			if (diff == 0)
				return SortByName(a, b) < 0;
			if (SortDirection == ListSortDirection.Ascending)
				return diff < 0;
			else
				return diff > 0;
		}
		
		private static FileSortComparer GetSortComparer(FileSortMethod method) {
			switch (method) {
			case FileSortMethod.None:
			case FileSortMethod.Size:
			case FileSortMethod.Percent:
			case FileSortMethod.Subtree: return SortBySize;
			case FileSortMethod.Name: return SortByName;
			case FileSortMethod.Items: return SortByItems;
			case FileSortMethod.Files: return SortByFiles;
			case FileSortMethod.Subdirs: return SortBySubdirs;
			case FileSortMethod.LastChange: return SortByLastChange;
			//case WinDirSort.LastAccess: return SortByLastAccess;
			//case WinDirSort.Creation: return SortByCreation;
			case FileSortMethod.Attributes: return SortByAttributes;
			default:
				throw new ArgumentException($"Invalid {typeof(FileSortMethod).Name} ({method})!", nameof(method));
			}
		}

		/*private void SortChildren(WinDirNode parent) {
			int index = itemsSource.IndexOf(parent);
			SortChildren(parent, ref index);
		}

		private void SortChildren(WinDirNode parent, ref int index) {
			index++;
			for (int i = 0; i < parent.Children.Count; i++) {
				WinDirNode child = parent.Children[i] as WinDirNode;
				if (child.IsExpanded)
					SortChildren(child, ref index);
			}
		}*/
		public static int SortBySizeDescending(FileNodeBase a, FileNodeBase b) {
			return b.Size.CompareTo(a.Size);
		}

		private static int SortByName(FileNodeBase a, FileNodeBase b) {
			return string.Compare(a.Name, b.Name, true);
		}
		private static int SortBySize(FileNodeBase a, FileNodeBase b) {
			return a.Size.CompareTo(b.Size);
		}
		private static int SortByItems(FileNodeBase a, FileNodeBase b) {
			return a.ItemCount - b.ItemCount;
		}
		private static int SortByFiles(FileNodeBase a, FileNodeBase b) {
			return a.FileCount - b.FileCount;
		}
		private static int SortBySubdirs(FileNodeBase a, FileNodeBase b) {
			return a.SubdirCount - b.SubdirCount;
		}
		private static int SortByAttributes(FileNodeBase a, FileNodeBase b) {
			return a.SortAttributes.CompareTo(b.SortAttributes);
		}
		private static int SortByLastChange(FileNodeBase a, FileNodeBase b) {
			return a.LastChangeTime.CompareTo(b.LastChangeTime);
		}

		int IComparer.Compare(object x, object y) {
			return Compare((FileNodeBase) x, (FileNodeBase) y);
		}
	}
}
