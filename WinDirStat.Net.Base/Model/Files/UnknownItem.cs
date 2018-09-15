using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model.Files {
	/// <summary>The file tree item that displays the amount of unidentified space in the drive.</summary>
	[Serializable]
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public sealed class UnknownItem : FileItemBase {
		
		#region Fields

		/// <summary>The <see cref="RootItem"/> containing this space.</summary>
		private readonly RootItem rootParent;

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="UnknownItem"/>.</summary>
		/// 
		/// <param name="rootParent">The <see cref="RootItem"/> containing this space.</param>
		internal UnknownItem(RootItem rootParent)
			: base(StringConstants.UnknownName, FileItemType.Unknown, FileItemFlags.None)
		{
			this.rootParent = rootParent;
			LastWriteTimeUtc = DateTime.MinValue;
			UpdateSize(0);
		}

		#endregion

		#region Helper Properties

		/// <summary>Gets the root path of <see cref="rootParent"/>.</summary>
		private string VolumePath => rootParent.RootPath;

		#endregion

		#region Updating

		/// <summary>
		/// Updates the size of the unknown space based on the amount of used space.
		/// </summary>
		/// <param name="usedSpace">The space of the root item excluding free and unknown space.</param>
		internal void UpdateSize(long usedSpace) {
			if (rootParent.Type == FileItemType.Computer) {
				Size = 0;
				List<FileItemBase> children = rootParent.Children;
				lock (children) {
					int count = children.Count;
					for (int i = 0; i < count; i++) {
						if (children[i] is RootItem root && root.Unknown != null) {
							root.Unknown.UpdateSize(root.GetUsedSize());
							Size += root.Unknown.Size;
						}
					}
				}
			}
			else if (rootParent.Type == FileItemType.Volume) {
				try {
					DriveInfo driveInfo = new DriveInfo(VolumePath);
					Size = driveInfo.TotalSize - driveInfo.TotalFreeSpace - usedSpace;
					// In the rare case that our used space is larger than the actual used space
					if (Size < 0)
						Size = 0;
				}
				catch {
					Size = 0;
				}
			}
		}

		#endregion
		
		#region ToString/DebuggerDisplay

		/// <summary>Gets the string representation of this item.</summary>
		public override string ToString() => "<Unknown>";

		/// <summary>Gets the string used to represent the file in the debugger.</summary>
		private string DebuggerDisplay => "<Unknown>";

		#endregion
	}
}
