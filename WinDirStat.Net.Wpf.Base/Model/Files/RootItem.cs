using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Services;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Model.Files {
	/// <summary>
	/// A root file tree item that is either the absolute root, a file root (volume/folder), or both.
	/// If this root is not a file root, it's children must all be <see cref="RootItem"/>s.
	/// </summary>
	[Serializable]
	public class RootItem : FolderItem, IDisposable {

		#region Fields

		/// <summary>Gets the service used for scanning the file tree.</summary>
		private readonly ScanningService scanning;

		/// <summary>Gets the root path of the container. This includes the name of the item.</summary>
		public string RootPath { get; private set; }
		/// <summary>Gets the free space item associated with this root.</summary>
		public FreeSpaceItem FreeSpace { get; private set; }
		/// <summary>Gets the unknown space item associated with this root.</summary>
		public UnknownItem Unknown { get; private set; }
		/// <summary>The cached total size of the drive.</summary>
		public long CachedTotalSize { get; private set; }
		/// <summary>The cached free space of the drive.</summary>
		public long CachedFreeSpace { get; private set; }

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="RootItem"/> as a Computer.</summary>
		/// 
		/// <param name="scanning">The service used for scanning the file tree.</param>
		public RootItem(ScanningService scanning)
			: base(StringConstants.ComputerName, FileItemType.Computer,
				  FileItemFlags.ContainerType | FileItemFlags.AbsoluteRootType)
		{
			this.scanning = scanning;
			scanning.SpaceChanged += UpdateSpace;
			SetupRoot();
		}

		/// <summary>Constructs the <see cref="RootItem"/> with a <see cref="DirectoryInfo"/>.</summary>
		/// 
		/// <param name="scanning">The service used for scanning the file tree.</param>
		/// <param name="info">The file information.</param>
		/// <param name="isAbsoluteRoot">True if there is no <see cref="RootItem"/> containing this.</param>
		public RootItem(ScanningService scanning, DirectoryInfo info, bool isAbsoluteRoot)
			: base(info, GetType(info.FullName), GetFlags(isAbsoluteRoot))
		{
			this.scanning = scanning;
			if (isAbsoluteRoot)
				scanning.SpaceChanged += UpdateSpace;
			RootPath = Path.GetFullPath(info.FullName);
			if (Type == FileItemType.Volume)
				RootPath = PathUtils.AddDirectorySeparator(RootPath);
			SetupRoot();
		}

		private void SetupRoot() {
			if (Type == FileItemType.Volume || Type == FileItemType.Computer) {
				FreeSpace = new FreeSpaceItem(this);
				Unknown = new UnknownItem(this);
				if (scanning.ShowFreeSpace)
					Add(FreeSpace);
				if (scanning.ShowUnknown)
					Add(Unknown);
				if (Type == FileItemType.Volume) {
					DriveInfo driveInfo = new DriveInfo(RootPath);
					CachedTotalSize = driveInfo.TotalSize;
					CachedFreeSpace = driveInfo.TotalFreeSpace;
				}
			}
		}

		private static FileItemType GetType(string path) {
			return (PathUtils.IsPathRoot(path) ? FileItemType.Volume : FileItemType.Directory);
		}

		private static FileItemFlags GetFlags(bool isAbsoluteRoot) {
			return (isAbsoluteRoot ? FileItemFlags.AbsoluteRootType : FileItemFlags.None) |
				FileItemFlags.FileRootType | FileItemFlags.FileType | FileItemFlags.ContainerType;
		}

		#endregion

		#region Validation

		/// <summary>
		/// Validates the file tree and all invalidated children.<para/>
		/// For use when scanning is still underway, but the UI wants to be updated.
		/// </summary>
		public void BasicValidate() {
			ValidateImpl(false);
		}

		/// <summary>Fully validates the file item tree and prepares it for use.</summary>
		public void FullValidate() {
			ValidateImpl(true);
			FullValidateImpl();
		}

		/// <summary>Validates changes to the roots only. For use when space display has been changed.</summary>
		private void RootValidate() {
			ValidateImpl(true);
			if (IsEmpty)
				return;

			lock (children) {
				if (Type == FileItemType.Computer) {
					int count = children.Count;
					for (int i = 0; i < count; i++) {
						if (children[i] is RootItem root)
							root.RootValidate();
					}
				}
				children.Sort();
				children.Minimize();
			}
		}

		#endregion
		
		public long GetUsedSize() {
			long usedSize = 0;
			int count = children.Count;
			for (int i = 0; i < count; i++) {
				FileItemBase child = children[i];
				if (child.Type != FileItemType.FreeSpace && child.Type != FileItemType.Unknown)
					usedSize += child.Size;
			}
			return usedSize;
		}

		private void UpdateSpace(object sender = null, EventArgs e = null) {
			if (Type == FileItemType.Computer) {
				int count = children.Count;
				for (int i = 0; i < count; i++) {
					if (children[i] is RootItem root)
						root.UpdateSpace(sender, e);
				}
			}

			// ShowTotalSpace means only the absolute root will show space
			// !ShowTotalSpace means only the file root will show space
			// We do this check because absolute roots can also be file roots
			bool baseShow = (IsAbsoluteRootType && scanning.ShowTotalSpace) ||
							(IsFileRootType && !scanning.ShowTotalSpace);

			bool showFree = scanning.ShowFreeSpace && baseShow;
			bool showUnknown = scanning.ShowUnknown && baseShow;

			// Create some children so we can lock things down
			EnsureChildren(2);

			lock (children) {
				if (showFree)
					FreeSpace.UpdateSize();
				if (showUnknown)
					Unknown.UpdateSize(GetUsedSize());
				AddOrRemove(FreeSpace, showFree);
				AddOrRemove(Unknown, showUnknown);

				// If nothing was added or removed and children was created,
				// children was never set back to empty.
				EnsureEmptyChildren();

				Invalidate();
			}

			if (IsAbsoluteRootType)
				RootValidate();
		}

		#region Override Methods

		/// <summary>Refreshes the item. Returns true if it still exists.</summary>
		/// 
		/// <returns>True if the file still exists.</returns>
		public override bool Refresh() {
			DirectoryInfo info = new DirectoryInfo(FullName);
			// Remove all of our children first, they will be repopulated
			ClearItems();
			if (info.Exists && info.Attributes.HasFlag(FileAttributes.Directory)) {
				//CaseSensitive = DirectoryCaseSensitivity.IsCaseSensitive(info.FullName);
				Attributes = info.Attributes;
				Size = 0L;
				LastWriteTimeUtc = info.LastWriteTimeUtc;
				UpdateSpace();

				return RefreshFinal(true);
			}
			return RefreshFinal(false);
		}
		
		/// <summary>Checks if it still exists.</summary>
		/// 
		/// <returns>True if the file exists.</returns>
		public override sealed bool CheckExists() {
			if (Type == FileItemType.Computer)
				return true;

			if (Directory.Exists(FullName)) {
				return (Exists = true);
			}
			RecurseClearChildren();
			return (Exists = false);
		}

		#endregion

		#region IDisposable Implementation

		/// <summary>Disposes of the root node's hooked events.</summary>
		public void Dispose() {
			if (IsAbsoluteRootType)
				scanning.SpaceChanged -= UpdateSpace;
		}

		#endregion
	}
}
