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
	public abstract partial class FileNodeBase : INotifyPropertyChanged, ITreemapItem {
		protected static readonly List<FileNodeBase> EmptyVirtualChildren = new List<FileNodeBase>();
		protected static readonly EmptyFileNodeCollection EmptyChildren = new EmptyFileNodeCollection();
		
		private string name;

		protected long size;
		protected DateTime lastChangeTime;
		internal FolderNode virtualParent;
		private Rectangle2S graphRectangle;
		
		protected FileNodeBase(INtfsNode node, FileNodeType type, FileNodeFlags rootType)
			: this(GetFileName(node.FullName, type), type, rootType)
		{
			Attributes = node.Attributes;
			//creationTime = node.CreationTime;
			lastChangeTime = node.LastChangeTime;
			//lastAccessTime = node.LastAccessTime;

			if (!IsContainerType && !IsReparsePointFile)
				size = (long) node.Size;
		}

		protected FileNodeBase(FileSystemInfo info, FileNodeType type, FileNodeFlags rootType)
			: this(GetFileName(info.FullName, type), type, rootType)
		{
			Attributes = info.Attributes;
			//creationTime = info.CreationTime;
			lastChangeTime = info.LastWriteTime;
			//lastAccessTime = info.LastAccessTime;

			if (!IsContainerType && !IsReparsePointFile && info is FileInfo fileInfo)
				size = fileInfo.Length;
		}

		protected FileNodeBase(IFileFindData find, FileNodeType type, FileNodeFlags rootType)
			: this(GetFileName(find.FullName, type), type, rootType)
		{
			Attributes = find.Attributes;
			//creationTime = find.CreationTime;
			lastChangeTime = find.LastWriteTime;
			//lastAccessTime = find.LastAccessTime;

			if (!IsContainerType && !IsReparsePointFile)
				size = find.Size;
		}

		protected FileNodeBase(string name, FileNodeType type, FileNodeFlags rootType) {
			if (type == FileNodeType.Volume) {
				if (!name.EndsWith(@"\") && !name.EndsWith("/"))
					name += '\\';
			}
			this.name = name;
			Type = type;
			if (rootType != FileNodeFlags.None) {
				IsFileRootType = rootType.HasFlag(FileNodeFlags.FileRootType);
				IsAbsoluteRootType = rootType.HasFlag(FileNodeFlags.AbsoluteRootType);
			}
		}

		public FileNodeBase(FileNodeBase node, FolderNode parent) {
			virtualParent = parent;
			type = node.type;
			name = node.name;
			flags = node.flags;
			lastChangeTime = node.lastChangeTime;
			//Attributes = node.Attributes;
			size = node.size;
			graphRectangle = node.graphRectangle;
			if (node.visualInfo != null) {
				vi.icon = node.vi.icon;
				//vi.isExpanded = node.vi.isExpanded;
				//vi.isSelected = node.vi.isSelected;
			}
		}

		private static string GetFileName(string path, FileNodeType type) {
			if (type == FileNodeType.Volume)
				return PathUtils.TrimSeparatorDotEnd(path);
			return GetFileName(path);
		}
		private static string GetFileName(string path) {
			int length = path.Length;
			for (int i = length; --i >= 0;) {
				char ch = path[i];
				if (ch == System.IO.Path.DirectorySeparatorChar || ch == System.IO.Path.AltDirectorySeparatorChar || ch == System.IO.Path.VolumeSeparatorChar)
					return PathUtils.TrimSeparatorDotEnd(path.Substring(i + 1, length - i - 1));

			}
			return PathUtils.TrimSeparatorDotEnd(path);
		}

		public string Name {
			get => name;
		}
		public FolderNode VirtualParent {
			get => virtualParent;
		}
		public bool HasVirtualChildren {
			get => VirtualChildren.Count != 0;
		}
		internal virtual List<FileNodeBase> VirtualChildren {
			get => EmptyVirtualChildren;
		}
		public virtual string Extension {
			get => string.Empty;
		}
		public virtual ExtensionRecord ExtensionRecord {
			get => ExtensionRecord.Empty;
		}
		public virtual int ItemCount {
			get => -1;
		}
		public virtual int FileCount {
			get => -1;
		}
		public virtual int SubdirCount {
			get => -1;
		}
		public RootNode Root {
			get {
				if (IsAbsoluteRootType)
					return (RootNode) this;
				else
					return virtualParent.Root;
			}
		}
		public string Path {
			get {
				if (IsFileRootType)
					return ((RootNode) this).RootPath;
				else if (IsFileType)
					return PathUtils.CombineNoChecks(virtualParent.Path, name);
				else if (type != FileNodeType.Computer)
					return virtualParent.Path;
				else
					return "Computer";
			}
		}
		public bool IsVirtualRoot {
			get => virtualParent == null;
		}
		public double Percent {
			get {
				if (IsAbsoluteRootType)
					return 1d;
				else if (virtualParent.size == 0)
					return 1d;
				else
					return (double) size / virtualParent.size;
			}
		}
		public bool IsEmptySize {
			get => size == 0;
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
		public string AttributesString {
			get {
				StringBuilder str = new StringBuilder();
				if (IsReadOnlyFile)		str.Append('R');
				if (IsHiddenFile)		str.Append('H');
				if (IsSystemFile)		str.Append('S');
				if (IsArchiveFile)		str.Append('A');
				if (IsCompressedFile)	str.Append('C');
				if (IsEncryptedFile)	str.Append('E');
				return str.ToString();
			}
		}
		protected bool IsLoaded {
			get => Parent != null || IsVirtualRoot;
		}
		protected bool InUIThread {
			get => (Dispatcher.CurrentDispatcher.Thread == Thread.CurrentThread);
		}

		public bool IsShortcut {
			get => IsReparsePointFile || (Extension == ".lnk");
		}
		public string Header {
			get {
				if (Type == FileNodeType.Volume || Type == FileNodeType.Computer)
					return ((RootNode) this).DisplayName;
				else
					return name;
			}
		}

		public IEnumerable<FileNodeBase> AncestorsInOrder() {
			return Ancestors().Reverse();
		}
		public IEnumerable<FileNodeBase> AncestorsInOrderAndSelf() {
			return AncestorsAndSelf().Reverse();
		}

		internal void LoadIcon(RootNode root, bool basicLoad = false) {
			string name;
			switch (Type) {
			case FileNodeType.FileCollection:
				vi.icon = IconCache.FileCollectionIcon;
				VisualPropertyChanged(nameof(Icon));
				return;
			case FileNodeType.FreeSpace:
				vi.icon = IconCache.FreeSpaceIcon;
				VisualPropertyChanged(nameof(Icon));
				return;
			case FileNodeType.Unknown:
				vi.icon = IconCache.UnknownSpaceIcon;
				VisualPropertyChanged(nameof(Icon));
				return;
			case FileNodeType.Computer:
				vi.icon = root.Document.Icons.CacheSpecialFolder(Environment.SpecialFolder.MyComputer,
					out name);
				if (vi.icon != null)
					((RootNode) this).displayName = name;
				VisualPropertiesChanged(
					nameof(Icon),
					nameof(Header),
					nameof(RootNode.DisplayName));
				return;
			case FileNodeType.Volume:
				vi.icon = IconCache.VolumeIcon;
				//VisualPropertyChanged(nameof(Icon));
				//root.Document.Icons.CacheIconAsync(Path, Attributes, LoadIconAndDisplayNameCallback);
				vi.icon = root.Document.Icons.CacheIcon(Path, Attributes, out name);
				if (vi.icon == null)
					vi.icon = IconCache.VolumeIcon;
				else
					((RootNode) this).displayName = name;
				VisualPropertiesChanged(
					nameof(Icon),
					nameof(Header),
					nameof(RootNode.DisplayName));
				return;
			case FileNodeType.File:
				vi.icon = ExtensionRecord.Icon ?? IconCache.FileIcon;
				break;
			default:
				vi.icon = IconCache.FolderIcon;
				break;
			}
			VisualPropertyChanged(nameof(Icon));
			if (!basicLoad)
				root.Document.Icons.CacheIconAsync(Path, Attributes, LoadIconCallback);
		}

		private void LoadIconCallback(ImageSource icon) {
			vi.icon = icon;
			if (icon == null) {
				if (type == FileNodeType.File)
					vi.icon = IconCache.FileIcon;
				else if (type == FileNodeType.Directory)
					vi.icon = IconCache.FolderIcon;
			}
			VisualPropertyChanged(nameof(Icon));
		}

		private void LoadIconAndDisplayNameCallback(ImageSource icon, string name) {
			vi.icon = icon ?? IconCache.FileIcon;
			if (icon == null) {
				if (type == FileNodeType.Volume)
					vi.icon = IconCache.VolumeIcon;
			}
			else {
				((RootNode) this).displayName = name;
			}
			VisualPropertiesChanged(
				nameof(Icon),
				nameof(Header),
				nameof(RootNode.DisplayName));
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

		protected void VisualInvokeIf(bool condition, Action action) {
			if (condition && IsLoaded)
				Application.Current.Dispatcher.Invoke(action);
			else
				action();
		}

		public bool IsLeaf {
			get => !IsContainerType;
		}

		public Rgb24Color Color {
			get {
				switch (Type) {
				case FileNodeType.File: return ExtensionRecord.Color;
				case FileNodeType.FreeSpace: return new Rgb24Color(100, 100, 100);
				case FileNodeType.Unknown: return new Rgb24Color(255, 255, 0);
				default: return Rgb24Color.Black;
				}
			}
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
				return VirtualChildren.Count;
				/*if (virtualChildren == null)
					return 0;
				return virt
				return VirtualChildren.Count;*/
			}
		}

		public FileNodeBase this[int index] {
			get {
				/*if (Type == FileNodeType.Root && Root.FreeSpace != null && !Root.Document.Settings.ShowFreeSpace) {
					if (index >= Root.FreeSpaceIndex)
						index++;
				}*/
				return VirtualChildren[index];
			}
		}

		ITreemapItem ITreemapItem.this[int index] {
			get {
				/*if (Type == FileNodeType.Root && Root.FreeSpace != null && !Root.Document.Settings.ShowFreeSpace) {
					if (index >= Root.FreeSpaceIndex)
						index++;
				}*/
				return VirtualChildren[index];
			}
		}

		
	}
}
