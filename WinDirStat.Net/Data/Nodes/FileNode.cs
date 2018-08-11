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
		private struct Int16Rect {
			public static readonly Int16Rect Empty = new Int16Rect();

			public ushort X;
			public ushort Y;
			public ushort Width;
			public ushort Height;

			public Int16Rect(int x, int y, int width, int height) {
				X = (ushort) x;
				Y = (ushort) y;
				Width = (ushort) width;
				Height = (ushort) height;
			}

			public static implicit operator Int16Rect(Rectangle rect) {
				return new Int16Rect(
					rect.X == -1 ? ushort.MaxValue : (ushort) rect.X,
					rect.Y == -1 ? ushort.MaxValue : (ushort) rect.Y,
					(ushort) rect.Width,
					(ushort) rect.Height);
			}

			public static implicit operator Rectangle(Int16Rect rect) {
				return new Rectangle(
					rect.X == ushort.MaxValue ? -1 : rect.X,
					rect.Y == ushort.MaxValue ? -1 : rect.Y,
					rect.Width,
					rect.Height);
			}
		}
		protected static IReadOnlyList<FileNode> EmptyList = new List<FileNode>();
		protected static EmptyFileNodeCollection EmptyChildren = new EmptyFileNodeCollection();

#if !COMPRESSED_DATA
		protected string name;
#else
		protected char[] cName;

		protected string name {
			get => new string(cName);
			set => cName = value.ToCharArray();
		}
#endif
		protected long size;
		protected FileAttributes attributes;
		//private DateTime creationTime;
		//private DateTime lastAccessTime;
		protected DateTime lastChangeTime;
		internal FolderNode virtualParent;
		//protected System.Drawing.Rectangle graphRectangle;
		private Rectangle2S graphRectangle;

		public static string CorrectPath(string path) {
			return "";
		}

		protected FileNode() {

		}

		public FileNode(INtfsNode node) : this(node.FullName) {
			attributes = node.Attributes;
			//creationTime = node.CreationTime;
			lastChangeTime = node.LastChangeTime;
			//lastAccessTime = node.LastAccessTime;

			if (!attributes.HasFlag(FileAttributes.Directory) &&
				!attributes.HasFlag(FileAttributes.ReparsePoint))
				size = (long) node.Size;
		}

		public FileNode(FileSystemInfo info) : this(info.FullName) {
			attributes = info.Attributes;
			//creationTime = info.CreationTime;
			lastChangeTime = info.LastWriteTime;
			//lastAccessTime = info.LastAccessTime;

			if (!attributes.HasFlag(FileAttributes.Directory) &&
				!attributes.HasFlag(FileAttributes.ReparsePoint) &&
				info is FileInfo fileInfo)
				size = fileInfo.Length;
		}

		public FileNode(IFileFindData data) : this(data.FullName) {
			attributes = data.Attributes;
			//creationTime = data.CreationTime;
			lastChangeTime = data.LastWriteTime;
			//lastAccessTime = data.LastAccessTime;

			if (!data.IsDirectory && !attributes.HasFlag(FileAttributes.ReparsePoint))
				size = data.Size;
		}

		protected FileNode(string path) {
			if (!System.IO.Path.IsPathRooted(path))
				path = System.IO.Path.GetFullPath(path);
			else
				path = PathUtils.TrimSeparatorDotEnd(path);
			name = System.IO.Path.GetFileName(path);
		}

		public virtual WinDirDocument Document {
			get => Root.Document;
		}

		public FolderNode VirtualParent {
			get => virtualParent;
		}
		public virtual FileNodeType Type {
			get => FileNodeType.File;
		}
		public virtual bool HasChildren {
			get => false;
		}

		public int ItemCount {
			get => Math.Max(-1, FileCount + SubdirCount);
		}

		public virtual int FileCount {
			get => -1;
		}
		public virtual int SubdirCount {
			get => -1;
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
		public FileAttributes Attributes {
			get => attributes;
		}
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

		public virtual IReadOnlyList<FileNode> VirtualChildren {
			get => EmptyList;
		}

		public string Extension {
			get {
				if (Type == FileNodeType.File) {
					string ext = System.IO.Path.GetExtension(name).ToLower();
					return (ext.Length != 0 ? ext : ".");
				}
				return "";
			}
		}

		public string AttributesString {
			get {
				string s = "";
				if (attributes.HasFlag(FileAttributes.ReadOnly)) s += "R";
				if (attributes.HasFlag(FileAttributes.Hidden)) s += "H";
				if (attributes.HasFlag(FileAttributes.System)) s += "S";
				if (attributes.HasFlag(FileAttributes.Archive)) s += "A";
				if (attributes.HasFlag(FileAttributes.Compressed)) s += "C";
				if (attributes.HasFlag(FileAttributes.Encrypted)) s += "E";
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
				root.Document.Icons.CacheIconAsync(Path, attributes, LoadIconCallback);
		}

		private void LoadIconCallback(ImageSource icon) {
			this.vi.icon = icon ?? Root.Document.Icons.DefaultFileIcon;
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

		bool ITreemapItem.IsLeaf {
			get => !HasChildren;
		}

		public bool IsShortcut {
			get => attributes.HasFlag(FileAttributes.ReparsePoint) || (Type == FileNodeType.File && Extension.ToLower() == ".lnk");
		}

		Rgb24Color ITreemapItem.Color {
			get => Root.Document.Extensions[Extension].Color;
		}

		Rectangle2I ITreemapItem.Rectangle {
			get => graphRectangle;
			set => graphRectangle = (Rectangle2S) value;
		}

		int ITreemapItem.ChildCount {
			get {
				if (Type == FileNodeType.Root && Root.FreeSpace != null && !Root.Document.Settings.ShowFreeSpace) {
					return VirtualChildren.Count - 1;
				}
				return VirtualChildren.Count;
			}
		}

		ITreemapItem ITreemapItem.this[int index] {
			get {
				if (Type == FileNodeType.Root && Root.FreeSpace != null && !Root.Document.Settings.ShowFreeSpace) {
					if (index >= Root.FreeSpaceIndex)
						index++;
				}
				return VirtualChildren[index];
			}
		}
	}
}
