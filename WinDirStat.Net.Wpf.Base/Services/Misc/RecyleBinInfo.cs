using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Services {
	/// <summary>Information on the contents of the Recycle Bin.</summary>
	public class RecycleBinInfo {
		/// <summary>Gets the owner of the Recycle Bin. May be null.</summary>
		public string Owner { get; }
		/// <summary>Gets the total number of items in the Recycle Bin.</summary>
		public long ItemCount { get; }
		/// <summary>Gets the total size of all items in the Recycle Bin.</summary>
		public long Size { get; }

		/// <summary>Constructs the <see cref="RecycleBinInfo"/>.</summary>
		/// 
		/// <param name="itemCount">The total number of items in the Recycle Bin.</param>
		/// <param name="size">The total size of items in the Recycle Bin.</param>
		public RecycleBinInfo(long itemCount, long size) {
			ItemCount = itemCount;
			Size = size;
		}

		/// <summary>Constructs the <see cref="RecycleBinInfo"/>.</summary>
		/// 
		/// <param name="owner">The owner of the Recycle Bin. Can be null.</param>
		/// <param name="itemCount">The total number of items in the Recycle Bin.</param>
		/// <param name="size">The total size of items in the Recycle Bin.</param>
		public RecycleBinInfo(string owner, long itemCount, long size) {
			Owner = owner;
			ItemCount = itemCount;
			Size = size;
		}
	}
}
