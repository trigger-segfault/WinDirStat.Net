using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using static WinDirStat.Net.Native.Win32;

namespace WinDirStat.Net.Wpf.Utils {
	public static class WindowExtensions {

		public static void ShowMaximizeMinimize(this Window window, bool maximize, bool minimize) {
			IntPtr hwnd = new WindowInteropHelper(window).Handle;
			WindowStyles styles = (WindowStyles) GetWindowLong(hwnd, WindowLongs.Style);
			if (maximize)
				styles |= WindowStyles.MaximizeBox;
			else
				styles &= ~WindowStyles.MaximizeBox;
			if (minimize)
				styles |= WindowStyles.MinimizeBox;
			else
				styles &= ~WindowStyles.MinimizeBox;
			SetWindowLong(hwnd, WindowLongs.Style, (uint) styles);
		}

		public static void ShowMaximize(this Window window, bool enabled) {
			IntPtr hwnd = new WindowInteropHelper(window).Handle;
			WindowStyles styles = (WindowStyles) GetWindowLong(hwnd, WindowLongs.Style);
			if (enabled)
				styles |= WindowStyles.MaximizeBox;
			else
				styles &= ~WindowStyles.MaximizeBox;
			SetWindowLong(hwnd, WindowLongs.Style, (uint) styles);
		}

		public static void ShowMinimize(this Window window, bool enabled) {
			IntPtr hwnd = new WindowInteropHelper(window).Handle;
			WindowStyles styles = (WindowStyles) GetWindowLong(hwnd, WindowLongs.Style);
			if (enabled)
				styles |= WindowStyles.MinimizeBox;
			else
				styles &= ~WindowStyles.MinimizeBox;
			SetWindowLong(hwnd, WindowLongs.Style, (uint) styles);
		}
	}
}
