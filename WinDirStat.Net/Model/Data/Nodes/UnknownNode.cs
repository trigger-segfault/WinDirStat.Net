using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Utils.Native;

namespace WinDirStat.Net.Model.Data.Nodes {
	public class UnknownNode : FileNodeBase {
		private const string UnknownName = "<Unknown>";
		
		private readonly string drivePath;

		public string DrivePath {
			get => drivePath;
		}

		public UnknownNode(string drivePath, long usedSpace)
			: base(UnknownName, FileNodeType.Unknown, FileNodeFlags.None)
		{
			this.drivePath = drivePath;
			//creationTime = DateTime.MinValue;
			lastChangeTime = DateTime.MinValue;
			//lastAccessTime = DateTime.MinValue;
			size = CalculateSize(usedSpace);
		}
		
		private long CalculateSize(long usedSpace) {
			Win32.GetDiskFreeSpaceEx(drivePath, out ulong freeSpace, out ulong totalSize, out _);
			long estimatedUsedSpace = Math.Max(0, (long) totalSize - (long) freeSpace);
			return (estimatedUsedSpace - usedSpace);
		}

		internal void UpdateSize(long usedSpace) {
			size = CalculateSize(usedSpace);
		}
	}
}
