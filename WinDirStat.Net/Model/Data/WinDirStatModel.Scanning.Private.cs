using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WinDirStat.Net.Model.Data.Nodes;
using WinDirStat.Net.Utils;
using WinDirStat.Net.Utils.Native;

namespace WinDirStat.Net.Model.Data {
	partial class WinDirStatModel {
		private class ScanningState {
			public long TotalSize;
			public long FreeSpace;
			public long ScannedSize;
			public string RootPath;
			public bool IsDrive;
			public string RecycleBinPath;
			public RootNode Root;
		}


		private bool IsSingleScanRoot {
			get => scanningStates.Count == 1;
		}

		private bool IsScanSuspended {
			get => isScanSuspended;
			set {
				if (isScanSuspended != value) {
					isScanSuspended = value;
					if (!value)
						resumeEvent.Set();
				}
			}
		}

		private void FinishScan(CancellationToken token) {
			if (!token.IsCancellationRequested) {
				Stopwatch watch = Stopwatch.StartNew();
				scanningRootNode.FinalValidate();
				scanningRootNode.IsDone = true;
				Debug.WriteLine($"Took {watch.ElapsedMilliseconds}ms to call FinalValidate()");
			}
			GC.Collect();
		}

		private void Scan(string rootPath, CancellationToken token) {
			ScanningState state = CreateState(rootPath, true);
			scanningStates = new List<ScanningState>() { state };
			CanDisplayProgress = scanningStates[0].IsDrive;

			scanTotalSize = state.TotalSize;
			scanTotalFreeSpace = state.FreeSpace;

			SetupRoot(state.Root);

			RootNode = scanningRootNode;
			ProgressState = ScanProgressState.Started;

			try {
				ScanMtf(state, token);
			}
			catch (Exception) {
				ScanNative(token);
			}
			FinishScan(token);
		}

		private void Scan(IEnumerable<string> rootPaths, CancellationToken token) {
			if (rootPaths.Count() == 1) {
				Scan(rootPaths.First(), token);
				return;
			}

			scanningStates = rootPaths.Select(p => CreateState(p, false)).ToList();
			CanDisplayProgress = !scanningStates.Any(s => !s.IsDrive);

			scanTotalSize = scanningStates.Sum(s => s.TotalSize);
			scanTotalFreeSpace = scanningStates.Sum(s => s.FreeSpace);

			// Computer Root
			RootNode computerRoot = new RootNode(this);
			FolderNode unusedFileCollection = null;
			FileNodeBase unusedFile = null;
			foreach (ScanningState state in scanningStates) {
				computerRoot.AddChild(state.Root, ref unusedFileCollection, ref unusedFile);
			}

			SetupRoot(computerRoot);
			RootNode = scanningRootNode;
			ProgressState = ScanProgressState.Started;

			// Master file tables cannot be scanned inline, so scan them all first
			for (int i = 0; i < scanningStates.Count; i++) {
				ScanningState state = scanningStates[i];
				try {
					ScanMtf(state, token);
					scanningStates.RemoveAt(i--);
				}
				catch (Exception) { }
			}

			// Are there any leftover states to work on?
			if (scanningStates.Any())
				ScanNative(token);

			FinishScan(token);
		}

		private ScanningState CreateState(string rootPath, bool single) {
			rootPath = Path.GetFullPath(rootPath);
			string pathRoot = Path.GetPathRoot(rootPath);
			ScanningState state = new ScanningState {
				IsDrive = PathUtils.IsSamePath(pathRoot, rootPath),
				RecycleBinPath = Path.Combine(pathRoot, "$Recycle.Bin"),
				Root = new RootNode(this, new DirectoryInfo(rootPath), single),
				RootPath = rootPath.ToUpperInvariant(),
			};
			if (state.IsDrive) {
				Win32.GetDiskFreeSpaceEx(rootPath, out _, out ulong totalSize, out ulong freeSpace);
				state.TotalSize = (long) totalSize;
				state.FreeSpace = (long) freeSpace;
			}
			return state;
		}

		private void SuspendCheck() {
			if (isScanSuspended)
				resumeEvent.WaitOne();
		}

		private bool SkipFile(ScanningState state, string name, string path) {
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

		private void SetupRoot(RootNode root) {
			if (!root.IsAbsoluteRootType)
				return;
			if (scanState != ScanState.Cancelling)
				scanningRootNode = root;
		}


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
	}
}
