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

		private const string ComputerName = "Computer";

		private readonly string rootPath;

		private readonly WinDirDocument document;
		private FreeSpaceNode freeSpace;
		private UnknownNode unknown;
		internal string displayName = null;

		public event EventHandler Invalidated;
		
		public string DisplayName {
			get => displayName ?? $"({PathUtils.TrimSeparatorEnd(Name)})";
		}

		public RootNode(WinDirDocument document)
			: base(ComputerName, FileNodeType.Computer, FileNodeFlags.AbsoluteRootType)
		{
			displayName = "My Computer";
			this.document = document;
		}

		public RootNode(WinDirDocument document, INtfsNode node, bool isAbsoluteRoot)
			: base(node, GetFileType(node.FullName), GetRootType(isAbsoluteRoot)) {
			this.document = document;
			rootPath = System.IO.Path.GetFullPath(node.FullName);
			if (Type == FileNodeType.Volume) {
				if (!rootPath.EndsWith(@"\") && !rootPath.EndsWith("/"))
					rootPath += '\\';
			}
			SetupDrive();
		}

		public RootNode(WinDirDocument document, FileSystemInfo info, bool isAbsoluteRoot)
			: base(info, GetFileType(info.FullName), GetRootType(isAbsoluteRoot)) {
			this.document = document;
			rootPath = System.IO.Path.GetFullPath(info.FullName);
			if (Type == FileNodeType.Volume) {
				if (!rootPath.EndsWith(@"\") && !rootPath.EndsWith("/"))
					rootPath += '\\';
			}
			SetupDrive();
		}

		public RootNode(WinDirDocument document, IFileFindData find, bool isAbsoluteRoot)
			: base(find, GetFileType(find.FullName), GetRootType(isAbsoluteRoot)) {
			this.document = document;
			rootPath = System.IO.Path.GetFullPath(find.FullName);
			if (Type == FileNodeType.Volume) {
				if (!rootPath.EndsWith(@"\") && !rootPath.EndsWith("/"))
					rootPath += '\\';
			}
			SetupDrive();
		}

		// Duplicate tree for faster iteration
		public RootNode(RootNode node) : base(node, null) {
			rootPath = node.rootPath;
			document = node.document;
			freeSpace = node.freeSpace;
		}

		private void SetupDrive() {
			if (Type == FileNodeType.Volume) {
				freeSpace = new FreeSpaceNode(rootPath);
				freeSpace.virtualParent = this;
				freeSpace.LoadIcon(this);
				unknown = new UnknownNode(rootPath, 0);
				unknown.virtualParent = this;
				unknown.LoadIcon(this);
				FolderNode fileCollection = null;
				if (document.Settings.ShowFreeSpace)
					AddChild(this, freeSpace, ref fileCollection);
				if (document.Settings.ShowUnknown)
					AddChild(this, unknown, ref fileCollection);
			}
		}

		private static FileNodeType GetFileType(string path) {
			return (PathUtils.IsPathRoot(path) ? FileNodeType.Volume : FileNodeType.Directory);
		}

		private static FileNodeFlags GetRootType(bool isAbsoluteRoot) {
			return (isAbsoluteRoot ? FileNodeFlags.AbsoluteRootType : FileNodeFlags.None) |
				FileNodeFlags.FileRootType;
		}

		internal void AddFreeSpace(FreeSpaceNode freeSpace, string path, bool async) {
			Debug.Assert(this.freeSpace == null);
			this.freeSpace = freeSpace;
			if (document.Settings.ShowFreeSpace) {
				FolderNode fileCollection = null;
				AddChild(this, freeSpace, ref fileCollection);
			}
		}

		public FreeSpaceNode FreeSpace {
			get => freeSpace;
			//internal set => freeSpace = value;
		}
		public UnknownNode Unknown {
			get => unknown;
		}

		public WinDirDocument Document {
			get => document;
		}

		public string RootPath {
			get => rootPath;
		}

		internal void OnShowFreeSpaceChanged(bool showFreeSpace) {
			// This must only be called when the value has actually switched
			if (Type == FileNodeType.Computer) {
				int count = virtualChildren.Count;
				for (int i = 0; i < count; i++) {
					((RootNode) virtualChildren[i]).OnShowFreeSpaceChanged(showFreeSpace);
				}
				return;
			}
			else if (Type == FileNodeType.Volume) {
				lock (virtualChildren) {
					int index = virtualChildren.IndexOf(freeSpace);
					if (index != -1 && !showFreeSpace) {
						virtualChildren.RemoveAt(index);
						size -= freeSpace.Size;
					}
					else if (index == -1 && showFreeSpace) {
						Add(freeSpace);
						virtualChildren.Sort(document.SizeFileComparer.Compare);
						size += freeSpace.Size;
					}
					if (IsPopulated) {
						index = vi.children.IndexOf(freeSpace);
						if (index != -1 && !showFreeSpace) {
							vi.children.RemoveAt(index);
						}
						else if (index == -1 && showFreeSpace) {
							vi.children.Add(freeSpace);
							Sort(Root, false);
						}
					}
					int count = virtualChildren.Count;
					for (int i = 0; i < count; i++) {
						virtualChildren[i].RaisePropertyChanged(nameof(Percent));
					}
					RaisePropertiesChanged(nameof(Size), nameof(Percent));
				}
			}
		}
		internal long GetUsedSize() {
			long usedSize = 0;
			int count = virtualChildren.Count;
			for (int i = 0; i < count; i++) {
				FileNodeBase child = virtualChildren[i];
				if (child.Type != FileNodeType.FreeSpace && child.Type != FileNodeType.Unknown)
					usedSize += child.Size;
			}
			return usedSize;
		}

		internal void OnShowUnknownChanged(bool showUnknown) {
			if (Type == FileNodeType.Computer) {
				int count = virtualChildren.Count;
				for (int i = 0; i < count; i++) {
					((RootNode) virtualChildren[i]).OnShowUnknownChanged(showUnknown);
				}
				return;
			}
			else if (Type == FileNodeType.Volume) {
				lock (virtualChildren) {
					int index = virtualChildren.IndexOf(unknown);
					if (index != -1 && !showUnknown) {
						virtualChildren.RemoveAt(index);
						size -= unknown.Size;
					}
					else if (index == -1 && showUnknown) {
						Add(unknown);
						unknown.UpdateSize(GetUsedSize());
						size += unknown.Size;
						virtualChildren.Sort(document.SizeFileComparer.Compare);
					}
					if (IsPopulated) {
						index = vi.children.IndexOf(unknown);
						if (index != -1 && !showUnknown) {
							vi.children.RemoveAt(index);
						}
						else if (index == -1 && showUnknown) {
							vi.children.Add(unknown);
							Sort(Root, false);
						}
					}
					int count = virtualChildren.Count;
					for (int i = 0; i < count; i++) {
						virtualChildren[i].RaisePropertyChanged(nameof(Percent));
					}
					RaisePropertiesChanged(nameof(Size), nameof(Percent));
				}
			}
		}
	}
}
