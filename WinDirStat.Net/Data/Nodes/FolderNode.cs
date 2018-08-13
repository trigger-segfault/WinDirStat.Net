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

		//private static readonly char[] FileCollectionName = "<Files>".ToArray();
		private const string FileCollectionName = "<Files>";

#if COMPRESSED_DATA
		/*private const byte InvalidatedFlag = 0x01;
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
		}*/
#endif

		//private readonly FileNodeType type;
		//viprivate FileNodeCollection visualChildren;
		internal int fileCount;
		internal int subdirCount;

#if !COMPRESSED_DATA
		private bool populated;
		private volatile bool invalidated = true;
		private volatile bool validating;
		private volatile bool done;
#else
		/*private bool invalidated {
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
		}*/
		protected bool invalidated {
			get => bitfield.GetBit(13);
			set => bitfield.SetBit(13, value);
		}
		protected bool validating {
			get => bitfield.GetBit(14);
			set => bitfield.SetBit(14, value);
		}
		protected bool done {
			get => bitfield.GetBit(16);
			set => bitfield.SetBit(16, value);
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
		//protected internal CompactList<FileNode> virtualChildren = new CompactList<FileNode>(0);
		/// <summary>
		/// This is only non-null during the loading process. Afterwords this list is no longer used.
		/// </summary>
		//private List<FileNode> childrenToLoad;
		private FolderNode fileCollection;

		//private readonly object childrenLock = new object();

		private FolderNode() {
			virtualChildren = new List<FileNode>(0);
			Type = FileNodeType.FileCollection;
			//cName = FileCollectionName;
			name = FileCollectionName;
		}

		protected FolderNode(INtfsNode node, FileNodeType type) : base(node) {
			this.Type = type;
			virtualChildren = new List<FileNode>(0);
		}

		protected FolderNode(FileSystemInfo info, FileNodeType type) : base(info) {
			this.Type = type;
			virtualChildren = new List<FileNode>(0);
		}

		protected FolderNode(IFileFindData data, FileNodeType type) : base(data) {
			this.Type = type;
			virtualChildren = new List<FileNode>(0);
		}

		public FolderNode(FolderNode node, FolderNode parent) : base(node, parent) {
			fileCount = node.fileCount;
			subdirCount = node.subdirCount;
			virtualChildren = new List<FileNode>(node.virtualChildren.Count);
			foreach (FileNode child in node.virtualChildren) {
				FileNode newChild;
				if (child is FolderNode folder)
					newChild = new FolderNode(folder, this);
				else
					newChild = new FileNode(child, this);
				virtualChildren.Add(newChild);
			}
			if (Type != FileNodeType.Root) {
				//populated = node.populated;
				invalidated = node.invalidated;
				done = node.done;
				validating = node.validating;
				if (node.populated) {
					vi.isExpanded = node.IsExpanded;
					LoadChildren(Root);
				}
			}
		}

		public FolderNode(INtfsNode node) : this(node, FileNodeType.Directory) {
		}

		public FolderNode(FileSystemInfo info) : this(info, FileNodeType.Directory) {
		}

		public FolderNode(IFileFindData data) : this(data, FileNodeType.Directory) {
		}
		
		internal void AddChild(RootNode root, FileNode node, string path, bool asyncLoad) {
			if (node.Type == FileNodeType.File && Type != FileNodeType.FileCollection) {
				if (fileCollection == null) {
					fileCollection = new FolderNode();
					AddChild(root, fileCollection, null, asyncLoad);
				}
				fileCollection.AddChild(root, node, null, asyncLoad);
			}
			else {
				if (node.Type == FileNodeType.FreeSpace && !root.Document.Settings.ShowFreeSpace)
					return;
				lock (virtualChildren) {
					if (virtualChildren == null) {
						virtualChildren = new List<FileNode>();
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
							//node.IsHidden = !root.Document.Settings.ShowFreeSpace;
					});
				}
				else if (virtualChildren.Count == 1) {
					VisualPropertyChanged(nameof(ShowExpander));
				}
			}
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
					/*if (child.Type == FileNodeType.FreeSpace) {
						if (root.Document.Settings.ShowFreeSpace)
							size += child.Size;
						continue;
					}*/
					if (child is FolderNode childDir)
						percentagesChanged |= childDir.Validate(root, force);
					size += child.Size;
					//lastAccessTime = Max(lastAccessTime, child.LastAccessTime);
					lastChangeTime = Max(lastChangeTime, child.LastChangeTime);
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

		/*internal void UpdateShowFreeSpace(bool showFreeSpace) {
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
					foreach (FileNode node in VirtualChildren) {
						node.RaisePropertyChanged(nameof(Percent));
					}
					RaisePropertiesChanged(nameof(Size), nameof(Percent));
				}
			});
		}*/



		

		protected internal void SortInsert(RootNode root, FileNode node) {
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

		/*protected override void OnExpanding() {
			RootNode root = Root;
			if (!populated)
				LoadChildren(root);
			else
				Sort(root, false);
			foreach (FileNode child in VirtualChildren)
				child.LoadIcon(root);
		}*/

		//protected override void OnCollapsing() {
		//	UnloadChildren(Root);
		/*if (vi.children != null) {
			FileNode[] removedChildren = visualInfo.children.ToArray();
			visualInfo.children.Clear();
			foreach (FileNode child in removedChildren) {
				child.UnloadVisuals();
			}
		}*/
		//}



		private static DateTime Max(DateTime a, DateTime b) {
			return (a > b ? a : b);
		}
		
		/*public override IReadOnlyList<FileNode> VirtualChildren {
			get => virtualChildren ?? EmptyList;
		}*/

		/*public override bool HasVirtualChildren {
			get => (virtualChildren?.Count ?? 0) != 0;
		}*/

		internal bool HasDirectories {
			get => Type != FileNodeType.FileCollection && (virtualChildren?.Count ?? 0) != 0;
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

		/*public override FileNodeType Type {
			get => type;
		}*/

		/*public override int FileCount {
			get => fileCount;
		}

		public override int SubdirCount {
			get => subdirCount;
		}*/
	}
}
