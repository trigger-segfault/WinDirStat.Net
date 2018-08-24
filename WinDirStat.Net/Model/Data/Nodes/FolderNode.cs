using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Filesystem.Ntfs;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;
using WinDirStat.Net.Utils;
using System.Threading;
using WinDirStat.Net.Utils.Native;

namespace WinDirStat.Net.Model.Data.Nodes {
	public partial class FolderNode : FileNodeBase {
		//private static readonly char[] FileCollectionName = "<Files>".ToArray();
		private const string FileCollectionName = "<Files>";
		
		protected volatile List<FileNodeBase> virtualChildren = EmptyVirtualChildren;
		internal int fileCount;
		internal int subdirCount;

		private FolderNode()
			: base(FileCollectionName, FileNodeType.FileCollection, FileNodeFlags.None)
		{
			// File Collections are only ever created when a file
			// needs to be added. Let's setup the list now.
			virtualChildren = new List<FileNodeBase>(4);
		}
		
		protected FolderNode(string name, FileNodeType type, FileNodeFlags rootType)
			: base(name, type, rootType)
		{
		}

		protected FolderNode(INtfsNode node, FileNodeType type, FileNodeFlags rootType)
			: base(node, type, rootType)
		{
		}

		protected FolderNode(FileSystemInfo info, FileNodeType type, FileNodeFlags rootType)
			: base(info, type, rootType)
		{
		}

		protected FolderNode(Win32.Win32FindData find, FileNodeType type, FileNodeFlags rootType)
			: base(find, type, rootType)
		{
		}

		public FolderNode(FolderNode node, FolderNode parent) : base(node, parent) {
			fileCount = node.fileCount;
			subdirCount = node.subdirCount;

			List<FileNodeBase> oldVirtualChildren = node.virtualChildren;
			int count = oldVirtualChildren.Count;
			if (count > 0) {
				virtualChildren = new List<FileNodeBase>(oldVirtualChildren.Count);
				for (int i = 0; i < count; i++) {
					FileNodeBase child = oldVirtualChildren[i];
					FileNodeBase newChild = null;
					if (child is FolderNode folder)
						newChild = new FolderNode(folder, this);
					else if (child is FileNode file)
						newChild = new FileNode(file, this);
					if (newChild != null)
						Add(newChild);
				}
			}
		}

		public FolderNode(INtfsNode node)
			: this(node, FileNodeType.Directory, FileNodeFlags.None)
		{
		}

		public FolderNode(FileSystemInfo info)
			: this(info, FileNodeType.Directory, FileNodeFlags.None)
		{
		}

		public FolderNode(Win32.Win32FindData find)
			: this(find, FileNodeType.Directory, FileNodeFlags.None)
		{
		}

		internal override sealed List<FileNodeBase> VirtualChildren {
			get => virtualChildren;
		}
		public override sealed int ItemCount {
			get => fileCount + subdirCount;
		}
		public override sealed int FileCount {
			get => fileCount;
		}
		public override sealed int SubdirCount {
			get => subdirCount;
		}
		private bool IsStoringFiles {
			get => virtualChildren.Count > 0 && virtualChildren[0].Type == FileNodeType.File;
		}
		private bool IsStoringMultipleFiles {
			get => virtualChildren.Count > 1 && ((virtualChildren[0].Type == FileNodeType.File) &&
				(virtualChildren[1].Type == FileNodeType.File));
		}
		private bool IsStoringNonFiles {
			get => (virtualChildren.Count > 0 && virtualChildren[0].Type != FileNodeType.File) ||
				(virtualChildren.Count > 1 && virtualChildren[1].Type != FileNodeType.File);
		}
		public bool IsEmpty {
			get => virtualChildren == EmptyVirtualChildren;
		}

