using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Filesystem.Ntfs;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WinDirStat.Net.Controls;
using WinDirStat.Net.Data;
using WinDirStat.Net.Drawing;
using WinDirStat.Net.Settings.Geometry;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Data.Nodes {
	public partial class FileNode : INotifyPropertyChanged, ITreemapItem {
		protected static readonly List<FileNode> EmptyList = new List<FileNode>();
		protected static EmptyFileNodeCollection EmptyChildren = new EmptyFileNodeCollection();


		protected internal List<FileNode> virtualChildren = EmptyList;
		protected BitField32 bitfield;

		public FileNodeType Type {
			get => (FileNodeType) bitfield.GetByte(0, 3);
			protected set {
				bitfield.SetByte(0, 3, (byte) value);
				if (value != FileNodeType.File)
					extension = string.Empty;
			}
		}
		public FileAttributes Attributes {
			get {
				FileAttributes attr = 0;
				if (IsFileReadOnly) attr |= FileAttributes.ReadOnly;
				if (IsFileHidden) attr |= FileAttributes.Hidden;
				if (IsFileSystem) attr |= FileAttributes.System;
				if (IsFileArchive) attr |= FileAttributes.Archive;
				if (IsFileCompressed) attr |= FileAttributes.Compressed;
				if (IsFileEncrypted) attr |= FileAttributes.Encrypted;
				if (IsFileDirectory) attr |= FileAttributes.Directory;
				if (IsFileReparsePoint) attr |= FileAttributes.ReparsePoint;
				if (IsFileTemporary) attr |= FileAttributes.Temporary;
				return attr;
			}
			set {
				IsFileReadOnly = value.HasFlag(FileAttributes.ReadOnly);
				IsFileHidden = value.HasFlag(FileAttributes.Hidden);
				IsFileSystem = value.HasFlag(FileAttributes.System);
				IsFileArchive = value.HasFlag(FileAttributes.Archive);
				IsFileCompressed = value.HasFlag(FileAttributes.Compressed);
				IsFileEncrypted = value.HasFlag(FileAttributes.Encrypted);
				IsFileDirectory = value.HasFlag(FileAttributes.Directory);
				IsFileReparsePoint = value.HasFlag(FileAttributes.ReparsePoint);
				IsFileTemporary = value.HasFlag(FileAttributes.Temporary);
			}
		}
		public bool IsFileReadOnly {
			get => bitfield.GetBit(4);
			set => bitfield.SetBit(4, value);
		}
		public bool IsFileHidden {
			get => bitfield.GetBit(5);
			set => bitfield.SetBit(5, value);
		}
		public bool IsFileSystem {
			get => bitfield.GetBit(6);
			set => bitfield.SetBit(6, value);
		}
		public bool IsFileArchive {
			get => bitfield.GetBit(7);
			set => bitfield.SetBit(7, value);
		}
		public bool IsFileCompressed {
			get => bitfield.GetBit(8);
			set => bitfield.SetBit(8, value);
		}
		public bool IsFileEncrypted {
			get => bitfield.GetBit(9);
			set => bitfield.SetBit(9, value);
		}
		public bool IsFileDirectory {
			get => bitfield.GetBit(10);
			set => bitfield.SetBit(10, value);
		}
		public bool IsFileReparsePoint {
			get => bitfield.GetBit(11);
			set => bitfield.SetBit(11, value);
		}
		public bool IsFileTemporary {
			get => bitfield.GetBit(12);
			set => bitfield.SetBit(12, value);
		}

#if !COMPRESSED_DATA
		protected string name;
#else
		protected string name;
		/*protected char[] cName;

		protected string name {
			get => new string(cName);
			set => cName = value.ToCharArray();
		}*/
#endif
		protected long size;
		//protected FileAttributes attributes;
		//private DateTime creationTime;
		//private DateTime lastAccessTime;
		protected DateTime lastChangeTime;
		internal FolderNode virtualParent;
		//protected System.Drawing.Rectangle graphRectangle;
		private Rectangle2S graphRectangle;
		private string extension;
		
		protected FileNode() {
			extension = string.Empty;
		}

		public FileNode(INtfsNode node) : this(node.FullName) {
			Attributes = node.Attributes;
			//creationTime = node.CreationTime;
			lastChangeTime = node.LastChangeTime;
			//lastAccessTime = node.LastAccessTime;

			if (!Attributes.HasFlag(FileAttributes.Directory) &&
				!Attributes.HasFlag(FileAttributes.ReparsePoint))
				size = (long) node.Size;
		}

		public FileNode(FileSystemInfo info) : this(info.FullName) {
			Attributes = info.Attributes;
			//creationTime = info.CreationTime;
			lastChangeTime = info.LastWriteTime;
			//lastAccessTime = info.LastAccessTime;

			if (!Attributes.HasFlag(FileAttributes.Directory) &&
				!Attributes.HasFlag(FileAttributes.ReparsePoint) &&
				info is FileInfo fileInfo)
				size = fileInfo.Length;
		}

		public FileNode(IFileFindData data) : this(data.FullName) {
			Attributes = data.Attributes;
			//creationTime = data.CreationTime;
			lastChangeTime = data.LastWriteTime;
			//lastAccessTime = data.LastAccessTime;

			if (!data.IsDirectory && !Attributes.HasFlag(FileAttributes.ReparsePoint))
				size = data.Size;
		}

		protected FileNode(string path) {
			Type = FileNodeType.File;
			if (!System.IO.Path.IsPathRooted(path))
				path = System.IO.Path.GetFullPath(path);
			else
				path = PathUtils.TrimSeparatorDotEnd(path);
			name = GetFileName(path);
			extension = GetExtension(name);
		}

		public FileNode(FileNode node, FolderNode parent) {
			virtualParent = parent;
			Type = node.Type;
			extension = node.extension;
			name = node.name;
			lastChangeTime = node.lastChangeTime;
			Attributes = node.Attributes;
			size = node.size;
			graphRectangle = node.graphRectangle;
			/*if (node.visualInfo != null) {
				visualInfo = new VisualInfo();
			}*/
		}

		private static string GetFileName(string path) {
			int length = path.Length;
			for (int i = length; --i >= 0;) {
				char ch = path[i];
				if (ch == System.IO.Path.DirectorySeparatorChar || ch == System.IO.Path.AltDirectorySeparatorChar || ch == System.IO.Path.VolumeSeparatorChar)
					return path.Substring(i + 1, length - i - 1);

			}
			return path;
		}

		/*public virtual WinDirDocument Document {
			get => Root.Document;
		}*/

		public WinDirDocument Document {
			get => (virtualParent == null ? ((RootNode) this).document : Root.Document);
		}

		public FolderNode VirtualParent {
			get => virtualParent;
		}
		/*public virtual FileNodeType Type {
			get => FileNodeType.File;
		}*/
		/*public virtual bool HasVirtualChildren {
			get => false;
		}*/
		public bool HasVirtualChildren {
			get => VirtualChildren.Count != 0;
		}

		public int ItemCount {
			get => Math.Max(-1, FileCount + SubdirCount);
		}

		/*public virtual int FileCount {
			get => -1;
		}
		public virtual int SubdirCount {
			get => -1;
		}*/

		public int FileCount {
			get => (this as FolderNode)?.fileCount ?? -1;
		}
		public int SubdirCount {
			get => (this as FolderNode)?.subdirCount ?? -1;
		}

		public string Name {
			get => name;
		}

		public RootNode Root {
			get {
				if (Type == FileNodeType.Root)
					return (RootNode) this;
				else
					return VirtualParent.Root;
			}
		}

		public string Path {
			get {
				switch (Type) {
				case FileNodeType.FileCollection:
				case FileNodeType.FreeSpace:
					return VirtualParent.Path;
				case FileNodeType.Root:
					return ((RootNode) this).RootPath;
				default:
					return PathUtils.CombineNoChecks(VirtualParent.Path, name);
				}
			}
		}

		public bool IsVirtualRoot {
			get => virtualParent == null;
		}

		public double Percent {
			get {
				if (IsVirtualRoot)
					return 1d;
				else if (VirtualParent.size == 0)
					return 0d;
				else
					return (double) size / VirtualParent.size;
			}
		}

		public bool IsEmptySize {
			get => size == 0;
		}
		/*public FileAttributes Attributes {
			get => attributes;
		}*/
		/*public DateTime CreationTime {
			get => creationTime;
		}*/
		public DateTime LastChangeTime {
			get => lastChangeTime;
		}
		/*public DateTime LastAccessTime {
			get => lastAccessTime;
		}*/
		public long Size {
			get => size;
		}

		/*public virtual IReadOnlyList<FileNode> VirtualChildren {
			get => EmptyList;
		}*/

		/*public IReadOnlyList<FileNode> VirtualChildren {
			get => (this as FolderNode)?.virtualChildren ?? EmptyList;
		}*/

		public IReadOnlyList<FileNode> VirtualChildren {
			get => virtualChildren ?? EmptyList;
		}

		/*public string Extension {
			get => (Type == FileNodeType.File ? GetExtension(name) : string.Empty);
		}*/

		public string Extension {
			get => extension;
		}


		private const string Dot = ".";

		private static string GetExtension(string path) {
			int length = path.Length;
			for (int i = length; --i >= 0;) {
				char ch = path[i];
				if (ch == '.') {
					if (i != length - 1)
						return path.Substring(i, length - i).ToLower();
					else
						return Dot;
				}
				if (ch == System.IO.Path.DirectorySeparatorChar || ch == System.IO.Path.AltDirectorySeparatorChar || ch == System.IO.Path.VolumeSeparatorChar)
					break;
			}
			return Dot;
		}

		public string AttributesString {
			get {
				string s = "";
				/*if (Attributes.HasFlag(FileAttributes.ReadOnly)) s += "R";
				if (Attributes.HasFlag(FileAttributes.Hidden)) s += "H";
				if (Attributes.HasFlag(FileAttributes.System)) s += "S";
				if (Attributes.HasFlag(FileAttributes.Archive)) s += "A";
				if (Attributes.HasFlag(FileAttributes.Compressed)) s += "C";
				if (Attributes.HasFlag(FileAttributes.Encrypted)) s += "E";*/
				if (IsFileReadOnly) s += "R";
				if (IsFileHidden) s += "H";
				if (IsFileSystem) s += "S";
				if (IsFileArchive) s += "A";
				if (IsFileCompressed) s += "C";
				if (IsFileEncrypted) s += "E";
				return s;
			}
		}

		internal void LoadIcon(RootNode root, bool basicLoad = false) {
			switch (Type) {
			case FileNodeType.FileCollection:
				vi.icon = IconCache.FileCollectionIcon;
				VisualPropertyChanged(nameof(Icon));
				return;
			case FileNodeType.FreeSpace:
				vi.icon = IconCache.FreeSpaceIcon;
				VisualPropertyChanged(nameof(Icon));
				return;
			case FileNodeType.File:
				vi.icon = root.Document.Extensions[Extension].Icon;
				break;
			default:
				vi.icon = root.Document.Icons.DefaultFolderIcon;
				break;
			}
			VisualPropertyChanged(nameof(Icon));
			if (!basicLoad)
				root.Document.Icons.CacheIconAsync(Path, Attributes, LoadIconCallback);
		}

		private void LoadIconCallback(ImageSource icon) {
			vi.icon = icon ?? Root.Document.Icons.DefaultFileIcon;
			VisualPropertyChanged(nameof(Icon));
		}

		protected bool IsLoaded {
			get => Parent != null || IsVirtualRoot;
		}

		protected void RaisePropertiesChanged(params string[] names) {
			for (int i = 0; i < names.Length; i++)
				RaisePropertyChanged(names[i]);
		}

		protected bool InUIThread {
			get => (Dispatcher.CurrentDispatcher.Thread == Thread.CurrentThread);
		}

		protected void VisualPropertyChanged(string name) {
			if (IsLoaded) {
				if (!InUIThread)
					Application.Current.Dispatcher.Invoke(() => RaisePropertyChanged(name));
				else
					RaisePropertyChanged(name);
			}
		}


		protected void VisualPropertiesChanged(params string[] names) {
			if (IsLoaded) {
				if (!InUIThread)
					Application.Current.Dispatcher.Invoke(() => RaisePropertiesChanged(names));
				else
					RaisePropertiesChanged(names);
			}
		}

		protected void VisualInvoke(Action action) {
			if (IsLoaded)
				Application.Current.Dispatcher.Invoke(action);
			else
				action();
		}

		public bool IsShortcut {
			//get => attributes.HasFlag(FileAttributes.ReparsePoint) || (Type == FileNodeType.File && Extension.ToLower() == ".lnk");
			get => IsFileReparsePoint || (Extension == ".lnk");
		}

		public bool IsLeaf {
			//get => (virtualChildren != null ? (virtualChildren.Count == 0) : true);
			get => virtualChildren.Count == 0;
		}

		public Rgb24Color Color {
			get => Root.Document.Extensions[Extension].Color;
		}

		public Rectangle2S Rectangle {
			get => graphRectangle;
			set => graphRectangle = value;
		}

		Rectangle2I ITreemapItem.Rectangle {
			get => graphRectangle;
			set => graphRectangle = value;
		}

		public int ChildCount {
			get {
				//if (virtualChildren == null)
				//	return 0;
				/*if (Type == FileNodeType.Root) {
					RootNode root = ((RootNode) this);
					if (root.FreeSpace != null && !Root.Document.Settings.ShowFreeSpace)
						return virtualChildren.Count - 1;
				}*/
				/*if (Type == FileNodeType.Root && Root.FreeSpace != null && !Root.Document.Settings.ShowFreeSpace) {
					return VirtualChildren.Count - 1;
				}*/
				return virtualChildren.Count;
				/*if (virtualChildren == null)
					return 0;
				return virt
				return VirtualChildren.Count;*/
			}
		}

		public FileNode this[int index] {
			get {
				/*if (Type == FileNodeType.Root && Root.FreeSpace != null && !Root.Document.Settings.ShowFreeSpace) {
					if (index >= Root.FreeSpaceIndex)
						index++;
				}*/
				return virtualChildren[index];
			}
		}

		ITreemapItem ITreemapItem.this[int index] {
			get {
				/*if (Type == FileNodeType.Root && Root.FreeSpace != null && !Root.Document.Settings.ShowFreeSpace) {
					if (index >= Root.FreeSpaceIndex)
						index++;
				}*/
				return virtualChildren[index];
			}
		}

		protected void LoadChildren(RootNode root) {
			lock (virtualChildren) {
				if (!populated) {
					EnsureVisualChildren();

					foreach (FileNode child in virtualChildren) {
						if (child.Icon == null && IsExpanded && virtualChildren.Count <= 5000)
							child.LoadIcon(root);
						if (child.IsExpanded)
							((FolderNode) child).LoadChildren(root);
					}
					vi.children.AddRange(virtualChildren.OrderBy(n => n, root.Document.FileComparer));
					/*foreach (FileNode child in virtualChildren) {
						if (child.Type == FileNodeType.FreeSpace)
							child.IsHidden = !root.Document.Settings.ShowFreeSpace;
					}*/
				}
				populated = true;
			}
		}

		protected bool populated {
			get => bitfield.GetBit(15);
			set => bitfield.SetBit(15, value);
		}
		protected void UnloadChildren(RootNode root) {
			lock (virtualChildren) {
				if (populated) {
					populated = false;

					//Children.Clear();
					vi.children.Clear();
					foreach (FolderNode child in VirtualFolders) {
						if (child.visualInfo != null) {
							if (child.populated)
								child.UnloadChildren(root);
							child.UnloadVisuals();
						}
					}
				}
				populated = false;
				//UnloadVisuals(false);
			}
		}

		public IEnumerable<FolderNode> VirtualFolders {
			get {
				if (Type == FileNodeType.FileCollection)
					yield break;
				foreach (FileNode node in VirtualChildren) {
					if (node is FolderNode folder)
						yield return folder;
				}
			}
		}

		public IEnumerable<FileNode> VirtualFiles {
			get {
				if (Type != FileNodeType.FileCollection)
					yield break;
				foreach (FileNode node in VirtualChildren) {
					if (node.Type == FileNodeType.File)
						yield return node;
				}
			}
		}

		protected void OnExpanding() {
			RootNode root = Root;
			if (!populated)
				LoadChildren(root);
			else
				Sort(root, false);
			foreach (FileNode child in VirtualChildren)
				child.LoadIcon(root);
		}

		protected void OnCollapsing() {
			UnloadChildren(Root);
			/*if (vi.children != null) {
				FileNode[] removedChildren = visualInfo.children.ToArray();
				visualInfo.children.Clear();
				foreach (FileNode child in removedChildren) {
					child.UnloadVisuals();
				}
			}*/
		}
		protected void EnsureVisualChildren() {
			if (vi.children == null)
				vi.children = new FileNodeCollection((FolderNode) this);
		}

		internal void Sort(RootNode root, bool sortSubChildren) {
			lock (virtualChildren) {
				if (!populated || visualInfo?.children == null || !visualInfo.children.Any())
					return;

				/*if (IsRoot && Root.FreeSpace != null) {
					vi.children.RemoveAt(Root.FreeSpaceIndex);
				}

				return;*/

				/*var query = visualInfo.children
					.Select((item, index) => (Item: item, Index: index));
				//query = root.Document.FileComparer.SortDirection == ListSortDirection.Descending
				//	? query.OrderByDescending(tuple => tuple.Item, root.Document.FileComparer)
				query = query.OrderBy(tuple => tuple.Item, root.Document.FileComparer);

				var map = query.Select((tuple, index) => (OldIndex: tuple.Index, NewIndex: index))
					.Where(o => o.OldIndex != o.NewIndex);
				using (var enumerator = map.GetEnumerator()) {
					if (enumerator.MoveNext()) {
						visualInfo.children.Move(enumerator.Current.NewIndex, enumerator.Current.OldIndex);
					}
				}
				if (sortSubChildren) {
					foreach (FileNode child in visualInfo.children) {
						if (child.IsExpanded && child is FolderNode subfolder)
							subfolder.Sort(root, true);
					}
				}*/
				/*foreach (var (OldIndex, NewIndex) in map) {
					visualInfo.children.Move(NewIndex, OldIndex);
				}*/
				/*using (var enumerator = map.GetEnumerator()) {
					if (enumerator.MoveNext()) {
						Move(enumerator.Current.OldIndex, enumerator.Current.NewIndex);
					}
				}*/

				//visualInfo.children.CustomSort = null;
				//visualInfo.children.CustomSort = root.Document.FileComparer;
				//visualInfo.children.Refresh();
				//root.Document.FileComparer.UpdateCollectionView(this, visualInfo.children.View);
				visualInfo.children.Sort(root.Document.FileComparer.Compare);
				//visualInfo.children.View.LiveSortingProperties.Clear();
				//visualInfo.children.View.LiveSortingProperties.Add(root.Document.FileComparer.SortProperty);
				if (sortSubChildren) {
					foreach (FileNode child in visualInfo.children) {
						if (child.IsExpanded && child is FolderNode subfolder)
							subfolder.Sort(root, true);
					}
				}

				/*FileNodeComparer comparer = root.Document.FileComparer;
				FileNode first = Children[0];
				//var sorted = new List<WinDirNode>(Children.Cast<WinDirNode>());
				//sorted.Sort(root.SortCompare);
				if (sortSubChildren && first.IsExpanded && first is FolderNode firstFolder)
					firstFolder.Sort(root, sortSubChildren);
				for (int i = 1; i < Children.Count; i++) {
					FileNode a = Children[i];

					int index;
					for (index = i; index > 0; index--) {
						if (!comparer.ShouldSort(a, Children[index - 1]))
							break;
					}
					if (i != index) {
						vi.children.Move(index, i);
						//vi.children.RemoveAt(i);
						//vi.children.Insert(index, a);
					}

					if (sortSubChildren && a.IsExpanded && a is FolderNode aFolder)
						aFolder.Sort(root, sortSubChildren);
				}*/
			}
		}
	}
}
