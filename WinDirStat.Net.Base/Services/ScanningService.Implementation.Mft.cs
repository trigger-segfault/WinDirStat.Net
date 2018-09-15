using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Filesystem.Ntfs;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Extensions;
using WinDirStat.Net.Model.Files;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Services {
	partial class ScanningService {

		#region Private Structures

		private class FileNodeIndexes {
			public FileItemBase FileItem;
			public FolderItem FileCollection;
			public FileItem FirstFile;
			public uint NodeIndex;
			public uint ParentNodeIndex;
			public string FullName;

			public FileNodeIndexes(FileItemBase fileItem, INtfsNode ntfsNode) {
				FileItem = fileItem;
				NodeIndex = ntfsNode.NodeIndex;
				ParentNodeIndex = ntfsNode.ParentNodeIndex;
				FullName = PathUtils.TrimSeparatorDotEnd(ntfsNode.FullName);
			}
		}

		#endregion

		private void ScanMtf(ScanningState state, CancellationToken token) {
			FileNodeIndexes[] fileItemsLookup;
			List<FileNodeIndexes> fileItems = new List<FileNodeIndexes>();

			DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(state.RootPath));
			using (NtfsReader ntfs = new NtfsReader(driveInfo, RetrieveMode.StandardInformations)) {
				fileItemsLookup = new FileNodeIndexes[ntfs.NodeCount];
				ReadMft(state, ntfs, fileItemsLookup, fileItems, token);
			}

			foreach (FileNodeIndexes indexes in fileItems) {
				FileItemBase item = indexes.FileItem;
				FileNodeIndexes parentIndexes = fileItemsLookup[indexes.ParentNodeIndex];
				if (indexes.NodeIndex == indexes.ParentNodeIndex) {
					// This must be the drive root
				}
				else if (parentIndexes != null) {
					((FolderItem) parentIndexes.FileItem).AddItem(item,
						ref parentIndexes.FileCollection, ref parentIndexes.FirstFile);
				}
				else {
					// This must be the root
				}
				if (AsyncChecks(token))
					return;
			}
		}

		private void ReadMft(ScanningState state, NtfsReader ntfs, FileNodeIndexes[] fileItemsLookup,
			List<FileNodeIndexes> fileItems, CancellationToken token)
		{
			foreach (INtfsNode node in ntfs.EnumerateNodes(state.RootPath)) {
				string fullPath = PathUtils.TrimSeparatorDotEnd(node.FullName);
				bool isRoot = (node.NodeIndex == node.ParentNodeIndex);
				if (!isRoot && SkipFile(state, Path.GetFileName(fullPath), fullPath))
					continue;
				FileItemBase child;
				bool reparsePoint = node.Attributes.HasFlag(FileAttributes.ReparsePoint);
				if (isRoot) {
					child = state.Root;
				}
				else if (node.Attributes.HasFlag(FileAttributes.Directory)) {
					if (!state.IsDrive && state.RootPath == node.FullName.ToUpperInvariant()) {
						child = state.Root;
					}
					else {
						child = new FolderItem(new ScanFileInfo(node));
					}
				}
				else {
					ExtensionItem extension = Extensions.GetOrAddFromPath(node.Name);
					FileItem file = new FileItem(new ScanFileInfo(node), extension);
					child = file;
					if (!reparsePoint)
						TotalScannedSize += child.Size;
				}
				FileNodeIndexes indexes = new FileNodeIndexes(child, node);
				fileItemsLookup[node.NodeIndex] = indexes;

				if (!reparsePoint)
					fileItems.Add(indexes);

				if (AsyncChecks(token))
					return;
			}
		}


		/*private void ScanMtf(ScanningState state, CancellationToken token) {
			Dictionary<uint, FileNodeIndexes> fileItems = new Dictionary<uint, FileNodeIndexes>();

			DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(state.RootPath));
			using (NtfsReader ntfs = new NtfsReader(driveInfo, RetrieveMode.StandardInformations)) {
				ReadMft(state, ntfs, fileItems, token);
			}

			foreach (FileNodeIndexes indexes in fileItems.Values) {
				FileItemBase item = indexes.FileItem;
				if (indexes.NodeIndex == indexes.ParentNodeIndex) {
					// This must be the drive root
				}
				else if (fileItems.TryGetValue(indexes.ParentNodeIndex, out var parentIndexes)) {
					((FolderItem) parentIndexes.FileItem).AddItem(item,
						ref parentIndexes.FileCollection, ref parentIndexes.FirstFile);
				}
				else {
					// This must be the root
				}
				if (AsyncChecks(token))
					return;
			}
		}

		private void ReadMft(ScanningState state, NtfsReader ntfs, Dictionary<uint, FileNodeIndexes> fileNodes,
			CancellationToken token) {
			foreach (INtfsNode node in ntfs.EnumerateNodes(state.RootPath)) {
				string fullPath = PathUtils.TrimSeparatorDotEnd(node.FullName);
				bool isRoot = (node.NodeIndex == node.ParentNodeIndex);
				if (!isRoot && SkipFile(state, Path.GetFileName(fullPath), fullPath))
					continue;
				FileItemBase child;
				if (isRoot) {
					child = state.Root;
				}
				else if (node.Attributes.HasFlag(FileAttributes.Directory)) {
					if (!state.IsDrive && state.RootPath == node.FullName.ToUpperInvariant()) {
						child = state.Root;
					}
					else {
						child = new FolderItem(new ScanFileInfo(node));
					}
				}
				else {
					ExtensionItem extension = Extensions.GetOrAddFromPath(node.Name);
					FileItem file = new FileItem(new ScanFileInfo(node), extension);
					child = file;
					if (!node.Attributes.HasFlag(FileAttributes.ReparsePoint)) {
						state.ScannedSize += child.Size;
						TotalScannedSize += child.Size;
					}
				}
				fileNodes[node.NodeIndex] = new FileNodeIndexes(child, node);

				if (AsyncChecks(token))
					return;
			}
		}*/
	}
}
