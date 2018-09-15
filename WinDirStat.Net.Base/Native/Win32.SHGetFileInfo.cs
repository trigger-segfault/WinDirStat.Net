using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Native {
	partial class Win32 {

		/// <summary>Contains information about a file object.</summary>
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct SHFileInfo {
			/// <summary>The marshaled size of the struct.</summary>
			public static readonly int CBSize = Marshal.SizeOf<SHFileInfo>();

			/// <summary>
			/// A handle to the icon that represents the file. You are responsible for destroying this handle
			/// with DestroyIcon when you no longer need it.
			/// </summary>
			public IntPtr hIcon;
			/// <summary>The index of the icon image within the system image list.</summary>
			public int iIcon;
			/// <summary>
			/// An array of values that indicates the attributes of the file object. For information about
			/// these values, see the IShellFolder::GetAttributesOf method.
			/// </summary>
			public uint dwAttributes;
			/// <summary>
			/// A string that contains the name of the file as it appears in the Windows Shell, or the path
			/// and file name of the file that contains the icon representing the file.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			/// <summary>A string that describes the type of file.</summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		}

		/// <summary>
		/// The flags that specify the file information to retrieve for <see cref="SHGetFileInfo"/>.
		/// </summary>
		[Flags]
		public enum SHFileInfoFlags : uint {
			None = 0,
			LargeIcon = 0x000000000,

			SmallIcon = 0x000000001,
			OpenIcon = 0x000000002,
			ShellIconSize = 0x000000004,
			PIDL = 0x000000008,

			UseFileAttributes = 0x000000010,
			AddOverlays = 0x000000020,
			OverlayIndex = 0x000000040,

			Icon = 0x000000100,
			DisplayName = 0x000000200,
			TypeName = 0x000000400,
			Attributes = 0x000000800,

			IconLocation = 0x000001000,
			ExeType = 0x000002000,
			SysIconIndex = 0x000004000,
			LinkOverlay = 0x000008000,

			Selected = 0x000010000,
			AttrSpecified = 0x000020000,
		}

		/// <summary>
		/// Retrieves information about an object in the file system, such as a file, folder, directory, or
		/// drive root.
		/// </summary>
		/// 
		/// <param name="pszPath">
		/// A string of maximum length MAX_PATH that contains the path and file name. Both absolute and
		/// relative paths are valid.
		/// </param>
		/// <param name="dwFileAttributes">
		/// A combination of one or more file attribute flags. If <paramref name="uFlags"/> does not include
		/// the <see cref="SHFileInfoFlags.UseFileAttributes"/> flag, this parameter is ignored.
		/// </param>
		/// <param name="psfi">A <see cref="SHFileInfo"/> structure to receive the file information.</param>
		/// <param name="cbFileInfo">Use <see cref="SHFileInfo.CBSize"/> here.</param>
		/// <param name="uFlags">
		/// The flags that specify the file information to retrieve. This parameter can be a combination of
		/// the following values.
		/// </param>
		/// <returns>Returns a value whose meaning depends on the uFlags parameter.</returns>
		[DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr SHGetFileInfo(
			string pszPath,
			[MarshalAs(UnmanagedType.U4)] FileAttributes dwFileAttributes,
			ref SHFileInfo psfi,
			int cbFileInfo,
			[MarshalAs(UnmanagedType.U4)] SHFileInfoFlags uFlags);

		[DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr SHGetFileInfo(
			IntPtr pszPath,
			[MarshalAs(UnmanagedType.U4)] FileAttributes dwFileAttributes,
			ref SHFileInfo psfi,
			int cbFileInfo,
			[MarshalAs(UnmanagedType.U4)] SHFileInfoFlags uFlags);

		[DllImport("shell32.dll", SetLastError = true)]
		public static extern bool SHGetSpecialFolderLocation(
			IntPtr hwnd,
			[MarshalAs(UnmanagedType.I4)] Environment.SpecialFolder csidl,
			ref IntPtr ppidl);
	}
}
