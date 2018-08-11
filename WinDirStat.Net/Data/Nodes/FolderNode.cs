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
	public class FolderNode : FileNode {

		private static readonly char[] FileCollectionName = "<Files>".ToArray();

#if COMPRESSED_DATA
		private const byte InvalidatedFlag = 0x01;
		private const byte ValidatingFlag = 0x02;
		private const byte PopulatedFlag = 0x04;
		private const byte DoneFlag = 0x08;

		private volatile byte flags = InvalidatedFlag;

		/// <summary>Gets one of the <see cref="heightAndFlags"/> flags.</summary>
		private bool GetFlag(byte flag) {
			return (flags & flag) != 0;
		}

		/// <summary>Sets one of the <see cref="heightAndFlags"/> flags.</summary>
		private void SetFlag(byte flag, bool enabled) {
			if (enabled)
				flags |= flag;
			else
				flags &= unchecked((byte) ~flag);
		}
#endif

		private readonly FileNodeType type;
		//viprivate FileNodeCollection visualChildren;
		private int fileCount;
		private int subdirCount;

#if !COMPRESSED_DATA
		private bool populated;
		private volatile bool invalidated = true;
		private volatile bool validating;
		private volatile bool done;
#else
		private bool invalidated {
			get => GetFlag(InvalidatedFlag);
			set => SetFlag(InvalidatedFlag, value);
		}
		private bool validating {
			get => GetFlag(ValidatingFlag);
			set => SetFlag(ValidatingFlag, value);
		}
		private bool populated {
			get => GetFlag(PopulatedFlag);
			set => SetFlag(PopulatedFlag, value);
		}
		private bool done {
			get => GetFlag(DoneFlag);
			set => SetFlag(DoneFlag, value);
		}

#endif




		//private volatile ushort typeAndFlags = 0x0100;

		/*private FileNodeType type {
			get => (FileNodeType) (typeAndFlags & 0xFF);
			set => typeAndFlags = unchecked((ushort) ((typeAndFlags & 0xFF00) | (int) value));
		}


		private bool invalidated {

		}*/

		/// <summary>
		/// 
		/// </summary>
		/// 
		/// <remarks>
		/// Unlike <see cref="Children"/>, this is always filled with all known children.
		/// <para/>
		/// This list is also sorted by size during the final validation.
		/// </remarks>
		protected CompactList<FileNode> virtualChildren = new CompactList<FileNode>(0);
		/// <summary>
		/// This is only non-null during the loading process. Afterwords this list is no longer used.
		/// </summary>
		//private List<FileNode> childrenToLoad;
		private FolderNode fileCollection;

		//private readonly object childrenLock = new object();

		private FolderNode() {
			type = FileNodeType.FileCollection;
			cName = FileCollectionName;
		}

		protected FolderNode(INtfsNode node, FileNodeType type) : base(node) {
			this.type = type;
		}

		protected FolderNode(FileSystemInfo info, FileNodeType type) : base(info) {
			this.type = type;
		}

		protected FolderNode(IFileFindData data, FileNodeType type) : base(data) {
			this.type = type;
		}

		public FolderNode(INtfsNode node) : this(node, FileNodeType.Directory) {
		}

		public FolderNode(FileSystemInfo info) : this(info, FileNodeType.Directory) {
		}

		public FolderNode(IFileFindData data) : this(data, FileNodeType.Directory) {
		}
		
		internal void AddChild(RootNode root, FileNode node, string path, bool asyncLoad) {
			if (node.Type == FileNodeType.File && type != FileNodeType.FileCollection) {
				if (fileCollection == null) {
					fileCollection = new FolderNode();
					AddChild(root, fileCollection, null, asyncLoad);
				}
				fileCollection.AddChild(root, node, null, asyncLoad);
			}
			else {
				lock (virtualChildren) {
					if (virtualChildren == null) {
						virtualChildren = new CompactList<FileNode>();
					}
					Debug.Assert(node.virtualParent == null);
					node.virtualParent = this;
					virtualChildren.Add(node);
					if (populated)
						EnsureVisualChildren();
					Invalidate();
				}
				if (populated) {
					node.LoadIcon(root);
					VisualInvoke(() => {
						if (virtualChildren.Count == 1)
							RaisePropertyChanged(nameof(ShowExpander));
						lock (virtualChildren)
							//SortInsert(root, node);
							vi.children.Add(node);
						if (node.Type == FileNodeType.FreeSpace)
							node.IsHidden = !root.Document.Settings.ShowFreeSpace;
					});
				}
				else if (virtualChildren.Count == 1) {
					VisualPropertyChanged(nameof(ShowExpander));
				}
			}
		}

		private void EnsureVisualChildren() {
			if (vi.children == null)
				vi.children = new FileNodeCollection(this);
		}

		private void Invalidate() {
			if (!invalidated) {
				invalidated = true;
				VirtualParent?.Invalidate();
			}
		}

		internal bool Validate(bool force = false) {
			return Validate(Root, force);
		}

		internal bool Validate(RootNode root, bool force) {
			if (!invalidated || (validating && !force))
				return false;
			while (validating)
				Thread.Sleep(1);
			validating = true;
			invalidated = false;

			bool percentagesChanged = false;
			long oldSize = size;

			//WinDirNode[] children;
			bool addedChildren = false;
			lock (virtualChildren) {
				fileCount = 0;
				subdirCount = 0;
				size = 0;
				foreach (FileNode child in VirtualChildren) {
					if (child.Type == FileNodeType.FreeSpace) {
						if (root.Document.Settings.ShowFreeSpace)
							size += child.Size;
						continue;
					}
					if (child is FolderNode childDir)
						percentagesChanged |= childDir.Validate(root, force);
					size += child.Size;
					//lastAccessTime = Max(lastAccessTime, child.LastAccessTime);
					lastChangeTime = Max(lastChangeTime, child.LastChangeTime);
					if (child.Type == FileNodeType.File) {
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
				percentagesChanged |= (oldSize != size) || addedChildren;
				/*if (percentagesChanged) {
					foreach (FileNode child in VirtualChildren) {
						child.RaisePropertyChanged(nameof(Percent));
					}
				}*/
			}

			if ((IsVirtualRoot || VirtualParent.populated) && IsVisible) {
				VisualPropertiesChanged(nameof(Size),
					//nameof(LastAccessTime),
					nameof(LastChangeTime),
					nameof(ItemCount),
					nameof(FileCount),
					nameof(SubdirCount),
					nameof(Percent));
			}

			if (percentagesChanged) {
				VisualInvoke(() => {

					lock (virtualChildren) {
						foreach (FileNode child in VirtualChildren) {
							child.RaisePropertyChanged(nameof(Percent));
						}
					}
					if (IsExpanded)
						Sort(root, false);
				});
			}

			validating = false;
			return percentagesChanged;
		}

		internal void UpdateShowFreeSpace(bool showFreeSpace) {
			// This must only be called when the value has actually switched
			Debug.Assert(Type == FileNodeType.Root);
			RootNode root = (RootNode) this;
			if (root.FreeSpace == null) {
				foreach (FileNode node in VirtualChildren) {
					if (node is RootNode subroot)
						UpdateShowFreeSpace(showFreeSpace);
				}
				return;
			}

			lock (virtualChildren) {
				foreach (FileNode node in VirtualChildren) {
					if (node.Type == FileNodeType.FreeSpace) {
						node.IsHidden = !showFreeSpace;
						if (showFreeSpace)
							size += node.Size;
						else
							size -= node.Size;
						break;
					}
				}
			}
			VisualInvoke(() => {
				lock (virtualChildren) {
					/*if (IsLoaded) {
						if (!showFreeSpace)
							vi.children.Remove(root.FreeSpace);
						else
							SortInsert(root, root.FreeSpace);
					}*/
					foreach (FileNode node in VirtualChildren) {
						node.RaisePropertyChanged(nameof(Percent));
					}
					RaisePropertiesChanged(nameof(Size), nameof(Percent));
				}
			});
		}

		private void LoadChildren(RootNode root) {
			lock (virtualChildren) {
				if (!populated && virtualChildren != null) {
					EnsureVisualChildren();
					
					foreach (FileNode child in virtualChildren) {
						if (child.Icon == null && IsExpanded)
							child.LoadIcon(root);
						if (child.IsExpanded)
							((FolderNode) child).LoadChildren(root);
					}
					vi.children.AddRange(virtualChildren.OrderBy(n => n, root.Document.FileComparer));
					foreach (FileNode child in virtualChildren) {
						if (child.Type == FileNodeType.FreeSpace)
							child.IsHidden = !root.Document.Settings.ShowFreeSpace;
					}
				}
				populated = true;
			}
		}

		private void UnloadChildren(RootNode root) {
			lock (virtualChildren) {
				if (populated && virtualChildren != null) {
					populated = false;

					//Children.Clear();
					foreach (FolderNode child in VirtualFolders) {
						if (child.populated)
							child.UnloadChildren(root);
					}
				}
				populated = false;
				//UnloadVisuals(false);
			}
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

		private void SortInsert(RootNode root, FileNode node) {
			int index;
			for (index = Children.Count; index > 0; index--) {
				if (!root.Document.FileComparer.ShouldSort(node, Children[index - 1]))
					break;
			}
			vi.children.Insert(index, node);
		}

		/*internal void Sort(RootNode root) {
			modelChildren.Sort(root.Document.FileComparer);
		}*/
		
		internal void FinalValidate(RootNode root) {
			lock (virtualChildren) {
				if (IsExpanded)
					LoadChildren(root);
				Validate(root, true);
				foreach (FolderNode subfolder in VirtualFolders) {
					subfolder.FinalValidate(root);
				}

				// Sort after the sizes have been fully determined
				virtualChildren.Minimize();
				virtualChildren?.Sort(root.Document.SizeFileComparer);
			};
		}
		
		protected override void OnExpanding() {
			RootNode root = Root;
			if (!populated)
				LoadChildren(root);
			else
				Sort(root, false);
			foreach (FileNode child in VirtualChildren)
				child.LoadIcon(root);
		}

		protected override void OnCollapsing() {
			UnloadChildren(Root);
			if (vi.children != null) {
				FileNode[] removedChildren = visualInfo.children.ToArray();
				visualInfo.children.Clear();
				foreach (FileNode child in removedChildren) {
					child.UnloadVisuals();
				}
			}
		}

		private static DateTime Max(DateTime a, DateTime b) {
			return (a > b ? a : b);
		}
		
		public override IReadOnlyList<FileNode> VirtualChildren {
			get => virtualChildren ?? EmptyList;
		}

		public IEnumerable<FolderNode> VirtualFolders {
			get {
				if (type == FileNodeType.FileCollection)
					yield break;
				foreach (FileNode node in VirtualChildren) {
					if (node is FolderNode folder)
						yield return folder;
				}
			}
		}

		public IEnumerable<FileNode> VirtualFiles {
			get {
				if (type != FileNodeType.FileCollection)
					yield break;
				foreach (FileNode node in VirtualChildren) {
					if (node.Type == FileNodeType.File)
						yield return node;
				}
			}
		}

		public override bool HasChildren {
			get => (virtualChildren?.Count ?? 0) != 0;
		}

		internal bool HasDirectories {
			get => type != FileNodeType.FileCollection && (virtualChildren?.Count ?? 0) != 0;
		}

		internal bool HasFileCollection {
			get => fileCollection != null;
		}

		public bool IsDone {
			get => done;
			set => done = value;
		}

		public bool Invalidated {
			get => invalidated;
		}

		public bool Validating {
			get => validating;
		}

		public override FileNodeType Type {
			get => type;
		}

		public override int FileCount {
			get => fileCount;
		}

		public override int SubdirCount {
			get => subdirCount;
		}
	}
}
