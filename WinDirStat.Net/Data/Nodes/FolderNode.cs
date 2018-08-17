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

namespace WinDirStat.Net.Data.Nodes {
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

		protected FolderNode(IFileFindData data, FileNodeType type, FileNodeFlags rootType)
			: base(data, type, rootType)
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

		public FolderNode(IFileFindData find)
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
		public bool IsStoringFiles {
			get => virtualChildren.Count > 0 && virtualChildren[0].Type == FileNodeType.File;
		}
		public bool IsEmpty {
			get => virtualChildren == EmptyVirtualChildren;
		}

		internal void AddChild(RootNode root, FileNodeBase node, ref FolderNode fileCollection) {
			bool fileCollectionCreated = false;
			if (node.Type == FileNodeType.File && Type != FileNodeType.FileCollection &&
				virtualChildren.Count > 0 && virtualChildren[0].Type != FileNodeType.File)
			{
				// Setup file collection if the folder is storing non-files.
				if (fileCollection == null) {
					fileCollection = new FolderNode();
					fileCollectionCreated = true;
				}
			}
			else if (IsStoringFiles && node.Type != FileNodeType.File) {
				// Setup file collection if the folder needs to store a non-file.
				if (fileCollection == null) {
					fileCollection = new FolderNode();
					fileCollectionCreated = true;
				}
			}
			FolderNode nonRefFileCollection = fileCollection;

			VisualInvokeIf(IsLoaded, () => VisualAddChild(root, node, nonRefFileCollection, fileCollectionCreated));
		}

		private void VisualAddChild(RootNode root, FileNodeBase node, FolderNode fileCollection, bool fileCollectionCreated) {
			bool newList = false;

			// We know we're adding a node to virtualChildren, make sure it's setup
			if (IsEmpty) {
				virtualChildren = new List<FileNodeBase>(4);
				newList = true;
			}
			
			lock (virtualChildren) {
				// Setup file collection if the folder is storing non-files.
				if (node.Type == FileNodeType.File && Type != FileNodeType.FileCollection &&
					virtualChildren.Count > 0 && virtualChildren[0].Type != FileNodeType.File)
				{
					if (fileCollectionCreated)
						Add(fileCollection);
					fileCollection.VisualAddChild(root, node, null, false);
					//if (newList)
					//	RaisePropertyChanged(nameof(ShowExpander));
					if (fileCollectionCreated && IsPopulated) {
						EnsureVisualChildren();
						fileCollection.LoadIcon(root);
						vi.children.Add(fileCollection);
					}
					return;
				}

				// Setup file collection if the folder needs to store a non-file.
				if (IsStoringFiles && node.Type != FileNodeType.File) {
					lock (fileCollection.virtualChildren) {
						// File collection was just created, so no UI validation required
						fileCollection.AddRange(virtualChildren);
						virtualChildren.Clear();
						if (fileCollectionCreated)
							Add(fileCollection);
						fileCollection.Invalidate();
						if (IsPopulated) {
							// No need to transfer the visual children
							vi.children.Clear();
						}
					}
				}
				Add(node);
				Invalidate();

				if (newList)
					RaisePropertyChanged(nameof(ShowExpander));

				if (IsPopulated) {
					EnsureVisualChildren();
					node.LoadIcon(root);
					if (fileCollectionCreated) {
						fileCollection.LoadIcon(root);
						vi.children.Add(fileCollection);
					}
					vi.children.Add(node);
				}
			}
		}

		protected void Add(FileNodeBase node) {
			//if (IsEmpty)
			//	virtualChildren = new List<FileNodeBase>(4);
			virtualChildren.Add(node);
			node.virtualParent = this;
		}

		/*protected void SetList(List<FileNodeBase> items) {
			virtualChildren = items;
			int count = virtualChildren.Count;
			for (int i = 0; i < count; i++)
				virtualChildren[i].virtualParent = this;
		}*/

		protected void AddRange(IEnumerable<FileNodeBase> items) {
			//if (IsEmpty)
			//	virtualChildren = new List<FileNodeBase>();
			virtualChildren.AddRange(items);
			int count = virtualChildren.Count;
			for (int i = 0; i < count; i++)
				virtualChildren[i].virtualParent = this;
		}

		/*protected List<FileNodeBase> Clear() {
			int count = virtualChildren.Count;
			if (count > 0) {
				for (int i = 0; i < count; i++)
					virtualChildren[i].virtualParent = null;
				List<FileNodeBase> oldChildren = virtualChildren;
				virtualChildren = EmptyVirtualChildren;
				return oldChildren;
			}
			return EmptyVirtualChildren;
		}*/

		private void Invalidate() {
			if (!IsInvalidated) {
				IsInvalidated = true;
				virtualParent?.Invalidate();
			}
		}

