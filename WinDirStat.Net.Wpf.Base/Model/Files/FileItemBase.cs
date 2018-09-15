using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

using WinDirStat.Net.Model.Extensions;
using WinDirStat.Net.Rendering;
using WinDirStat.Net.Structures;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Model.Files {
	/// <summary>The abstract base class for all file tree items.</summary>
	[Serializable]
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public abstract class FileItemBase : IComparable<FileItemBase>, IComparable, ITreemapItem {

		#region Constants

		/// <summary>
		/// The static list of empty children to use for empty lists. This should never be modified.
		/// </summary>
		private protected static readonly List<FileItemBase> EmptyChildren = new List<FileItemBase>();

		#endregion

		#region Fields

		// Fields and property fields are both stored here to keep track of memory usage
		// ViewModel Events also has one event that will take up space

		/// <summary>Gets the type of this file item.</summary>
		public FileItemType Type { get; }
		/// <summary>The volatile state of the file.</summary>
		private protected FileItemStates state;
		/// <summary>The constant flags of the file.</summary>
		private protected FileItemFlags flags;

		/// <summary>Gets the parent containing this file.</summary>
		public FolderItem Parent { get; internal set; }
		/// <summary>
		/// Gets the UTC time of when the file was last written to.<para/>
		/// If this is a container, it returns the most recent time of all children.
		/// </summary>
		public DateTime LastWriteTimeUtc { get; private protected set; }
		/// <summary>Gets the name of the file.</summary>
		public string Name { get; }
		/// <summary>Gets the total size of the file and all of its children.</summary>
		public long Size { get; private protected set; }
		/// <summary>Gets the rectangle of the file for drawing in the treemap.</summary>
		public Rectangle2S Rectangle { get; set; }

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="FileItemBase"/> without any file info.</summary>
		/// 
		/// <param name="name">The name of the item.</param>
		/// <param name="type">The type of the item.</param>
		/// <param name="flags">The flags for the item (except <see cref="FileAttributes"/>).</param>
		private protected FileItemBase(string name, FileItemType type, FileItemFlags flags) {
			if (flags != FileItemFlags.None)
				this.flags |= (flags & FileItemFlags.TypeFlagsMask);
			if (type == FileItemType.Volume)
				name = PathUtils.AddDirectorySeparator(name);

			Name = name;
			Type = type;
			state |= FileItemStates.Exists;
		}

		/// <summary>Constructs the <see cref="FileItemBase"/> with a <see cref="FileSystemInfo"/>.</summary>
		/// 
		/// <param name="info">The file information.</param>
		/// <param name="type">The type of the item.</param>
		/// <param name="flags">The flags for the item (except <see cref="FileAttributes"/>).</param>
		private protected FileItemBase(FileSystemInfo info, FileItemType type, FileItemFlags flags)
			: this(info.Name, type, flags)
		{
			LastWriteTimeUtc = info.LastWriteTimeUtc;
			Attributes = info.Attributes;

			if (!info.Attributes.HasFlag(FileAttributes.Directory) &&
				!info.Attributes.HasFlag(FileAttributes.ReparsePoint) &&
				info is FileInfo fileInfo)
				Size = fileInfo.Length;

			if (!info.Exists) {
				state &= ~FileItemStates.Exists;
				Attributes = 0;
				LastWriteTimeUtc = DateTime.MinValue;
			}
		}

		/// <summary>Constructs the <see cref="FileItemBase"/> with a <see cref="IScanFileInfo"/>.</summary>
		/// 
		/// <param name="info">The file information.</param>
		/// <param name="type">The type of the item.</param>
		/// <param name="flags">The flags for the item (except <see cref="FileAttributes"/>).</param>
		private protected FileItemBase(IScanFileInfo info, FileItemType type, FileItemFlags flags)
			: this(info.Name, type, flags)
		{
			LastWriteTimeUtc = info.LastWriteTimeUtc;
			Attributes = info.Attributes;

			if (!info.IsDirectory && !info.IsSymbolicLink)
				Size = info.Size;
		}

		#endregion

		#region ViewModel Events

		// TODO: Eventually determine if we can move this to FolderItem

		/// <summary>Notifies any view models watching this item of important changes to the file.</summary>
		[field: NonSerialized]
		public event FileItemEventHandler Changed;

		/// <summary>Gets if the file is being watched by a view model.</summary>
		public bool IsWatched => Changed != null;

		protected void RaiseChanged(FileItemEventArgs e) {
			Changed?.Invoke(this, e);
		}
		protected void RaiseChanged(FileItemAction action) {
			Changed?.Invoke(this, new FileItemEventArgs(action));
		}
		protected void RaiseChanged(FileItemAction action, int index) {
			Changed?.Invoke(this, new FileItemEventArgs(action, index));
		}
		protected void RaiseChanged(FileItemAction action, List<FileItemBase> children, int index) {
			Changed?.Invoke(this, new FileItemEventArgs(action, children, index));
		}
		protected void RaiseChanged(FileItemAction action, IEnumerable<FileItemBase> children, int index) {
			Changed?.Invoke(this, new FileItemEventArgs(action, children, index));
		}
		protected void RaiseChanged(FileItemAction action, FileItemBase child, int index) {
			Changed?.Invoke(this, new FileItemEventArgs(action, child, index));
		}

		public object GetViewModel() {
			FileItemEventArgs e = new FileItemEventArgs(FileItemAction.GetViewModel);
			RaiseChanged(e);
			return e.ViewModel;
		}

		public TViewModel GetViewModel<TViewModel>() where TViewModel : class {
			FileItemEventArgs e = new FileItemEventArgs(FileItemAction.GetViewModel);
			RaiseChanged(e);
			return e.ViewModel as TViewModel;
		}

		#endregion

		#region Virtual Methods
		
		/// <summary>Refreshes the file. Returns true if it still exists.</summary>
		/// 
		/// <returns>True if the file still exists.</returns>
		public virtual bool Refresh() => true;

		/// <summary>Checks if the file still exists.</summary>
		/// 
		/// <returns>True if the file exists.</returns>
		public virtual bool CheckExists() => true;

		/// <summary>
		/// Must be called at the end of <see cref="Refresh"/> in order to raise a Refresh change.<para/>
		/// For file types only.
		/// </summary>
		/// 
		/// <param name="existsNew">True if the file exists.</param>
		/// <returns>The passed <paramref name="existsNew"/> parameter.</returns>
		protected bool RefreshFinal(bool existsNew) {
			Exists = existsNew;
			if (IsWatched)
				RaiseChanged(FileItemAction.Refreshed);
			return existsNew;
		}

		/// <summary>
		/// Must be called at the end of <see cref="CheckExists"/> in order to raise a Exists change.<para/>
		/// For file types only.
		/// </summary>
		/// 
		/// <param name="existsNew">True if the file exists.</param>
		/// <returns>The passed <paramref name="existsNew"/> parameter.</returns>
		protected bool CheckExistsFinal(bool existsNew) {
			if (Exists != existsNew) {
				Exists = existsNew;
				if (IsWatched)
					RaiseChanged(FileItemAction.Exists);
			}
			return existsNew;
		}

		#endregion

		#region Virtual Properties

		/// <summary>Returns true if this folder has any children.</summary>
		public virtual bool HasChildren => false;
		/// <summary>
		/// Gets the list of children for this folder.<para/>
		/// This list is only for fast access and should never be modified.
		/// </summary>
		internal virtual List<FileItemBase> Children => EmptyChildren;

		/// <summary>
		/// Gets the number of files and directories this folder contains. Returns -1 if this is not a
		/// container.
		/// </summary>
		public virtual int ItemCount => -1;
		/// <summary>
		/// Gets the number of files this folder contains. Returns -1 if this is not a container.
		/// </summary>
		public virtual int FileCount {
			get => -1;
			protected set => throw InvalidSet();
		}
		/// <summary>
		/// Gets the number of directories this folder contains. Returns -1 if this is not a container.
		/// </summary>
		public virtual int SubdirCount {
			get => -1;
			protected set => throw InvalidSet();
		}

		/// <summary>
		/// Gets the extension item of the file.<para/>
		/// Returns <see cref="ExtensionItem.Empty"/> if this is not a file.
		/// </summary>
		public virtual ExtensionItem ExtensionItem => ExtensionItem.NotAFile;
		/// <summary>
		/// Gets the extension of the file. Empty file extensions are always returned with a '.'.<para/>
		/// Returns <see cref="string.Empty"/> if this is not a file.
		/// </summary>
		public virtual string Extension => string.Empty;

		#endregion

		#region Properties

		/// <summary>Finds the absolute root node.</summary>
		public RootItem Root {
			get {
				if (IsAbsoluteRootType)
					return (RootItem) this;
				else
					return Parent.Root;
			}
		}

		/// <summary>Gets the file root node</summary>
		public FolderItem FileRoot {
			get {
				if (IsFileRootType)
					return (RootItem) this;
				else
					return Parent.Root;
			}
		}

		/// <summary>
		/// Gets the folder/drive parent of the node.<para/>
		/// This is used to skip FileCollection containers.
		/// </summary>
		public FolderItem FileParent {
			get {
				if (Parent != null && Parent.Type == FileItemType.FileCollection)
					return Parent.Parent;
				return Parent;
			}
		}

		/// <summary>
		/// Gets the local time of when the file was last written to.<para/>
		/// If this is a container, it returns the most recent time of all children.
		/// </summary>
		public DateTime LastWriteTime => LastWriteTimeUtc.ToLocalTime();

		/// <summary>Gets the visible level of the item in the tree.</summary>
		public int VisibleLevel => Parent != null ? Parent.VisibleLevel + 1 : 0;

		/// <summary>Gets the file level of the item in the tree.</summary>
		public int FileLevel => Parent != null ? (Parent.FileLevel + (Parent.IsFileType ? 1 : 0)) : 0;

		/// <summary>Gets the path of the file.</summary>
		public string FullName {
			get {
				if (IsFileRootType)
					return ((RootItem) this).RootPath;
				else if (IsFileType)
					return PathUtils.CombineNoChecks(Parent.FullName, Name);
				else if (Type != FileItemType.Computer)
					return Parent.FullName;
				else
					return "::{20d04fe0-3aea-1069-a2d8-08002b30309d}";// Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
			}
		}

		/// <summary>Gets this file's size relative to the parent's size.</summary>
		public double Percent {
			get {
				if (IsAbsoluteRootType)
					return 1d;
				else if (Parent.Size == 0)
					return 0d;
				else
					return (double) Size / Parent.Size;
			}
		}

		/// <summary>Gets if the file has a size of zero.</summary>
		public bool IsEmptySize => Size == 0;
		
		/// <summary>Gets if this file is a shortcut.</summary>
		public bool IsShortcut => IsReparsePointFile || (Extension == ".lnk");

		/// <summary>Gets the attributes for this file.</summary>
		public FileAttributes Attributes {
			get {
				FileAttributes attr = 0;
				if (IsReadOnlyFile)		attr |= FileAttributes.ReadOnly;
				if (IsHiddenFile)		attr |= FileAttributes.Hidden;
				if (IsSystemFile)		attr |= FileAttributes.System;
				if (IsArchiveFile)		attr |= FileAttributes.Archive;
				if (IsCompressedFile)	attr |= FileAttributes.Compressed;
				if (IsEncryptedFile)	attr |= FileAttributes.Encrypted;
				if (IsReparsePointFile)	attr |= FileAttributes.ReparsePoint;
				if (IsTemporaryFile)	attr |= FileAttributes.Temporary;
				return attr;
			}
			private protected set {
				IsReadOnlyFile      = value.HasFlag(FileAttributes.ReadOnly);
				IsHiddenFile        = value.HasFlag(FileAttributes.Hidden);
				IsSystemFile        = value.HasFlag(FileAttributes.System);
				IsArchiveFile       = value.HasFlag(FileAttributes.Archive);
				IsCompressedFile    = value.HasFlag(FileAttributes.Compressed);
				IsEncryptedFile     = value.HasFlag(FileAttributes.Encrypted);
				IsReparsePointFile  = value.HasFlag(FileAttributes.ReparsePoint);
				IsTemporaryFile     = value.HasFlag(FileAttributes.Temporary);
			}
		}
		/// <summary>Gets the file attributes as a displayable string.</summary>
		public string AttributesString {
			get {
				StringBuilder str = new StringBuilder();
				if (IsReadOnlyFile) str.Append('R');
				if (IsHiddenFile) str.Append('H');
				if (IsSystemFile) str.Append('S');
				if (IsArchiveFile) str.Append('A');
				if (IsCompressedFile) str.Append('C');
				if (IsEncryptedFile) str.Append('E');
				return str.ToString();
			}
		}
		/// <summary>Gets the file attributes as an integer to use for sorting.</summary>
		public short SortAttributes => unchecked((short) (flags & FileItemFlags.SortAttributesMask));

		#endregion

		#region Constant Flags

		/// <summary>This file has the <see cref="FileAttributes.ReadOnly"/> attribute.</summary>
		public bool IsReadOnlyFile {
			get => flags.HasFlag(FileItemFlags.ReadOnly);
			private set => flags = flags.SetFlag(FileItemFlags.ReadOnly, value);
		}
		/// <summary>This file has the <see cref="FileAttributes.Hidden"/> attribute.</summary>
		public bool IsHiddenFile {
			get => flags.HasFlag(FileItemFlags.Hidden);
			private set => flags = flags.SetFlag(FileItemFlags.Hidden, value);
		}
		/// <summary>This file has the <see cref="FileAttributes.System"/> attribute.</summary>
		public bool IsSystemFile {
			get => flags.HasFlag(FileItemFlags.System);
			private set => flags = flags.SetFlag(FileItemFlags.System, value);
		}
		/// <summary>This file has the <see cref="FileAttributes.Archive"/> attribute.</summary>
		public bool IsArchiveFile {
			get => flags.HasFlag(FileItemFlags.Archive);
			private set => flags = flags.SetFlag(FileItemFlags.Archive, value);
		}
		/// <summary>This file has the <see cref="FileAttributes.Compressed"/> attribute.</summary>
		public bool IsCompressedFile {
			get => flags.HasFlag(FileItemFlags.Compressed);
			private set => flags = flags.SetFlag(FileItemFlags.Compressed, value);
		}
		/// <summary>This file has the <see cref="FileAttributes.Encrypted"/> attribute.</summary>
		public bool IsEncryptedFile {
			get => flags.HasFlag(FileItemFlags.Encrypted);
			private set => flags = flags.SetFlag(FileItemFlags.Encrypted, value);
		}
		/// <summary>This file has the <see cref="FileAttributes.ReparsePoint"/> attribute.</summary>
		public bool IsReparsePointFile {
			get => flags.HasFlag(FileItemFlags.ReparsePoint);
			private set => flags = flags.SetFlag(FileItemFlags.ReparsePoint, value);
		}
		/// <summary>This file has the <see cref="FileAttributes.Temporary"/> attribute.</summary>
		public bool IsTemporaryFile {
			get => flags.HasFlag(FileItemFlags.Temporary);
			private set => flags = flags.SetFlag(FileItemFlags.Temporary, value);
		}

		/// <summary>True if this directory is case sensitive.</summary>
		public bool CaseSensitive {
			get => flags.HasFlag(FileItemFlags.CaseSensitive);
			private protected set => flags = flags.SetFlag(FileItemFlags.CaseSensitive, value);
		}
		/// <summary>True if this type can contain other nodes.</summary>
		public bool IsContainerType {
			get => flags.HasFlag(FileItemFlags.ContainerType);
			private set => flags = flags.SetFlag(FileItemFlags.ContainerType, value);
		}
		/// <summary>True if this is a real file system node.</summary>
		public bool IsFileType {
			get => flags.HasFlag(FileItemFlags.FileType);
			private set => flags = flags.SetFlag(FileItemFlags.FileType, value);
		}
		/// <summary>True if this node contains the root path for the file tree.</summary>
		public bool IsFileRootType {
			get => flags.HasFlag(FileItemFlags.FileRootType);
			private set => flags = flags.SetFlag(FileItemFlags.FileRootType, value);
		}
		/// <summary>True if this node never has a parent. A computer node is always of this type.</summary>
		public bool IsAbsoluteRootType {
			get => flags.HasFlag(FileItemFlags.AbsoluteRootType);
			private set => flags = flags.SetFlag(FileItemFlags.AbsoluteRootType, value);
		}
		/// <summary>True if this node is an absolute or file root.</summary>
		public bool IsAnyRootType {
			get => (flags & (FileItemFlags.FileRootType | FileItemFlags.AbsoluteRootType)) != 0;
		}

		#endregion

		#region State Flags

		/// <summary>
		/// This lock is static because these values are not accessed often and only within two threads.<para/>
		/// (mostly in just the async thread)
		/// </summary>
		private static readonly object stateLock = new object();

		/// <summary>Gets if the folder and all its children have been fully scanned.</summary>
		public bool IsDone {
			get {
				lock (stateLock)
					return state.HasFlag(FileItemStates.Done);
			}
			private protected set {
				lock (stateLock)
					state = state.SetFlag(FileItemStates.Done, value);
			}
		}
		/// <summary>Gets if the folder needs to be validated.</summary>
		public bool IsInvalidated {
			get {
				lock (stateLock)
					return state.HasFlag(FileItemStates.Invalidated);
			}
			private protected set {
				lock (stateLock) {
					// Perform two operations in the same lock
					if (value)
						state &= ~FileItemStates.Done;
					state = state.SetFlag(FileItemStates.Invalidated, value);
				}
			}
		}
		/// <summary>Gets if the folder is currently validating.</summary>
		public bool IsValidating {
			get {
				lock (stateLock)
					return state.HasFlag(FileItemStates.Validating);
			}
			private protected set {
				lock (stateLock) {
					// Perform two operations in the same lock
					if (value)
						state &= ~FileItemStates.Invalidated;
					state = state.SetFlag(FileItemStates.Validating, value);
				}
			}
		}
		/// <summary>Gets if the file exists in the system.</summary>
		public bool Exists {
			get {
				lock (stateLock)
					return state.HasFlag(FileItemStates.Exists);
			}
			private protected set {
				bool changed = false;
				lock (stateLock) {
					changed = value != Exists;
					state = state.SetFlag(FileItemStates.Exists, value);
				}
				if (changed && IsWatched)
					RaiseChanged(FileItemAction.Exists);
			}
		}

		#endregion

		#region ITreemapItem Implementation

		/// <summary>Gets if this treemap item is a leaf that should be drawn.</summary>
		public bool IsLeaf => !IsContainerType;

		/// <summary>Gets the color of this treemap item leaf.</summary>
		public Rgb24Color Color {
			get {
				switch (Type) {
				case FileItemType.File: return ExtensionItem.Color;
				case FileItemType.FreeSpace: return new Rgb24Color(100, 100, 100);
				case FileItemType.Unknown: return new Rgb24Color(255, 255, 0);
				default: return Rgb24Color.Black;
				}
			}
		}

		/// <summary>Gets the number of children in this treemap item.</summary>
		public virtual int ChildCount => 0;
		
		/// <summary>Gets the child at the specified index in the treemap item.</summary>
		public virtual FileItemBase this[int index] => throw InvalidCall();
		/// <summary>Gets the child at the specified index in the treemap item.</summary>
		ITreemapItem ITreemapItem.this[int index] => this[index];

		/// <summary>Gets the number of children in this treemap item.</summary>
		Rectangle2I ITreemapItem.Rectangle {
			get => Rectangle;
			set => Rectangle = (Rectangle2S) value;
		}

		#endregion

		#region Exception Helpers

		/// <summary>
		/// Throws an <see cref="InvalidOperationException"/> stating the calling method cannot be called due
		/// to lack of support from this class.
		/// </summary>
		private Exception InvalidCall([CallerMemberName] string name = null) {
			return new InvalidOperationException($"{name} cannot be called because this item does not " +
				$"support it!");
		}
		/// <summary>
		/// Throws an <see cref="InvalidOperationException"/> stating the calling property cannot be set due
		/// to lack of support from this class.
		/// </summary>
		private Exception InvalidSet([CallerMemberName] string name = null) {
			return new InvalidOperationException($"{name} cannot be set because this item does not " +
				$"support it!");
		}

		#endregion

		#region IComparable Implementation

		/// <summary>Compares this file to another based on size in descending order.</summary>
		/// 
		/// <param name="other">The other file to compare to.</param>
		/// <returns>The comparison result.</returns>
		public int CompareTo(FileItemBase other) {
			int diff = other.Size.CompareTo(Size);
			if (diff == 0)
				return string.Compare(Name, other.Name, true);
			return diff;
		}

		/// <summary>Compares this file to another based on size in descending order.</summary>
		/// 
		/// <param name="obj">The other file to compare to.</param>
		/// <returns>The comparison result.</returns>
		int IComparable.CompareTo(object obj) {
			return CompareTo((FileItemBase) obj);
		}

		#endregion

		#region ToString/DebuggerDisplay

		/// <summary>Gets the string representation of this item.</summary>
		public override string ToString() => $"{Type}: {Name}";

		/// <summary>Gets the string used to represent the file in the debugger.</summary>
		private string DebuggerDisplay => Name;

		#endregion
	}
}
