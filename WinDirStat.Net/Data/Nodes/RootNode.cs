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

		private readonly WinDirDocument document;
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

		public override WinDirDocument Document {
			get => document;
		}
	}
}
