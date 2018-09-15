using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Extensions;
using WinDirStat.Net.Model.Files;
using WinDirStat.Net.Utils;
using static WinDirStat.Net.Native.Win32;

namespace WinDirStat.Net.Services {
	partial class ScanningService {
		private struct FolderState {
			public FolderItem Folder;
			public ScanningState State;

			public FolderState(ScanningState state) {
				Folder = state.Root;
				State = state;
			}

			public FolderState(FolderItem folder, ScanningState state) {
				State = state;
				Folder = folder;
			}
		}

		private void ScanNative(CancellationToken token) {
			Queue<FolderState> subdirs = new Queue<FolderState>();
			foreach (ScanningState state in scanningStates)
				subdirs.Enqueue(new FolderState(state));
			do {
				ReadNative(subdirs, token);
			} while (subdirs.Any() && !AsyncChecks(token));
		}

		private void ReadNative(Queue<FolderState> subdirs, CancellationToken token) {
			Win32FindData find = new Win32FindData();
			FolderState folderState = subdirs.Dequeue();
			ScanningState state = folderState.State;
			FolderItem parent = folderState.Folder;
			bool findResult;
			string parentPath = parent.FullName;
			string searchPattern = PathUtils.CombineNoChecks(parentPath, "*");
			if (!searchPattern.StartsWith(@"\\?\"))
				searchPattern = @"\\?\" + searchPattern;
			IntPtr hFind = FindFirstFileEx(searchPattern,
										   FindExInfoLevels.Basic, out find,
										   FindExSearchOps.NameMatch, IntPtr.Zero,
										   FindExFlags.LargeFetch);
			if (hFind == InvalidHandle)
				return;

			FolderItem fileCollection = null;
			FileItem firstFile = null;
			bool subdirsAdded = false;
			try {
				do {
					string filePath = PathUtils.CombineNoChecks(parentPath, find.cFileName);
					if (find.IsRelativeDirectory || SkipFile(state, find.cFileName, filePath)) {
						// Skip these types of entries
						findResult = FindNextFile(hFind, out find);
						continue;
					}
					FileItemBase child;
					if (find.IsDirectory) {
						FolderItem folder = new FolderItem(new ScanFileInfo(find, filePath));
						child = folder;
						if (!find.IsSymbolicLink) {
							subdirsAdded = true;
							subdirs.Enqueue(new FolderState(folder, state));
						}
					}
					else {
						ExtensionItem extension = Extensions.GetOrAddFromPath(filePath);
						FileItem file = new FileItem(new ScanFileInfo(find, filePath), extension);
						child = file;
						if (!find.IsSymbolicLink)
							TotalScannedSize += child.Size;
					}
					parent.AddItem(child, ref fileCollection, ref firstFile);
					
					if (AsyncChecks(token))
						return;

					findResult = FindNextFile(hFind, out find);
				} while (findResult);

				if (!subdirsAdded)
					parent.Finish();
				//if (parent.IsWatched)
				//	parent.RaiseChanged(FileItemAction.ChildrenDone);
			}
			finally {
				FindClose(hFind);
			}
		}
	}
}
