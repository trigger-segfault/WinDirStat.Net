using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WinDirStat.Net.Native {
	partial class Win32 {

		public enum WindowLongs : int {
			ExStyle = -20,
			HInstance = -6,
			HwndParent = -8,
			ID = -12,
			Style = -16,
			UserData = -21,
			WndProc = -4,
			User = 0x8,
			MsgResult = 0x0,
			DlgProc = 0x4
		}

		public const int GWL_STYLE = -16;

		[Flags]
		public enum WindowStyles : uint {
			None = 0,
			VScroll = 0x00200000,
			HScroll = 0x00100000,
			Visible = 0x10000000,
			Border = 0x00800000,
			Overlapped = 0x00000000,
			Tiled = 0x00000000,
			Maximize = 0x01000000,
			Minimize = 0x20000000,
			Iconic = 0x20000000,
			ThickFrame = 0x00040000,
			SizeBox = 0x00040000,
			Popup = 0x80000000,
			PopupWindow = Popup | Border | SysMenu,
			DlgFrame = 0x00400000,
			Caption = 0x00C00000,
			ChildWindow = 0x40000000,
			SysMenu = 0x00080000,
			MinimizeBox = 0x00020000,
			MaximizeBox = 0x00010000,
			OverlappedWindow = Overlapped | Caption | SysMenu | ThickFrame | MinimizeBox | MaximizeBox,
			Child = 0x40000000,
			ClipChildren = 0x02000000,
			ClipSiblings = 0x04000000,
			Disabled = 0x08000000,
			Group = 0x00020000,
			Tabstop = 0x00010000,
			TiledWindow = Tiled | Caption | SysMenu | ThickFrame | MinimizeBox | MaximizeBox,
		}

		[DllImport("user32.dll")]
		public static extern uint GetWindowLong(
			IntPtr hWnd,
			[MarshalAs(UnmanagedType.U4)] WindowLongs nIndex);

		[DllImport("user32.dll")]
		public static extern uint SetWindowLong(
			IntPtr hWnd,
			[MarshalAs(UnmanagedType.U4)] WindowLongs nIndex,
			uint dwNewLong);
	}
}
