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
using WinDirStat.Net.Drawing;
using WinDirStat.Net.Utils;
using WinDirStat.Net.Utils.Native;

namespace WinDirStat.Net.Data {
	internal partial class FileEnumerator : IDisposable {
		private class ScanState {
			public long TotalSize;
			public long FreeSpace;
			public long ScannedSize;
			public string RootPath;
			public bool IsDrive;
			public string RecycleBinPath;
			public RootNode Root;
		}
		private readonly WinDirDocument document;

		private List<ScanState> states;
		private long totalScannedSize;
		private bool allDrives;
		private long totalTotalSize;
		private long totalFreeSpace;

		private RootNode absoluteRoot;
		/*private TreemapItem itemRoot;
		private long totalSize;
		private long freeSpace;
		private long scannedSize;
		private string rootPath;
		private bool isDrive;
		private string recycleBinPath;*/
		private AutoResetEvent resumeEvent = new AutoResetEvent(false);
		private volatile bool isSuspended;

		public RootNode Root {
			get => absoluteRoot;
		}
		private bool IsSingle {
			get => states.Count == 1;
		}
		/*public bool IsDone {
			get => root?.IsDone ?? false;
		}*/
		public bool HasProgress {
			get => allDrives;
		}

		public double Progress {
			get {
				if (allDrives)
					return (double) totalScannedSize / (totalTotalSize - totalFreeSpace);
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

		private void FinishScan(CancellationToken token) {
			if (!token.IsCancellationRequested) {
				Stopwatch watch = Stopwatch.StartNew();
				Application.Current.Dispatcher.Invoke(() => {
					document.FinalValidate();
				});
				absoluteRoot.IsDone = true;
				Debug.WriteLine($"Took {watch.ElapsedMilliseconds}ms to call FinalValidate()");
			}
			GC.Collect();
		}

		internal void Scan(string rootPath, Action<RootNode> rootSetup, CancellationToken token) {
			allDrives = false;
			ScanState state = CreateState(rootPath, true);
			states = new List<ScanState>() { state };
			allDrives = states[0].IsDrive;

			totalTotalSize = state.TotalSize;
			totalFreeSpace = state.FreeSpace;

			SetupRoot(state.Root, rootSetup);

			try {
				ScanMtf(state, rootSetup, token);
			}
			catch (Exception) {
				ScanNative(rootSetup, token);
			}
			FinishScan(token);
		}
		internal void Scan(IEnumerable<string> rootPaths, Action<RootNode> rootSetup, CancellationToken token) {
			allDrives = false;
			if (rootPaths.Count() == 1) {
				Scan(rootPaths.First(), rootSetup, token);
				return;
			}

			states = rootPaths.Select(p => CreateState(p, false)).ToList();
			allDrives = !states.Any(s => !s.IsDrive);

			totalTotalSize = states.Sum(s => s.TotalSize);
			totalFreeSpace = states.Sum(s => s.FreeSpace);

			// Computer Root
			RootNode computerRoot = new RootNode(document);
			SetupRoot(computerRoot, rootSetup);
			FolderNode unusedFileCollection = null;
			foreach (ScanState state in states) {
				computerRoot.AddChild(computerRoot, state.Root, ref unusedFileCollection);
			}

			// Master file tables cannot be scanned inline, so scan them all first
			for (int i = 0; i < states.Count; i++) {
				ScanState state = states[i];
				try {
					ScanMtf(state, rootSetup, token);
					states.RemoveAt(i--);
				}
				catch (Exception ex) {
					Console.WriteLine(ex);
				}
			}

			// Are there any leftover states to work on?
			if (states.Any())
				ScanNative(rootSetup, token);
			
			FinishScan(token);
		}

		private ScanState CreateState(string rootPath, bool single) {
			ScanState state = new ScanState();
			state.RootPath = Path.GetFullPath(rootPath);
			state.IsDrive = PathUtils.IsSamePath(Path.GetPathRoot(state.RootPath), state.RootPath);
			state.RecycleBinPath = Path.Combine(Path.GetPathRoot(rootPath), "$Recycle.Bin");
			if (state.IsDrive) {
				Win32.GetDiskFreeSpaceEx(rootPath, out _, out ulong totalSize, out ulong freeSpace);
				state.TotalSize = (long) totalSize;
				state.FreeSpace = (long) freeSpace;
			}
			state.Root = new RootNode(document, new DirectoryInfo(state.RootPath), single);
			state.RootPath = state.RootPath.ToUpperInvariant();
			return state;
		}

		/*internal void Scan(string rootPath, Action<RootNode> rootSetup, CancellationToken token) {
			//if (root != null)
			//	throw new InvalidOperationException("File Enumerator has already been loaded!");
			isSuspended = false;
			rootPath = Path.GetFullPath(rootPath);
			string testPath = rootPath.ToUpperInvariant();
			isDrive = PathUtils.IsSamePath(Path.GetPathRoot(testPath), testPath);
			recycleBinPath = Path.Combine(Path.GetPathRoot(rootPath), "$Recycle.Bin");
			this.rootPath = rootPath;
			scannedSize = 0L;
			SetupDriveSizes();

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
		}*/

		private void SuspendCheck() {
			if (isSuspended)
				resumeEvent.WaitOne();
		}

		private bool SkipFile(ScanState state, string name, string path) {
			if (name.Length == 0)
				Console.WriteLine("WHAT");
			// We still want to see all those delicious files that were thrown away
			if (name[0] == '$' && !path.StartsWith(state.RecycleBinPath))
				return true;

			// Certified spam
			//if (string.Compare(name, "desktop.ini", true) == 0)
			//	return true;

			return false;
		}

		private void SetupRoot(RootNode root, Action<RootNode> rootSetup) {
			if (!root.IsAbsoluteRootType)
				return;
			this.absoluteRoot = root;
			Application.Current.Dispatcher.Invoke(() => {
				root.LoadIcon(root);
				root.IsExpanded = true;
				rootSetup?.Invoke(root);
			});
			//if (isDrive)
			//	root.AddFreeSpace(new FreeSpaceNode(rootPath), rootPath, true);
		}
		
		private void ScanMtf(ScanState state, Action<RootNode> rootSetup, CancellationToken token) {
			Dictionary<uint, FileNodeIndexes> fileNodes = new Dictionary<uint, FileNodeIndexes>();

			DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(state.RootPath));
			using (NtfsReader ntfs = new NtfsReader(driveInfo, RetrieveMode.StandardInformations)) {
				ReadMft(state, ntfs, fileNodes, rootSetup, token);
			}

			foreach (FileNodeIndexes indexes in fileNodes.Values) {
				FileNodeBase node = indexes.FileNode;
				//if (node.Type == FileNodeType.File)
				//	document.Extensions.Include(node.Extension, node.Size);
				if (indexes.NodeIndex == indexes.ParentNodeIndex) {
					// This must be a root
				}
				else if (fileNodes.TryGetValue(indexes.ParentNodeIndex, out var parentIndexes)) {
					((FolderNode) parentIndexes.FileNode).AddChild(state.Root, node, ref parentIndexes.FileCollection);
				}
				SuspendCheck();
				if (token.IsCancellationRequested)
					return;
			}
		}
		
		private void ReadMft(ScanState state, NtfsReader ntfs, Dictionary<uint, FileNodeIndexes> fileNodes,
			Action<RootNode> rootSetup, CancellationToken token)
		{
			foreach (INtfsNode node in ntfs.EnumerateNodes(state.RootPath)) {
				string fullPath = PathUtils.TrimSeparatorDotEnd(node.FullName);
				bool isRoot = (node.NodeIndex == node.ParentNodeIndex);
				if (!isRoot && SkipFile(state, Path.GetFileName(fullPath), fullPath))
					continue;
				FileNodeBase child;
				if (isRoot) {
					child = state.Root;
					/*state.Root = new RootNode(document, node, IsSingle);
					child = state.Root;
					// Let the user know that the root was found
					SetupRoot(state.Root, rootSetup);*/
				}
				else if (node.Attributes.HasFlag(FileAttributes.Directory)) {
					if (!state.IsDrive && state.RootPath == node.FullName.ToUpperInvariant()) {
						child = state.Root;
						/*state.Root = new RootNode(document, node, IsSingle);
						child = state.Root;
						// Let the user know that the root was found
						SetupRoot(state.Root, rootSetup);*/
					}
					else {
						child = new FolderNode(node);
					}
				}
				else {
					FileNode file = new FileNode(node);
					child = file;
					if (!node.Attributes.HasFlag(FileAttributes.ReparsePoint)) {
						state.ScannedSize += child.Size;
						totalScannedSize += child.Size;
					}
					file.extRecord = document.Extensions.Include(GetExtension(file.Name), child.Size);
				}
				fileNodes[node.NodeIndex] = new FileNodeIndexes(child, node);
				SuspendCheck();
				if (token.IsCancellationRequested)
					return;
			}
		}
		
		/*private void SetupDriveSizes() {
			if (isDrive) {
				Win32.GetDiskFreeSpaceEx(rootPath, out _, out ulong totalSize, out ulong freeSpace);
				this.totalSize = (long) totalSize;
				this.freeSpace = (long) freeSpace;
			}
		}*/

		private const string Dot = ".";

		private static string GetExtension(string path) {
			int length = path.Length;
			for (int i = length; --i >= 0;) {
				char ch = path[i];
				if (ch == '.') {
					if (i != length - 1)
						return path.Substring(i, length - i).ToLower();
					else
						return Dot;
				}
				if (ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar || ch == Path.VolumeSeparatorChar)
					break;
			}
			return Dot;
		}

		private class FileNodeIndexes {
			public FileNodeBase FileNode;
			public FolderNode FileCollection;
			public uint NodeIndex;
			public uint ParentNodeIndex;
			public string Path;

			public FileNodeIndexes(FileNodeBase fileNode, INtfsNode ntfsNode) {
				FileNode = fileNode;
				FileCollection = null;
				NodeIndex = ntfsNode.NodeIndex;
				ParentNodeIndex = ntfsNode.ParentNodeIndex;
				Path = ntfsNode.FullName;
				if (Path.EndsWith(@"\."))
					Path = Path.Substring(0, Path.Length - 1);
			}
		}
	}
}
