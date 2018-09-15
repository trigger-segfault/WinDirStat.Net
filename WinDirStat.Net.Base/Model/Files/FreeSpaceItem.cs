using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model.Files {
	/// <summary>The file tree item that displays the amount of unused space in the drive.</summary>
	[Serializable]
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public sealed class FreeSpaceItem : FileItemBase {
		
		#region Fields

		/// <summary>The <see cref="RootItem"/> containing this space.</summary>
		private readonly RootItem rootParent;

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="FreeSpaceItem"/>.</summary>
		/// 
		/// <param name="rootParent">The <see cref="RootItem"/> containing this space.</param>
		internal FreeSpaceItem(RootItem rootParent)
			: base(StringConstants.FreeSpaceName, FileItemType.FreeSpace, FileItemFlags.None)
		{
			this.rootParent = rootParent;
			LastWriteTimeUtc = DateTime.MinValue;
			UpdateSize();
		}

		#endregion

		#region Helper Properties

		/// <summary>Gets the root path of <see cref="rootParent"/>.</summary>
		private string VolumePath => rootParent.RootPath;

		#endregion

		#region Updating

		/// <summary>Updates the size of the free space.</summary>
		internal void UpdateSize() {
			if (rootParent.Type == FileItemType.Computer) {
				Size = 0;
				List<FileItemBase> children = rootParent.Children;
				lock (children) {
					int count = children.Count;
					for (int i = 0; i < count; i++) {
						if (children[i] is RootItem root && root.FreeSpace != null) {
							root.FreeSpace.UpdateSize();
							Size += root.FreeSpace.Size;
						}
					}
				}
			}
			else if (rootParent.Type == FileItemType.Volume) {
				try {
					DriveInfo driveInfo = new DriveInfo(VolumePath);
					Size = driveInfo.TotalFreeSpace;
				}
				catch {
					Size = 0;
				}
			}
		}

		#endregion

		#region ToString/DebuggerDisplay

		/// <summary>Gets the string representation of this item.</summary>
		public override string ToString() => "<Free Space>";

		/// <summary>Gets the string used to represent the file in the debugger.</summary>
		private string DebuggerDisplay => "<Free Space>";

		#endregion
	}
}