		internal bool Validate(bool force = false) {
			return Validate(Root, force);
		}

		internal bool Validate(RootNode root, bool force) {
			if (!IsInvalidated || (IsValidating && !force))
				return false;
			while (IsValidating)
				Thread.Sleep(1);

			IsInvalidated = false;

			// Nothing to validate
			if (IsEmpty)
				return true;

			IsValidating = true;

			bool percentagesChanged = false;
			long oldSize = size;

			//WinDirNode[] children;
			bool addedChildren = false;
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
					if (child is FolderNode childDir)
						percentagesChanged |= childDir.Validate(root, force);
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
				percentagesChanged |= (oldSize != size) || addedChildren;
				/*if (percentagesChanged) {
					foreach (FileNode child in VirtualChildren) {
						child.RaisePropertyChanged(nameof(Percent));
					}
				}*/

				if ((virtualParent?.IsPopulated ?? true) && IsVisible) {
					RaisePropertiesChanged(nameof(Size),
						//nameof(LastAccessTime),
						nameof(LastChangeTime),
						nameof(ItemCount),
						nameof(FileCount),
						nameof(SubdirCount),
						nameof(Percent));
				}

				if (percentagesChanged) {
					count = virtualChildren.Count;
					for (int i = 0; i < count; i++) {
						virtualChildren[i].RaisePropertyChanged(nameof(Percent));
					}
					if (IsExpanded)
						Sort(root, false);
				}
			}

			IsValidating = false;
			return percentagesChanged;
		}
		
		protected internal void SortInsert(RootNode root, FileNodeBase node) {
			int index;
			for (index = Children.Count; index > 0; index--) {
				if (!root.Document.FileComparer.ShouldSort(node, Children[index - 1]))
					break;
			}
			vi.children.Insert(index, node);
		}
		
		internal void FinalValidate(RootNode root) {
			// Nothing to validate
			if (IsEmpty)
				return;

			Validate(root, true);
			lock (virtualChildren) {
				if (!IsStoringFiles) {
					int count = virtualChildren.Count;
					for (int i = 0; i < count; i++) {
						FileNodeBase child = virtualChildren[i];
						if (child is FolderNode folder)
							folder.FinalValidate(root);
					}
				}
				virtualChildren.Minimize();
				virtualChildren.Sort(root.Document.SizeFileComparer.Compare);
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

		protected void LoadChildren(RootNode root) {

			lock (virtualChildren) {
				if (!IsPopulated) {
					IsPopulated = true;
					// Nothing to load
					if (IsEmpty)
						return;

					EnsureVisualChildren();

					int count = virtualChildren.Count;
					for (int i = 0; i < count; i++) {
						FileNodeBase child = virtualChildren[i];
						if (child.Icon == null && IsExpanded)
							child.LoadIcon(root, virtualChildren.Count > 5000);
						if (child.IsExpanded && child is FolderNode folder)
							folder.LoadChildren(root);
					}
					vi.children.AddRange(virtualChildren.OrderBy(n => n, root.Document.FileComparer));
				}
			}
		}

		protected void UnloadChildren(RootNode root) {
			lock (virtualChildren) {
				if (IsPopulated) {
					IsPopulated = false;
					// Nothing to unload
					if (IsEmpty)
						return;
					
					vi.children.Clear();
					if (!IsStoringFiles) {
						int count = virtualChildren.Count;
						for (int i = 0; i < count; i++) {
							FileNodeBase child = virtualChildren[i];
							if (child.visualInfo != null) {
								if (child is FolderNode folder) {
									if (folder.IsPopulated)
										folder.UnloadChildren(root);
								}
								child.UnloadVisuals();
							}
						}
					}
				}
				IsPopulated = false;
			}
		}

		protected override sealed void OnExpanding() {
			RootNode root = Root;
			lock (virtualChildren) {
				if (!IsPopulated)
					LoadChildren(root);
				else
					Sort(root, false);
			}
		}

		protected override sealed void OnCollapsing() {
			UnloadChildren(Root);
		}
		protected void EnsureVisualChildren() {
			if (vi.children == null)
				vi.children = new FileNodeCollection(this);
		}

		internal void Sort(RootNode root, bool sortSubChildren) {
			// Nothing to sort
			if (IsEmpty)
				return;

			lock (virtualChildren) {
				if (!IsPopulated || visualInfo?.children == null || !visualInfo.children.Any())
					return;
				
				visualInfo.children.Sort(root.Document.FileComparer.Compare);
				if (sortSubChildren) {
					foreach (FileNodeBase child in visualInfo.children) {
						if (child.IsExpanded && child is FolderNode subfolder)
							subfolder.Sort(root, true);
					}
				}
			}
		}
	}
}
