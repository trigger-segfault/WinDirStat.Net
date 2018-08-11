using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Data.Nodes {
	public class FreeSpaceNode : FileNode {

		private static readonly char[] FreeSpaceName = "<Free Space>".ToArray();

		private readonly string driveName;

		public FreeSpaceNode(string driveName, bool showFreeSpace) {
			this.driveName = driveName;
			cName = FreeSpaceName;
			attributes = FileAttributes.Normal;
			//creationTime = DateTime.MinValue;
			lastChangeTime = DateTime.MinValue;
			//lastAccessTime = DateTime.MinValue;

			size = GetAvailableFreeSpace();
			IsHidden = !showFreeSpace;
		}

		public string DriveName {
			get => driveName;
		}

		public override FileNodeType Type {
			get => FileNodeType.FreeSpace;
		}

		private long GetAvailableFreeSpace() {
			GetDiskFreeSpaceEx(driveName, out ulong freeSpace, out _, out _);
			return (long) freeSpace;
		}

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);
	}
}
