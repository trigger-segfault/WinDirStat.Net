using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Filesystem.Ntfs;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Data.Nodes;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Model.Data {
	partial class WinDirStatModel {
		private class FileNodeIndexes {
			public FileNodeBase FileNode;
			public FolderNode FileCollection;
			public FileNodeBase SingleFile;
			public uint NodeIndex;
			public uint ParentNodeIndex;
			public string Path;

			public FileNodeIndexes(FileNodeBase fileNode, INtfsNode ntfsNode) {
				FileNode = fileNode;
				NodeIndex = ntfsNode.NodeIndex;
				ParentNodeIndex = ntfsNode.ParentNodeIndex;
				Path = ntfsNode.FullName;
				if (Path.EndsWith(@"\."))
					Path = Path.Substring(0, Path.Length - 1);
			}
		}

		private void ScanMtf(ScanningState state, CancellationToken token) {
			Dictionary<uint, FileNodeIndexes> fileNodes = new Dictionary<uint, FileNodeIndexes>();

			DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(state.RootPath));
			using (NtfsReader ntfs = new NtfsReader(driveInfo, RetrieveMode.StandardInformations)) {
				ReadMft(state, ntfs, fileNodes, token);
			}

			foreach (FileNodeIndexes indexes in fileNodes.Values) {
				FileNodeBase node = indexes.FileNode;
				//if (node.Type == FileNodeType.File)
				//	document.Extensions.Include(node.Extension, node.Size);
				if (indexes.NodeIndex == indexes.ParentNodeIndex) {
					// This must be a root
				}
				else if (fileNodes.TryGetValue(indexes.ParentNodeIndex, out var parentIndexes)) {
					((FolderNode) parentIndexes.FileNode).AddChild(node,
						ref parentIndexes.FileCollection, ref parentIndexes.SingleFile);
				}
				SuspendCheck();
				if (token.IsCancellationRequested)
					return;
			}
		}

		private void ReadMft(ScanningState state, NtfsReader ntfs, Dictionary<uint, FileNodeIndexes> fileNodes,
			CancellationToken token)
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
					file.extRecord = extensions.Include(GetExtension(file.Name), child.Size);
				}
				fileNodes[node.NodeIndex] = new FileNodeIndexes(child, node);
				SuspendCheck();
				if (token.IsCancellationRequested)
					return;
			}
		}
	}
}