		internal void AddChild(FileNodeBase node, ref FolderNode fileCollection, ref FileNodeBase file) {
			// We know we're adding a node to virtualChildren, make sure it's setup
			if (IsEmpty)
				virtualChildren = new List<FileNodeBase>(4);

			lock (virtualChildren) {
				// file is only non-null when one file is being stored (with or without other files)
				if (node.Type == FileNodeType.File) {
					if (fileCollection != null) {
						fileCollection.Add(node);
						fileCollection.Invalidate();
						return;
					}
					else {
						bool storingNonFiles = IsStoringNonFiles;
						if (virtualChildren.Count == 0 || (file == null && storingNonFiles)) {
							// Our first file! Let's celebrate by keeping track of it.
							// If more files are added then it will no longer be tracked.
							file = node;
						}
						else if (file != null) {
							if (storingNonFiles) {
								// We've hit our limit of only one visible file when a
								// folder is storing non-files. Move to FileCollection.
								fileCollection = new FolderNode();
								Remove(file);
								Add(fileCollection);
								fileCollection.Add(file);
								fileCollection.Add(node);
								fileCollection.Invalidate();
								return;
							}
							file = null;
						}
					}
				}
				else if (IsStoringMultipleFiles) {
					// Setup file collection if the folder needs to store a non-file.
					Debug.Assert(fileCollection == null);
					fileCollection = new FolderNode();
					List<FileNodeBase> files = ClearAndGetRange();
					Add(fileCollection);
					fileCollection.AddRange(files);
					fileCollection.Invalidate();
					file = null;
				}

				Add(node);

				Invalidate();
			}
		}

		protected void Add(FileNodeBase node) {
			if (IsEmpty)
				virtualChildren = new List<FileNodeBase>(4);
			int index = virtualChildren.Count;
			virtualChildren.Add(node);
			node.virtualParent = this;
			if (IsWatched)
				RaiseChanged(FileNodeAction.ChildrenAdded, node, index);
		}


		protected void AddRange(params FileNodeBase[] nodes) {
			AddRange((IEnumerable<FileNodeBase>) nodes);
		}

		protected void AddRange(IEnumerable<FileNodeBase> nodes) {
			if (IsEmpty)
				virtualChildren = new List<FileNodeBase>();
			int index = virtualChildren.Count;
			virtualChildren.AddRange(nodes);
			foreach (FileNodeBase node in nodes)
				node.virtualParent = this;
			if (IsWatched)
				RaiseChanged(FileNodeAction.ChildrenAdded, nodes.ToList(), index);
		}

		protected void RemoveRange(params FileNodeBase[] nodes) {
			RemoveRange((IEnumerable<FileNodeBase>) nodes);
		}

		protected void RemoveRange(IEnumerable<FileNodeBase> nodes) {
			if (IsEmpty)
				virtualChildren = new List<FileNodeBase>();
			foreach (FileNodeBase node in nodes) {
				virtualChildren.Remove(node);
				node.virtualParent = null;
			}
			if (IsWatched)
				RaiseChanged(FileNodeAction.ChildrenRemoved, nodes.ToList(), -1);
		}

		protected bool Remove(FileNodeBase node) {
			if (IsEmpty)
				virtualChildren = new List<FileNodeBase>();
			if (virtualChildren.Remove(node)) {
				node.virtualParent = null;
				if (IsWatched)
					RaiseChanged(FileNodeAction.ChildrenRemoved, node, -1);
				return true;
			}
			return false;
		}

		protected void RemoveAt(int index) {
			FileNodeBase node = virtualChildren[index];
			virtualChildren.RemoveAt(index);
			node.virtualParent = null;
			if (IsWatched)
				RaiseChanged(FileNodeAction.ChildrenRemoved, node, index);
		}

		protected void RemoveRange(int index, int count) {
			List<FileNodeBase> nodes = virtualChildren.GetRange(index, count);
			virtualChildren.RemoveRange(index, count);
			for (int i = 0; i < count; i++)
				nodes[i].virtualParent = null;
			if (IsWatched)
				RaiseChanged(FileNodeAction.ChildrenRemoved, nodes.ToList(), index);
		}

		private List<FileNodeBase> ClearAndGetRange() {
			int count = virtualChildren.Count;
			if (count > 0) {
				for (int i = 0; i < count; i++)
					virtualChildren[i].virtualParent = null;
				List<FileNodeBase> oldChildren = virtualChildren.GetRange(0, count);
				virtualChildren.Clear();
				if (IsWatched)
					RaiseChanged(FileNodeAction.ChildrenRemoved, oldChildren, 0);
				return oldChildren;
			}
			return EmptyVirtualChildren;
		}

		protected void Clear() {
			int count = virtualChildren.Count;
			if (count > 0) {
				for (int i = 0; i < count; i++)
					virtualChildren[i].virtualParent = null;
				if (IsWatched) {
					List<FileNodeBase> oldChildren = virtualChildren.ToList();
					virtualChildren.Clear();
					virtualChildren = EmptyVirtualChildren;
					RaiseChanged(FileNodeAction.ChildrenRemoved, oldChildren, 0);
				}
				else {
					virtualChildren.Clear();
					virtualChildren = EmptyVirtualChildren;
				}
			}
		}

