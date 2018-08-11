using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WinDirStat.Net.Data.Nodes {
	internal class FileNodeComparer : IComparer<FileNode>, IComparer {

		public static readonly FileNodeComparer Default = new FileNodeComparer();

		private delegate int FileSortComparer(FileNode a, FileNode b);
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

		public int Compare(FileNode a, FileNode b) {
			int diff = sortComparer(a, b);
			if (diff == 0)
				diff = SortByName(a, b);
			if (SortDirection == ListSortDirection.Ascending)
				return diff;
			else
				return -diff;
		}

		public bool ShouldSort(FileNode a, FileNode b) {
			int diff = sortComparer(a, b);
			if (diff == 0)
				diff = SortByName(a, b);
			if (SortDirection == ListSortDirection.Ascending)
				return diff <= 0;
			else
				return diff > 0;
		}

		public string SortProperty {
			get {
				switch (sortMethod) {
				case FileSortMethod.None:
				case FileSortMethod.Size:
				case FileSortMethod.Percent:
				case FileSortMethod.Subtree: return nameof(FileNode.Size);
				case FileSortMethod.Name: return nameof(FileNode.Name);
				case FileSortMethod.Items: return nameof(FileNode.ItemCount);
				case FileSortMethod.Files: return nameof(FileNode.FileCount);
				case FileSortMethod.Subdirs: return nameof(FileNode.SubdirCount);
				case FileSortMethod.LastChange: return nameof(FileNode.LastChangeTime);
				//case WinDirSort.LastAccess: return SortByLastAccess;
				//case WinDirSort.Creation: return SortByCreation;
				case FileSortMethod.Attributes: return nameof(FileNode.Attributes);
				}
				return "";
			}
		}

		public void UpdateCollectionView(FolderNode parent, ListCollectionView view) {
			if (view.IsEditingItem)
				view.CommitEdit();
			if (view.IsAddingNew)
				view.CommitNew();
			string prop = SortProperty;
			//view.SortDescriptions.Clear();
			//view.LiveSortingProperties.Clear();
			if (!view.SortDescriptions.Any() || view.SortDescriptions[0].PropertyName != prop
				|| view.SortDescriptions[0].Direction != sortDirection)
			{
				//parent?.OnChildrenResetting();
				FileNode[] childrenToReadd = null;
				if (parent != null) {
					childrenToReadd = parent.vi.children.ToArray();
					parent.vi.children.Clear();
				}
				//using (view.DeferRefresh()) {
					view.SortDescriptions.Clear();
					view.LiveSortingProperties.Clear();
					//if (!view.SortDescriptions.Any()) {
					view.SortDescriptions.Add(new SortDescription(prop, sortDirection));
					view.LiveSortingProperties.Add(prop);
					//view.LiveFilteringProperties.Add(prop);
					if (sortMethod != FileSortMethod.Name) {
						//view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
						//view.LiveSortingProperties.Add("Name");
						//view.LiveFilteringProperties.Add("Name");
					}
					if (childrenToReadd != null) {
						parent.vi.children.AddRange(childrenToReadd);
					}
				//}
			}
			//view.Refresh();
			//}
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
		public static int SortBySizeDescending(FileNode a, FileNode b) {
			return b.Size.CompareTo(a.Size);
		}

		private static int SortByName(FileNode a, FileNode b) {
			return string.Compare(a.Name, b.Name, true);
		}
		private static int SortBySize(FileNode a, FileNode b) {
			return a.Size.CompareTo(b.Size);
		}
		private static int SortByItems(FileNode a, FileNode b) {
			return a.ItemCount - b.ItemCount;
		}
		private static int SortByFiles(FileNode a, FileNode b) {
			return a.FileCount - b.FileCount;
		}
		private static int SortBySubdirs(FileNode a, FileNode b) {
			return a.SubdirCount - b.SubdirCount;
		}
		private static int SortByAttributes(FileNode a, FileNode b) {
			return string.Compare(a.AttributesString, b.AttributesString);
		}
		private static int SortByLastChange(FileNode a, FileNode b) {
			return a.LastChangeTime.CompareTo(b.LastChangeTime);
		}

		int IComparer.Compare(object x, object y) {
			return Compare((FileNode) x, (FileNode) y);
		}
	}
}
