using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Native {
	partial class Win32 {

		[Flags]
		public enum ShellExecuteMaskFlags : uint {
			Default = 0x00000000,
			ClassName = 0x00000001,
			ClassKey = 0x00000003,
			IDList = 0x00000004,
			InvokeIDList = 0x0000000c,   // Note SEE_MASK_INVOKEIDLIST(0xC) implies SEE_MASK_IDLIST(0x04)
			HotKey = 0x00000020,
			NoCloseProcess = 0x00000040,
			ConnectNetDRV = 0x00000080,
			NoAsync = 0x00000100,
			FlagDDEWait = NoAsync,
			DOEnvSubset = 0x00000200,
			FlagNoUI = 0x00000400,
			Unicode = 0x00004000,
			NoConsole = 0x00008000,
			AsyncOK = 0x00100000,
			HMonitor = 0x00200000,
			NoZoneChecks = 0x00800000,
			NoQueryClassStore = 0x01000000,
			WaitForInputIdle = 0x02000000,
			MaskFlagLogUsage = 0x04000000,
		}

		public enum ShowCommands : int {
			Hide = 0,
			ShowNormal = 1,
			Normal = 1,
			ShowMinimized = 2,
			ShowMaximized = 3,
			Maximize = 3,
			ShowNoActive = 4,
			Show = 5,
			Minimize = 6,
			ShowMinNoActive = 7,
			ShowNA = 8,
			Restore = 9,
			ShowDefault = 10,
			ForceMinimize = 11,
			Max = 11
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct ShellExecuteInfo {
			public static readonly int CBSize = Marshal.SizeOf<ShellExecuteInfo>();

			public int cbSize;
			[MarshalAs(UnmanagedType.U4)]
			public ShellExecuteMaskFlags fMask;
			public IntPtr hwnd;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpVerb;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpFile;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpParameters;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpDirectory;
			[MarshalAs(UnmanagedType.I4)]
			public ShowCommands nShow;
			public IntPtr hInstApp;
			public IntPtr lpIDList;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpClass;
			public IntPtr hkeyClass;
			public uint dwHotKey;
			public IntPtr hIcon;
			public IntPtr hProcess;
		}

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern bool ShellExecuteEx(ref ShellExecuteInfo lpExecInfo);
	}
}
