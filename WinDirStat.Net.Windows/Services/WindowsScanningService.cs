using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using WinDirStat.Net.Model.Extensions;
using WinDirStat.Net.Model.Files;
using WinDirStat.Net.Services;
using WinDirStat.Net.Utils;
using WinDirStat.Net.Windows.Native;
using System.Runtime.InteropServices;
using WinDirStat.Net.Services.Structures;

namespace WinDirStat.Net.Windows.Services {
	public partial class WindowsScanningService : ScanningService {

		#region Private Classes

		private class ScanningState {
			public long TotalSize;
			public long FreeSpace;
			public string RootPath;
			public bool IsDrive;
			public string RecycleBinPath;
			public FolderItem Root;
		}

		#endregion

		#region Fields

		private List<ScanningState> scanningStates;

		#endregion

		#region Constructors

		public WindowsScanningService(SettingsService settings,
									  IOSService os,
									  IUIService ui)
			: base(settings,
				   os,
				   ui)
		{
		}

		#endregion

		public long GetCompressedFileSize(string filePath) {
			uint low = Win32.GetCompressedFileSize(filePath, out int high);
			if (low == Win32.InvalidFileSize) {
				int error = Marshal.GetLastWin32Error();
				if (error != 0)
					throw new Win32Exception(error);
			}
			return ((long) high << 32) | low;
		}

		/// <summary>Runs the actual scan on any number of root paths.</summary>
		/// 
		/// <param name="rootPaths">The root paths to scan.</param>
		/// <param name="token">The token for cleanly cancelling the operations.</param>
		protected override void Scan(string[] rootPaths, CancellationToken token) {
			// See if we can perform a single scan
			if (rootPaths.Length == 1) {
				Scan(rootPaths[0], token);
				return;
			}

			scanningStates = rootPaths.Select(p => CreateState(p, false)).ToList();
			CanDisplayProgress = !scanningStates.Any(s => !s.IsDrive);

			TotalSize = scanningStates.Sum(s => s.TotalSize);
			TotalFreeSpace = scanningStates.Sum(s => s.FreeSpace);

			// Computer Root
			RootItem computerRoot = new RootItem(this);
			foreach (ScanningState state in scanningStates) {
				computerRoot.AddItem(state.Root);
			}
			
			RootItem = computerRoot;
			ProgressState = ScanProgressState.Started;

			// Master file tables cannot be scanned inline, so scan them one at a time first
			for (int i = 0; i < scanningStates.Count; i++) {
				ScanningState state = scanningStates[i];
				try {
					ScanMtf(state, token);
					scanningStates.RemoveAt(i--);
				}
				catch (Exception) {
					// We don't have permission, are not elevated, or path is not an NTFS drive
					// We'll scan this path normally instead
				}
			}

			// Are there any leftover states to work on?
			if (scanningStates.Any())
				ScanNative(token);

			FinishScan(token);
		}

		/// <summary>Runs the actual scan on a single root path.</summary>
		/// 
		/// <param name="rootPath">The single root path to scan.</param>
		/// <param name="token">The token for cleanly cancelling the operations.</param>
		private void Scan(string rootPath, CancellationToken token) {
			ScanningState state = CreateState(rootPath, true);
			scanningStates = new List<ScanningState>() { state };
			CanDisplayProgress = scanningStates[0].IsDrive;

			TotalSize = state.TotalSize;
			TotalFreeSpace = state.FreeSpace;
			
			RootItem = (RootItem) state.Root;
			ProgressState = ScanProgressState.Started;

			try {
				ScanMtf(state, token);
			}
			catch (Exception) {
				// We don't have permission, are not elevated, or path is not an NTFS drive
				// We'll scan this path normally instead
				ScanNative(token);
			}
			FinishScan(token);
		}

		/// <summary>Performs the final opreations after a scan.</summary>
		/// <param name="token"></param>
		private void FinishScan(CancellationToken token) {
			scanningStates = null;
			if (!token.IsCancellationRequested) {
				RootItem.Finish();
			}
		}

		/// <summary>Creates a <see cref="ScanningState"/> for the specified root path.</summary>
		/// 
		/// <param name="rootPath">The path to create a state for.</param>
		/// <param name="single">
		/// True if the there is only one state.<para/>
		/// This means this is the absolute root item instead of Computer.
		/// </param>
		/// <returns>The newly created and initialized <see cref="ScanningState"/>.</returns>
		private ScanningState CreateState(string rootPath, bool single) {
			rootPath = Path.GetFullPath(rootPath);
			string pathRoot = Path.GetPathRoot(rootPath);
			ScanningState state = new ScanningState {
				IsDrive = PathUtils.IsSamePath(pathRoot, rootPath),
				RecycleBinPath = Path.Combine(pathRoot, "$Recycle.Bin"),
				Root = new RootItem(this, new DirectoryInfo(rootPath), single),
				RootPath = rootPath.ToUpperInvariant(),
			};
			if (state.IsDrive) {
				Win32.GetDiskFreeSpaceEx(rootPath, out _, out ulong totalSize, out ulong freeSpace);
				state.TotalSize = (long) totalSize;
				state.FreeSpace = (long) freeSpace;
			}
			return state;
		}

		/// <summary>
		/// Creates a <see cref="ScanningState"/> for the specified <see cref="FolderItem"/>.
		/// </summary>
		/// 
		/// <param name="folder">The folder to create the state for.</param>
		/// <returns>The newly created and initialized <see cref="ScanningState"/>.</returns>
		private ScanningState CreateState(FolderItem folder) {
			string rootPath = folder.FullName;
			string pathRoot = Path.GetPathRoot(rootPath);
			ScanningState state = new ScanningState {
				IsDrive = PathUtils.IsSamePath(pathRoot, rootPath),
				RecycleBinPath = Path.Combine(pathRoot, "$Recycle.Bin"),
				Root = folder,
				RootPath = rootPath.ToUpperInvariant(),
			};
			if (state.IsDrive) {
				Win32.GetDiskFreeSpaceEx(rootPath, out _, out ulong totalSize, out ulong freeSpace);
				state.TotalSize = (long) totalSize;
				state.FreeSpace = (long) freeSpace;
			}
			return state;
		}

		/// <summary>Checks if we should ignore this file in the scan.</summary>
		/// 
		/// <param name="state">The scan state for this file.</param>
		/// <param name="name">The name of the file.</param>
		/// <param name="path">The full path of the file.</param>
		/// <returns>True if the file should be skipped.</returns>
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

		protected override void FinishedCleanup() {
			scanningStates = null;
		}

		protected override void ClosedCleanup() {
		}
	}
}
