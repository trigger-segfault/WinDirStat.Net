using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WinDirStat.Net.Model.Data.Nodes;
using WinDirStat.Net.Utils;
using static WinDirStat.Net.Utils.Native.Win32;

namespace WinDirStat.Net.Model.Data {
	partial class WinDirStatModel {
		private struct FolderState {
			public FolderNode Folder;
			public ScanningState State;

			public FolderState(ScanningState state) {
				Folder = state.Root;
				State = state;
			}

			public FolderState(FolderNode folder, ScanningState state) {
				State = state;
				Folder = folder;
			}
		}
		
		private void ScanNative(CancellationToken token) {
			//root = new RootNode(document, new DirectoryInfo(rootPath), true);
			//SetupRoot(rootSetup);
			Queue<FolderState> subdirs = new Queue<FolderState>();
			foreach (ScanningState state in scanningStates)
				subdirs.Enqueue(new FolderState(state));
			do {
				SuspendCheck();
				ReadNative(subdirs, token);
			} while (subdirs.Any() && !token.IsCancellationRequested);
		}

		private void ReadNative(Queue<FolderState> subdirs, CancellationToken token) {
			Win32FindData find = new Win32FindData();
			FolderState folderState = subdirs.Dequeue();
			ScanningState state = folderState.State;
			FolderNode parent = folderState.Folder;
			bool findResult;
			string parentPath = parent.Path;
			string searchPattern = PathUtils.CombineNoChecks(parentPath, "*");
			if (!searchPattern.StartsWith(@"\\?\"))
				searchPattern = @"\\?\" + searchPattern;
			IntPtr hFind = FindFirstFileEx(searchPattern,
										   FindExInfoLevels.Basic, out find,
										   FindExSearchOps.NameMatch, IntPtr.Zero,
										   FindExFlags.LargeFetch);
			if (hFind == InvalidHandle)
				return;

			FolderNode fileCollection = null;
			FileNodeBase singleFile = null;
			try {
				do {
					string filePath = PathUtils.CombineNoChecks(parentPath, find.cFileName);
					if (find.IsRelativeDirectory || SkipFile(state, find.cFileName, filePath)) {
						// Skip these types of entries
						findResult = FindNextFile(hFind, out find);
						continue;
					}
					FileNodeBase child;
					if (find.IsDirectory) {
						FolderNode folder = new FolderNode(find);
						child = folder;
						subdirs.Enqueue(new FolderState(folder, state));
					}
					else {
						FileNode file = new FileNode(find);
						child = file;
						if (!find.IsSymbolicLink) {
							state.ScannedSize += child.Size;
							totalScannedSize += child.Size;
						}
						file.extRecord = extensions.Include(GetExtension(file.Name), child.Size);
					}
					parent.AddChild(child, ref fileCollection, ref singleFile);

					SuspendCheck();
					if (token.IsCancellationRequested)
						return;

					findResult = FindNextFile(hFind, out find);
				} while (findResult);

				//if (parent.IsWatched)
				//	parent.RaiseChanged(FileNodeAction.ChildrenDone);
			}
			finally {
				FindClose(hFind);
			}
		}
	}
}
