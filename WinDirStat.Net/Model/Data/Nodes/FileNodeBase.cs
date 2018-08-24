using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using WinDirStat.Net.Drawing;
using WinDirStat.Net.Model.Data;
using WinDirStat.Net.Model.Data.Extensions;
using WinDirStat.Net.Settings.Geometry;
using WinDirStat.Net.Utils;
using WinDirStat.Net.Utils.Native;

namespace WinDirStat.Net.Model.Data.Nodes {
	public abstract partial class FileNodeBase : ITreemapItem, IComparable<FileNodeBase>, IComparable {
		protected static readonly List<FileNodeBase> EmptyVirtualChildren = new List<FileNodeBase>();
		
		private string name;

		protected long size;
		protected DateTime lastChangeTime;
		internal FolderNode virtualParent;
		private Rectangle2S graphRectangle;


		public event FileNodeEventHandler Changed;

		public bool IsWatched {
			get => Changed != null;
		}

		protected void RaiseChanged(FileNodeEventArgs e) {
			Changed?.Invoke(this, e);
		}
		protected void RaiseChanged(FileNodeAction action) {
			Changed?.Invoke(this, new FileNodeEventArgs(action));
		}
		protected void RaiseChanged(FileNodeAction action, int index) {
			Changed?.Invoke(this, new FileNodeEventArgs(action, index));
		}
		protected void RaiseChanged(FileNodeAction action, List<FileNodeBase> children, int index) {
			Changed?.Invoke(this, new FileNodeEventArgs(action, children, index));
		}
		protected void RaiseChanged(FileNodeAction action, FileNodeBase child, int index) {
			Changed?.Invoke(this, new FileNodeEventArgs(action, child, index));
		}

		public object GetView() {
			FileNodeEventArgs e = new FileNodeEventArgs(FileNodeAction.GetView);
			RaiseChanged(e);
			return e.View;
		}

		public TView GetView<TView>() {
			FileNodeEventArgs e = new FileNodeEventArgs(FileNodeAction.GetView);
			RaiseChanged(e);
			return (TView) e.View;
		}

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

		protected FileNodeBase(Win32.Win32FindData find, FileNodeType type, FileNodeFlags rootType)
			: this(find.cFileName, type, rootType)
		{
			Attributes = find.dwFileAttributes;
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
			/*internal set {
				virtualParent = value;
				if (virtualParent != null)
					Debug.WriteLine($"{this} <= {virtualParent}");
				else
					Debug.WriteLine($"{this} <= null");
			}*/
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
					return 0d;
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

		public bool IsShortcut {
			get => IsReparsePointFile || (Extension == ".lnk");
		}

		/*public IEnumerable<FileNodeBase> AncestorsInOrder() {
			return Ancestors().Reverse();
		}

		public IEnumerable<FileNodeBase> AncestorsInOrderAndSelf() {
			return AncestorsAndSelf().Reverse();
		}*/

	


		/*protected void VisualPropertiesChanged(params string[] names) {
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
		}*/

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
			get => VirtualChildren.Count;
		}

		public FileNodeBase this[int index] {
			get => VirtualChildren[index];
		}

		ITreemapItem ITreemapItem.this[int index] {
			get => VirtualChildren[index];
		}

		public int CompareTo(FileNodeBase other) {
			int diff = other.size.CompareTo(size);
			if (diff == 0)
				return string.Compare(name, other.name, true);
			return diff;
		}

		int IComparable.CompareTo(object obj) {
			return CompareTo((FileNodeBase) obj);
		}

		public override string ToString() {
			return name;
		}
	}
}
