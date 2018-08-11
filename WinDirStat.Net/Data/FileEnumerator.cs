using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Filesystem.Ntfs;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32.SafeHandles;
using WinDirStat.Net.Data.Nodes;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Data {
	internal partial class FileEnumerator : IDisposable {
		private readonly WinDirDocument document;

		private RootNode root;

		private long totalSize;
		private long freeSpace;
		private long scannedSize;
		private string rootPath;
		private bool isRoot;
		private string recycleBinPath;
		private AutoResetEvent resumeEvent = new AutoResetEvent(false);
		private volatile bool isSuspended;

		public RootNode Root {
			get => root;
		}
		/*public bool IsDone {
			get => root?.IsDone ?? false;
		}*/
		public bool HasProgress {
			get => isRoot;
		}

		public double Progress {
			get {
				if (isRoot)
					return (double) scannedSize / (totalSize - freeSpace);
				return 0d;
			}
		}

		public FileEnumerator(WinDirDocument document) {
			this.document = document;
		}


		public bool IsSuspended {
			get => isSuspended;
			set {
				if (isSuspended != value) {
					isSuspended = value;
					if (!value)
						resumeEvent.Set();
				}
			}
		}

		public void Dispose() {
			resumeEvent.Dispose();
		}

		public WinDirDocument Document {
			get => document;
		}

		internal void Scan(string rootPath, Action<RootNode> rootSetup, CancellationToken token) {
			//if (root != null)
			//	throw new InvalidOperationException("File Enumerator has already been loaded!");
			isSuspended = false;
			rootPath = Path.GetFullPath(rootPath);
			string testPath = rootPath.ToUpperInvariant();
			isRoot = PathUtils.IsSamePath(Path.GetPathRoot(testPath), testPath);
			recycleBinPath = Path.Combine(Path.GetPathRoot(rootPath), "$Recycle.Bin");
			this.rootPath = rootPath;
			if (isRoot) {
				freeSpace = GetAvailableFreeSpace();
				totalSize = GetTotalSize();
				scannedSize = 0L;
			}

			if (!Directory.Exists(rootPath))
				throw new DirectoryNotFoundException(rootPath);

			try {
				ScanMtf(rootSetup, token);
			}
			catch (Exception) {
				ScanNative(rootSetup, token);
			}
			if (!token.IsCancellationRequested) {
				Stopwatch watch = Stopwatch.StartNew();
				Application.Current.Dispatcher.Invoke(() => {
					document.FinalValidate();
				});
				root.IsDone = true;
				//document.FinalValidate();
				Debug.WriteLine($"Took {watch.ElapsedMilliseconds}ms to call FinalValidate()");
			}
			GC.Collect();
		}

		private void SuspendCheck() {
			if (isSuspended)
				resumeEvent.WaitOne();
		}

		private bool SkipFile(string name, string path) {
			// We still want to see all those delicious files that were thrown away
			if (name.StartsWith("$") && !path.StartsWith(recycleBinPath))
				return true;

			// Certified spam
			if (string.Compare(name, "desktop.ini", true) == 0)
				return true;

			return false;
		}

		private void SetupRoot(Action<RootNode> rootSetup) {
			Application.Current.Dispatcher.Invoke(() => {
				root.LoadIcon(root);
				root.IsExpanded = true;
				rootSetup?.Invoke(root);
			});
			if (isRoot)
				root.AddFreeSpace(new FreeSpaceNode(rootPath, document.Settings.ShowFreeSpace), rootPath, true);
		}

		private void ReadSlow(Action<RootNode> rootSetup, CancellationToken token) {
			root = new RootNode(document, new DirectoryInfo(rootPath));
			SetupRoot(rootSetup);
			Queue<FolderNode> subdirs = new Queue<FolderNode>();
			subdirs.Enqueue(root);
			do {
				SuspendCheck();
				ReadSlow(subdirs, token);
			} while (subdirs.Any() && !token.IsCancellationRequested);
		}

		private void ReadSlow(Queue<FolderNode> subdirs, CancellationToken token) {
			FolderNode parent = subdirs.Dequeue();
			try {
				foreach (string path in Directory.EnumerateFileSystemEntries(parent.Path).AsParallel()) {
					if (SkipFile(Path.GetFileName(path), path))
						continue;
					try {
						// FileInfo works for directories with the exception
						// of not supporting the Length property.
						FileInfo info = new FileInfo(path);
						FileNode child;
						if (info.Attributes.HasFlag(FileAttributes.Directory)) {
							FolderNode subdir = new FolderNode(info);
							child = subdir;
							subdirs.Enqueue(subdir);
						}
						else {
							child = new FileNode(info);
							scannedSize += child.Size;
							document.Extensions.Include(child.Extension, child.Size);
						}
						parent.AddChild(root, child, path, true);
					}
					catch { }
					SuspendCheck();
					if (token.IsCancellationRequested)
						return;
				}
			}
			catch { }
			if (parent.IsExpanded) {
				Application.Current.Dispatcher.Invoke(() => {
					parent.Validate(root, false);
				});
			}
		}

		private void ScanMtf(Action<RootNode> rootSetup, CancellationToken token) {
			Dictionary<uint, FileNodeIndexes> fileNodes = new Dictionary<uint, FileNodeIndexes>();

			DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(rootPath));
			using (NtfsReader ntfs = new NtfsReader(driveInfo, RetrieveMode.StandardInformations)) {
				ReadMft(ntfs, fileNodes, rootSetup, token);
			}

			foreach (FileNodeIndexes indexes in fileNodes.Values) {
				FileNode node = indexes.FileNode;
				//if (node.Type == FileNodeType.File)
				//	document.Extensions.Include(node.Extension, node.Size);
				if (indexes.NodeIndex == indexes.ParentNodeIndex) {
					// This must be a root
				}
				else if (fileNodes.TryGetValue(indexes.ParentNodeIndex, out var parentIndexes)) {
					((FolderNode) parentIndexes.FileNode).AddChild(root, node, indexes.Path, true);
				}
				SuspendCheck();
				if (token.IsCancellationRequested)
					return;
			}
		}
		
		private void ReadMft(NtfsReader ntfs, Dictionary<uint, FileNodeIndexes> fileNodes,
			Action<RootNode> rootSetup, CancellationToken token)
		{
			foreach (INtfsNode node in ntfs.EnumerateNodes(rootPath)) {
				string fullPath = node.FullName;
				if (fullPath.EndsWith(@"\."))
					fullPath = fullPath.Substring(0, fullPath.Length - 1);
				string name = Path.GetFileName(fullPath);
				if (SkipFile(name, fullPath))
					continue;
				FileNode child;
				if (node.NodeIndex == node.ParentNodeIndex) {
					root = new RootNode(document, node);
					child = root;
					// Let the user know that the root was found
					SetupRoot(rootSetup);
				}
				else if (node.Attributes.HasFlag(FileAttributes.Directory)) {
					if (!isRoot && rootPath == node.FullName.ToUpperInvariant()) {
						root = new RootNode(document, node);
						child = root;
						// Let the user know that the root was found
						SetupRoot(rootSetup);
					}
					else {
						child = new FolderNode(node);
					}
				}
				else {
					child = new FileNode(node);
					if (!node.Attributes.HasFlag(FileAttributes.ReparsePoint))
						scannedSize += child.Size;
					document.Extensions.Include(child.Extension, child.Size);
				}
				fileNodes[node.NodeIndex] = new FileNodeIndexes(child, node);
				SuspendCheck();
				if (token.IsCancellationRequested)
					return;
			}
		}

		private long GetAvailableFreeSpace() {
			GetDiskFreeSpaceEx(rootPath, out _, out _, out ulong freeSpace);
			return (long) freeSpace;
		}
		private long GetTotalSize() {
			GetDiskFreeSpaceEx(rootPath, out _, out ulong totalSize, out _);
			return (long) totalSize;
		}

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);
		
		private struct FileNodeIndexes {
			public FileNode FileNode { get; }
			public uint NodeIndex { get; }
			public uint ParentNodeIndex { get; }
			public string Path { get; }

			public FileNodeIndexes(FileNode fileNode, INtfsNode ntfsNode) {
				FileNode = fileNode;
				NodeIndex = ntfsNode.NodeIndex;
				ParentNodeIndex = ntfsNode.ParentNodeIndex;
				Path = ntfsNode.FullName;
				if (Path.EndsWith(@"\."))
					Path = Path.Substring(0, Path.Length - 1);
			}
		}
	}
}
