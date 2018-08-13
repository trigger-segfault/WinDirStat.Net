using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Filesystem.Ntfs;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Data;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Data.Nodes {
	public class RootNode : FolderNode {

		private readonly string rootPath;

		internal readonly WinDirDocument document;
		private FreeSpaceNode freeSpace;

		public RootNode(WinDirDocument document, INtfsNode node)
			: base(node, FileNodeType.Root)
		{
			this.document = document;
			rootPath = System.IO.Path.GetFullPath(node.FullName);
			if (PathUtils.IsPathRoot(rootPath)) {
				if (!rootPath.EndsWith(@"\") && !rootPath.EndsWith("/"))
					rootPath += @"\";
				name = rootPath;
			}
		}
		
		public RootNode(WinDirDocument document, FileSystemInfo info)
			: base(info, FileNodeType.Root)
		{
			this.document = document;
			rootPath = System.IO.Path.GetFullPath(info.FullName);
			if (PathUtils.IsPathRoot(rootPath)) {
				if (!rootPath.EndsWith(@"\") && !rootPath.EndsWith("/"))
					rootPath += @"\";
				name = rootPath;
			}
		}

		public RootNode(RootNode node) : base(node, null) {
			rootPath = node.rootPath;
			document = node.document;
			freeSpace = node.freeSpace;
			//populated = node.populated;
			invalidated = node.invalidated;
			done = node.done;
			validating = node.validating;
			if (node.populated) {
				vi.isExpanded = node.IsExpanded;
				LoadChildren(Root);
			}
		}

		internal void AddFreeSpace(FreeSpaceNode freeSpace, string path, bool async) {
			Debug.Assert(this.freeSpace == null);
			this.freeSpace = freeSpace;
			AddChild(this, freeSpace, path, async);
		}

		public FreeSpaceNode FreeSpace {
			get => freeSpace;
			//internal set => freeSpace = value;
		}

		internal int FreeSpaceIndex {
			get {
				if (freeSpace != null)
					return virtualChildren.IndexOf(freeSpace);
				return -1;
			}
		}

		public string RootPath {
			get => rootPath;
		}

		internal void UpdateShowFreeSpace(bool showFreeSpace) {
			// This must only be called when the value has actually switched
			Debug.Assert(Type == FileNodeType.Root);
			if (freeSpace == null) {
				foreach (FileNode node in VirtualChildren) {
					if (node is RootNode subroot)
						UpdateShowFreeSpace(showFreeSpace);
				}
				return;
			}

			lock (virtualChildren) {
				int index = virtualChildren.IndexOf(freeSpace);
				if (index != -1 && !showFreeSpace) {
					virtualChildren.RemoveAt(index);
					size -= freeSpace.Size;
				}
				else if (index == -1 && showFreeSpace) {
					virtualChildren.Add(freeSpace);
					virtualChildren.Sort(document.SizeFileComparer.Compare);
					size += freeSpace.Size;
				}
			}
			VisualInvoke(() => {
				if (populated) {
					int index = vi.children.IndexOf(freeSpace);
					if (index != -1 && !showFreeSpace) {
						vi.children.RemoveAt(index);
					}
					else if (index == -1 && showFreeSpace) {
						vi.children.Add(freeSpace);
						Sort(Root, false);
					}
				}
				lock (virtualChildren) {
					foreach (FileNode node in VirtualChildren) {
						node.RaisePropertyChanged(nameof(Percent));
					}
					RaisePropertiesChanged(nameof(Size), nameof(Percent));
				}
			});
		}

		/*public override WinDirDocument Document {
			get => document;
		}*/
	}
}
