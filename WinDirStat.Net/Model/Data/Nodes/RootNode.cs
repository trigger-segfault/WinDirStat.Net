using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Filesystem.Ntfs;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Data;
using WinDirStat.Net.Utils;
using WinDirStat.Net.Utils.Native;

namespace WinDirStat.Net.Model.Data.Nodes {
	public class RootNode : FolderNode {

		private const string ComputerName = "Computer";

		private readonly string rootPath;

		private readonly WinDirStatModel model;
		private FreeSpaceNode freeSpace;
		private UnknownNode unknown;
		/*internal string displayName = null;
		
		
		public string DisplayName {
			get => displayName ?? $"({PathUtils.TrimSeparatorEnd(Name)})";
		}*/

		public RootNode(WinDirStatModel model)
			: base(ComputerName, FileNodeType.Computer, FileNodeFlags.AbsoluteRootType)
		{
			//displayName = "My Computer";
			this.model = model;
		}

		public RootNode(WinDirStatModel model, INtfsNode node, bool isAbsoluteRoot)
			: base(node, GetFileType(node.FullName), GetRootType(isAbsoluteRoot))
		{
			this.model = model;
			rootPath = System.IO.Path.GetFullPath(node.FullName);
			if (Type == FileNodeType.Volume) {
				if (!rootPath.EndsWith(@"\") && !rootPath.EndsWith("/"))
					rootPath += '\\';
			}
			SetupDrive();
		}

		public RootNode(WinDirStatModel model, FileSystemInfo info, bool isAbsoluteRoot)
			: base(info, GetFileType(info.FullName), GetRootType(isAbsoluteRoot))
		{
			this.model = model;
			rootPath = System.IO.Path.GetFullPath(info.FullName);
			if (Type == FileNodeType.Volume) {
				if (!rootPath.EndsWith(@"\") && !rootPath.EndsWith("/"))
					rootPath += '\\';
			}
			SetupDrive();
		}

		// Duplicate tree for faster iteration
		public RootNode(RootNode node) : base(node, null) {
			rootPath = node.rootPath;
			model = node.model;
			freeSpace = node.freeSpace;
		}

		private void SetupDrive() {
			if (Type == FileNodeType.Volume) {
				freeSpace = new FreeSpaceNode(rootPath);
				unknown = new UnknownNode(rootPath, 0);
				if (model.ShowFreeSpace)
					Add(freeSpace);
				if (model.ShowUnknown)
					Add(unknown);
			}
		}

		private static FileNodeType GetFileType(string path) {
			return (PathUtils.IsPathRoot(path) ? FileNodeType.Volume : FileNodeType.Directory);
		}

		private static FileNodeFlags GetRootType(bool isAbsoluteRoot) {
			return (isAbsoluteRoot ? FileNodeFlags.AbsoluteRootType : FileNodeFlags.None) |
				FileNodeFlags.FileRootType;
		}

		public FreeSpaceNode FreeSpace {
			get => freeSpace;
		}
		public UnknownNode Unknown {
			get => unknown;
		}

		public WinDirStatModel Model {
			get => model;
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
			}
			else if (Type == FileNodeType.Volume) {
				lock (virtualChildren) {
					int index = virtualChildren.IndexOf(freeSpace);
					if (index != -1 && !showFreeSpace) {
						RemoveAt(index);
						//size -= freeSpace.Size;
					}
					else if (index == -1 && showFreeSpace) {
						Add(freeSpace);
						//size += freeSpace.Size;
						//Sort(false);
					}
				}
				Invalidate();
			}
			if (IsAbsoluteRootType)
				ValidateRoots();
		}

		private void ValidateRoots() {
			if (IsEmpty)
				return;

			Validate(true);
			lock (virtualChildren) {
				if (Type == FileNodeType.Computer) {
					int count = virtualChildren.Count;
					for (int i = 0; i < count; i++) {
						((RootNode) virtualChildren[i]).ValidateRoots();
					}
				}
				virtualChildren.Sort();
				virtualChildren.Minimize();
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
			}
			else if (Type == FileNodeType.Volume) {
				lock (virtualChildren) {
					int index = virtualChildren.IndexOf(unknown);
					if (index != -1 && !showUnknown) {
						RemoveAt(index);
						//size -= unknown.Size;
					}
					else if (index == -1 && showUnknown) {
						Add(unknown);
						//unknown.UpdateSize(GetUsedSize());
						//size += unknown.Size;
						//Sort(false);
					}
				}
				Invalidate();
			}
			if (IsAbsoluteRootType)
				ValidateRoots();
		}
	}
}
