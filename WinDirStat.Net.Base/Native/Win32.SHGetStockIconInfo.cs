using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Native {
	partial class Win32 {
		/// <summary>
		/// Used by <see cref="SHGetStockIconInfo"/> to identify which stock system icon to retrieve.
		/// </summary>
		public enum SHStockIconID : uint {
			DocNoAssoc = 0,
			DocAssoc = 1,
			Application = 2,
			Folder = 3,
			FolderOpen = 4,
			Drive525 = 5,
			Drive35 = 6,
			DriveRemove = 7,
			DriveFixed = 8,
			DriveNet = 9,
			DriveNetDisabled = 10,
			DriveCD = 11,
			DriveRAM = 12,
			World = 13,
			Server = 15,
			Printer = 16,
			MyNetwork = 17,
			Find = 22,
			Help = 23,
			Share = 28,
			Link = 29,
			SlowFile = 30,
			Recycler = 31,
			RecyclerFull = 32,
			MediaCDAudio = 40,
			Lock = 47,
			AutoList = 49,
			PrinterNet = 50,
			ServerShare = 51,
			PrinterFax = 52,
			PrenterFaxNet = 53,
			PrinterFile = 54,
			Stack = 55,
			MediaSVCD = 56,
			StuffedFolder = 57,
			DriveUnknown = 58,
			DriveDVD = 59,
			MediaDVD = 60,
			MediaDVDRAM = 61,
			MediaDVDRW = 62,
			MediaDVDR = 63,
			MediaDVDROM = 64,
			MediaCDAudioPlus = 65,
			MediaCDRW = 66,
			MediaCDR = 67,
			MediaCDBurn = 68,
			MediaCDBlank = 69,
			MediaCDROM = 70,
			AudioFiles = 71,
			ImageFiles = 72,
			VideoFiles = 73,
			MixedFiles = 74,
			FolderBack = 75,
			FolderFront = 76,
			Shield = 77,
			Warning = 78,
			Info = 79,
			Error = 80,
			Key = 81,
			Software = 82,
			Rename = 83,
			Delete = 84,
			MediaAudioDVD = 85,
			MediaMovieDVD = 86,
			MediaEnhancedCD = 87,
			MediaEnhancedDVD = 88,
			MediaHDDVD = 89,
			MediaBluray = 90,
			MediaVCD = 91,
			MediaDVDPlusR = 92,
			MediaDVDPlusRW = 93,
			DesktopPC = 94,
			MobilePC = 95,
			Users = 96,
			MediaSmartMedia = 97,
			MediaCompactFlash = 98,
			DeviceCellPhone = 99,
			DeviceCamera = 100,
			DeviceVideoCamera = 101,
			DeviceAudioPlayer = 102,
			NetworkConnect = 103,
			Internet = 104,
			ZipFile = 105,
			Settings = 106,
			DriveHDDVD = 132,
			DriveBD = 133,
			MediaHDDVDROM = 134,
			MediaHDDVDR = 135,
			MediaHDDVDRAM = 136,
			MediaBDROM = 137,
			MediaBDR = 138,
			MediaBDRE = 139,
			ClusteredDrive = 140,
			MaxIcons = 175,
		}

		[Flags]
		public enum SHStockIconFlags : uint {
			None = 0,

			IconLocation = 0,
			LargeIcon = 0x000000000,
			SmallIcon = 0x000000001,
			ShellIconSize = 0x000000004,
			Icon = 0x000000100,
			SysIconIndex = 0x000004000,
			LinkOverlay = 0x000008000,
			Selected = 0x000010000,
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct SHStockIconInfo {
			/// <summary>The marshaled size of the struct.</summary>
			public static readonly int CBSize = Marshal.SizeOf<SHStockIconInfo>();

			public int cbSize;
			public IntPtr hIcon;
			public int iSysIconIndex;
			public int iIcon;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szPath;
		}

		[DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool SHGetStockIconInfo(
			[MarshalAs(UnmanagedType.U4)] SHStockIconID siid,
			[MarshalAs(UnmanagedType.U4)] SHStockIconFlags uFlags,
			ref SHStockIconInfo psii);
	}
}
