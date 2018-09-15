using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Native {
	partial class Win32 {
		[DllImport("comctl32.dll", SetLastError = true)]
		public static extern int ImageList_GetImageCount(
			IntPtr hImageList);

		[DllImport("comctl32.dll", SetLastError = true)]
		public static extern IntPtr ImageList_Duplicate(
			IntPtr hImageList);

		[DllImport("comctl32.dll", SetLastError = true)]
		public static extern IntPtr ImageList_Destroy(
			IntPtr hImageList);

		[DllImport("comctl32.dll", SetLastError = true)]
		public static extern int ImageList_Add(
			IntPtr hImageList,
			IntPtr image,
			IntPtr mask);

		[DllImport("comctl32.dll", SetLastError = true)]
		public static extern IntPtr ImageList_ExtractIcon(
			IntPtr hInstance,
			IntPtr hImageList,
			int i);

		[DllImport("comctl32.dll", SetLastError = true)]
		public static extern IntPtr ImageList_GetIcon(
			IntPtr hImageList,
			int i,
			[MarshalAs(UnmanagedType.U4)] ImageListDrawFlags flags);

		[Flags]
		public enum ImageListDrawFlags : uint {
			Normal = 0x00000000,
			Transparent = 0x00000001,
			Blend25 = 0x00000002,
			Focus = 0x00000002,
			Blend50 = 0x00000004,
			Selected = 0x00000004,
			Blend = 0x00000004,
			Mask = 0x00000010,
			Image = 0x00000020,
			Rop = 0x00000040,
			OverlayMask = 0x00000F00,
			PreserveAlpha = 0x00001000,
			Scale = 0x00002000,
			DpiScale = 0x00004000,
			Async = 0x00008000,
		}
	}
}