		protected void Invalidate() {
			if (!IsInvalidated) {
				IsInvalidated = true;
				if (IsWatched)
					RaiseChanged(FileNodeAction.Invalidated);
				virtualParent?.Invalidate();
			}
		}
		
		public bool Validate(bool force) {
			if (!IsInvalidated || (IsValidating && !force))
				return false;
			while (IsValidating)
				Thread.Sleep(1);

			IsInvalidated = false;

			// Nothing to validate
			if (IsEmpty) {
				if (IsWatched)
					RaiseChanged(FileNodeAction.Validated);
				return true;
			}

			IsValidating = true;

			// True, if any of the sort orders that can change, have changed
			bool sortOrderChanged = false;
			long oldSize = size;
			DateTime oldLastChangeTime = lastChangeTime;

			//WinDirNode[] children;
			lock (virtualChildren) {
				UnknownNode unknown = null;
				FileNodeBase freeSpace = null;
				fileCount = 0;
				subdirCount = 0;
				size = 0;
				int count = virtualChildren.Count;
				for (int i = 0; i < count; i++) {
					FileNodeBase child = virtualChildren[i];
					if (child.Type == FileNodeType.Unknown) {
						// Don't count the size for this because it will 
						// be updated based on the remaining size.
						unknown = (UnknownNode) child;
						continue;
					}
					else if (child.Type == FileNodeType.FreeSpace) {
						// Don't count the size for this because it will 
						// be excluded when updating unknown's size.
						freeSpace = child;
						continue;
					}
					if (child is FolderNode childFolder)
						sortOrderChanged |= childFolder.Validate(force);
					size += child.Size;
					//lastAccessTime = Max(lastAccessTime, child.LastAccessTime);
					lastChangeTime = MaxDateTime(lastChangeTime, child.LastChangeTime);
					if (child.Type == FileNodeType.File || child.Type == FileNodeType.FreeSpace) {
						fileCount++;
					}
					else {
						fileCount += child.FileCount;
						subdirCount += child.SubdirCount;
						if (child.Type != FileNodeType.FileCollection) {
							subdirCount++;
						}
					}
				}
				if (unknown != null) {
					unknown.UpdateSize(size);
					size += unknown.Size;
				}
				if (freeSpace != null) {
					size += freeSpace.Size;
				}
				sortOrderChanged |= (oldSize != size);// || (oldLastChangeTime != lastChangeTime);
				/*if (percentagesChanged) {
					foreach (FileNode child in VirtualChildren) {
						child.RaisePropertyChanged(nameof(Percent));
					}
				}*/
				
			}

			IsValidating = false;
			if (IsWatched) {
				if (sortOrderChanged)
					RaiseChanged(FileNodeAction.ValidatedSortOrder);
				else
					RaiseChanged(FileNodeAction.Validated);
			}
			return sortOrderChanged || (oldLastChangeTime != lastChangeTime);
		}

		internal void FinalValidate() {
			Validate(true);
			FinalValidateInternal();
		}
		private void FinalValidateInternal() {
			// Nothing to validate
			if (IsEmpty)
				return;

			lock (virtualChildren) {
				int count = virtualChildren.Count;
				for (int i = 0; i < count; i++) {
					FileNodeBase child = virtualChildren[i];
					if (child is FolderNode folder)
						folder.FinalValidateInternal();
				}
				virtualChildren.Sort();
				virtualChildren.Minimize();
			}
		}
		private static DateTime MaxDateTime(DateTime a, DateTime b) {
			return (a > b ? a : b);
		}
		
		public FolderNode GetFileCollection() {
			int count = virtualChildren.Count;
			for (int i = 0; i < count; i++) {
				FileNodeBase child = virtualChildren[i];
				if (child.Type == FileNodeType.FileCollection)
					return (FolderNode) child;
			}
			return null;
		}

		public FileNode GetFirstFile() {
			int count = virtualChildren.Count;
			for (int i = 0; i < count; i++) {
				FileNodeBase child = virtualChildren[i];
				if (child.Type == FileNodeType.File)
					return (FileNode) child;
			}
			return null;
		}

		protected void Sort() {
			if (IsEmpty)
				return;

			lock (virtualChildren) {
				virtualChildren.Sort();
			}
		}
	}
}
