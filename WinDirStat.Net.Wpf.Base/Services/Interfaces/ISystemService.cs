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
	public interface ISystemService {
		
		/// <summary>Opens the explorer to display the Computer.</summary>
		void ExploreComputer();
		/// <summary>Opens a folder in explorer.</summary>
		void ExploreFolder(string folderPath);
		/// <summary>Opens and selects a file in explorer.</summary>
		void ExploreFile(string filePath);
		/// <summary>Opens the properties window of a file.</summary>
		void OpenProperties(string filePath);
		/// <summary>Opens the command prompt or terminal.</summary>
		void OpenCommandLine(string directory);
		/// <summary>Opens PowerShell.</summary>
		void OpenPowerShell(string directory);
		
		/// <summary>Recycles the specified files.</summary>
		bool RecycleFiles(params string[] files);
		/// <summary>Recycles the specified directories.</summary>
		bool RecycleDirectories(params string[] directories);
		/// <summary>Empties the Recycle Bin.</summary>
		bool EmptyRecycleBin(string owner = null);
		/// <summary>Gets the Recycle Bin info.</summary>
		RecycleBinInfo GetRecycleBinInfo(string owner = null);
	}
}
