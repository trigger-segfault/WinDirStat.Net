using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model.Drives {
	/// <summary>An item containing basic information on a drive.</summary>
	[Serializable]
	public class DriveItem : IComparable<DriveItem>, IComparable {

		#region Fields

		/// <summary>Gets the name/path of the drive.</summary>
		public string Name { get; }
		/// <summary>Gets the total size of the drive.</summary>
		public long TotalSize { get; }
		/// <summary>Gets the freespace on the drive in bytes.</summary>
		public long FreeSpace { get; }
		/// <summary>Gets the type of the drive.</summary>
		public DriveType DriveType { get; }
		/// <summary>Gets the partition format of the drive.</summary>
		public string DriveFormat { get; }

		#endregion

		#region Constructors

		/// <summary>Constructs a <see cref="DriveItem"/> using the <see cref="DriveInfo"/>.</summary>
		/// 
		/// <param name="info">The information about the drive.</param>
		public DriveItem(DriveInfo info) {
			Name = info.Name;
			TotalSize = info.TotalSize;
			FreeSpace = info.TotalFreeSpace;
			DriveType = info.DriveType;
			DriveFormat = info.DriveFormat;
		}

		#endregion

		#region Properties

		/// <summary>Gets the used space on the drive in bytes.</summary>
		public long UsedSpace => Math.Max(0L, TotalSize - FreeSpace);

		/// <summary>Gets the used percentage of the drive.</summary>
		public double Percent => (double) UsedSpace / TotalSize;

		#endregion

		#region IComparable Implementation

		/// <summary>Compares this drive to another based on name in ascending order.</summary>
		/// 
		/// <param name="other">The other drive to compare to.</param>
		/// <returns>The comparison result.</returns>
		public int CompareTo(DriveItem other) {
			int diff = other.TotalSize.CompareTo(TotalSize);
			if (diff == 0)
				return string.Compare(Name, other.Name, true);
			return diff;
		}

		/// <summary>Compares this drive to another based on name in ascending order.</summary>
		/// 
		/// <param name="obj">The other drive to compare to.</param>
		/// <returns>The comparison result.</returns>
		int IComparable.CompareTo(object obj) {
			return CompareTo((DriveItem) obj);
		}

		#endregion

		#region ToString/DebuggerDisplay

		/// <summary>Gets the string representation of this item.</summary>
		public override sealed string ToString() => Name;

		/// <summary>Gets the string used to represent the file in the debugger.</summary>
		private string DebuggerDisplay => Name;

		#endregion
	}
}
