using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Data {
	public class ExtensionRecordComparer : IComparer<ExtensionRecord> {
		public static readonly ExtensionRecordComparer Default = new ExtensionRecordComparer();

		private delegate int ExtensionSortComparer(ExtensionRecord a, ExtensionRecord b);
		private ExtensionSortMethod sortMethod;
		private ListSortDirection sortDirection;
		private ExtensionSortComparer sortComparer;

		public ExtensionRecordComparer() {
			sortMethod = ExtensionSortMethod.Bytes;
			sortDirection = ListSortDirection.Descending;
			sortComparer = SortBySize;
		}

		public ExtensionSortMethod SortMethod {
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

		public int Compare(ExtensionRecord a, ExtensionRecord b) {
			int diff = sortComparer(a, b);
			if (diff == 0)
				diff = SortByExtension(a, b);
			if (SortDirection == ListSortDirection.Ascending)
				return diff;
			else
				return -diff;
		}

		public bool ShouldSort(ExtensionRecord a, ExtensionRecord b) {
			int diff = sortComparer(a, b);
			if (diff == 0)
				diff = SortByExtension(a, b);
			if (SortDirection == ListSortDirection.Ascending)
				return diff <= 0;
			else
				return diff > 0;
		}

		private static ExtensionSortComparer GetSortComparer(ExtensionSortMethod method) {
			switch (method) {
			case ExtensionSortMethod.None:
			case ExtensionSortMethod.Bytes:
			case ExtensionSortMethod.Percent:
			case ExtensionSortMethod.Color: return SortBySize;
			case ExtensionSortMethod.Extension: return SortByExtension;
			case ExtensionSortMethod.Description: return SortByDescription;
			case ExtensionSortMethod.Files: return SortByFiles;
			default:
				throw new ArgumentException($"Invalid {typeof(ExtensionSortMethod).Name} ({method})!", nameof(method));
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
		public static int SortBySizeDescending(ExtensionRecord a, ExtensionRecord b) {
			return b.Size.CompareTo(a.Size);
		}

		private static int SortByExtension(ExtensionRecord a, ExtensionRecord b) {
			return string.Compare(a.Extension, b.Extension, true);
		}
		private static int SortByDescription(ExtensionRecord a, ExtensionRecord b) {
			return string.Compare(a.Name, b.Name, true);
		}
		private static int SortBySize(ExtensionRecord a, ExtensionRecord b) {
			return a.Size.CompareTo(b.Size);
		}
		private static int SortByFiles(ExtensionRecord a, ExtensionRecord b) {
			return a.FileCount - b.FileCount;
		}
	}
}
