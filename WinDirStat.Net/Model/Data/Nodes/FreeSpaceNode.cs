using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Utils.Native;

namespace WinDirStat.Net.Model.Data.Nodes {
	public class FreeSpaceNode : FileNodeBase {
		private const string FreeSpaceName = "<Free Space>";

		private readonly string drivePath;

		public string DrivePath {
			get => drivePath;
		}

		public FreeSpaceNode(string drivePath)
			: base(FreeSpaceName, FileNodeType.FreeSpace, FileNodeFlags.None)
		{
			this.drivePath = drivePath;
			//creationTime = DateTime.MinValue;
			lastChangeTime = DateTime.MinValue;
			//lastAccessTime = DateTime.MinValue;
			size = CalculateSize();
		}
		private long CalculateSize() {
			Win32.GetDiskFreeSpaceEx(drivePath, out ulong freeSpace, out _, out _);
			return (long) freeSpace;
		}

		internal void UpdateSize() {
			size = CalculateSize();
		}
	}
}
