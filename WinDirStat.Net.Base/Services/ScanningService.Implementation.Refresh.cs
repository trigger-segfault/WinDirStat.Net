using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Extensions;
using WinDirStat.Net.Model.Files;

namespace WinDirStat.Net.Services {
	partial class ScanningService {
		
		protected void Refresh(RefreshFiles[] refreshFiles, CancellationToken token) {
			CanDisplayProgress = rootItem.Type == FileItemType.Volume ||
								(rootItem.Type == FileItemType.Computer &&
								!rootItem.Children.Any(f => f.Type != FileItemType.Volume &&
								f.Type != FileItemType.FreeSpace && f.Type != FileItemType.Unknown));
			if (CanDisplayProgress) {
				if (rootItem.Type == FileItemType.Volume) {
					try {
						if (rootItem.CheckExists()) {
							DriveInfo driveInfo = new DriveInfo(rootItem.RootPath);
							TotalSize = driveInfo.TotalSize;
							TotalFreeSpace = driveInfo.TotalFreeSpace;
						}
						else {
							CanDisplayProgress = false;
						}
					}
					catch {
						CanDisplayProgress = false;
					}
				}
				else if (rootItem.Type == FileItemType.Computer) {
					bool anySuccess = false;
					foreach (FileItemBase child in rootItem.Children) {
						if (child is RootItem root) {
							try {
								if (root.CheckExists()) {
									DriveInfo driveInfo = new DriveInfo(root.RootPath);
									TotalSize += driveInfo.TotalSize;
									TotalFreeSpace += driveInfo.TotalFreeSpace;
									anySuccess = true;
									continue;
								}
							}
							catch { }
							TotalSize += root.CachedTotalSize;
							TotalFreeSpace += root.CachedFreeSpace;
						}
					}
					if (!anySuccess)
						CanDisplayProgress = false;
				}
			}
			// Reset the progress if applicable
			TotalScannedSize = Extensions.TotalSize - refreshFiles.Sum(rf => rf.Files.Sum(f => f.Size));
			ProgressState = ScanProgressState.Started;

			foreach (RefreshFiles refresh in refreshFiles) {
				FolderItem parent = refresh.Parent;
				FolderItem fileCollection = parent.GetFileCollection();

				Queue<FolderItem> subdirs = new Queue<FolderItem>();

				foreach (FileItemBase refreshChild in refresh.Files) {
					if (!refreshChild.Refresh()) {
						string fullName = refreshChild.FullName;
						parent.RemoveItem(refreshChild, ref fileCollection);

						// See if the type changed (file <-> directory)
						FileItemBase child = null;
						if (refreshChild.Type == FileItemType.File) {
							DirectoryInfo info = new DirectoryInfo(fullName);
							if (info.Exists) {
								FolderItem folder = new FolderItem(info);
								child = folder;
								// Folder exists enqueue it for scanning
								if (!info.Attributes.HasFlag(FileAttributes.ReparsePoint))
									subdirs.Enqueue(folder);
							}
						}
						else if (refreshChild.Type == FileItemType.Directory) {
							FileInfo info = new FileInfo(fullName);
							if (info.Exists) {
								ExtensionItem extension = Extensions.GetOrAddFromPath(fullName);
								FileItem file = new FileItem(info, extension);
								child = file;
								// File exists, add it to the scanned size
								if (!info.Attributes.HasFlag(FileAttributes.ReparsePoint))
									TotalScannedSize += child.Size;
							}
						}
						if (child != null) {
							FileItem firstFile = parent.GetFirstFile();
							parent.AddItem(child, ref fileCollection, ref firstFile);
						}
					}
					else if (!refreshChild.IsReparsePointFile) {
						// Folder exists enqueue it for scanning
						if (refreshChild is FolderItem folder)
							subdirs.Enqueue(folder);
						// File exists, add it to the scanned size
						else
							TotalScannedSize += refreshChild.Size;
					}

					if (AsyncChecks(token))
						return;
				}

				if (subdirs.Count > 0)
					RefreshFolders(subdirs, token);

				if (AsyncChecks(token))
					return;
			}
		}

		private void RefreshFolders(Queue<FolderItem> subdirs, CancellationToken token) {
			scanningStates = new List<ScanningState>();
			foreach (FolderItem subdir in subdirs)
				scanningStates.Add(CreateState(subdir));

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

			if (scanningStates.Count > 0)
				ScanNative(token);

			scanningStates = null;
		}
	}
}
