using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Native {
	partial class Win32 {

		/// <summary>Information about the Recycle Bin returned by <see cref="SHQueryRecycleBin"/>.</summary>
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct SHRecycleBinInfo {
			public static readonly int CBSize = Marshal.SizeOf<SHRecycleBinInfo>();

			/// <summary>The size of this structure.</summary>
			public int cbSize;
			/// <summary>The size of items in the Recycle Bin.</summary>
			public long i64Size;
			/// <summary>The number of items in the Recycle Bin.</summary>
			public long i64NumItems;
		}

		/// <summary>Flags for <see cref="SHEmptyRecycleBin"/>.</summary>
		[Flags]
		public enum SHEmptyRecycleBinFlags : uint {
			/// <summary>No flags.</summary>
			None = 0,
			/// <summary>No dialog box confirming the deletion of the objects will be displayed.</summary>
			NoConfirmation = 0x00000001,
			/// <summary>No dialog box indicating the progress will be displayed.</summary>
			NoProgressUI = 0x00000002,
			/// <summary>No sound will be played when the operation is complete.</summary>
			NoSound = 0x00000004,
		}

		/// <summary>
		/// Retrieves the size of the Recycle Bin and the number of items in it, for a specified drive.
		/// </summary>
		/// 
		/// <param name="pszRootPath">
		/// The address of a null-terminated string of maximum length MAX_PATH to contain the path of the
		/// root drive on which the Recycle Bin is located. This parameter can contain the address of a
		/// string formatted with the drive, folder, and subfolder names (C:\Windows\System...).
		/// </param>
		/// <param name="pSHQueryRBInfo">
		/// The reference to a <see cref="SHRecycleBinInfo"/> structure that receives the Recycle Bin
		/// information. The cbSize member of the structure must be set to the size of the structure before
		/// calling this API.
		/// </param>
		/// <returns>False if the function succeeds.</returns>
		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern bool SHQueryRecycleBin(
			string pszRootPath,
			ref SHRecycleBinInfo pSHQueryRBInfo);

		/// <summary>Empties the Recycle Bin on the specified drive.</summary>
		/// 
		/// <param name="hWnd">
		/// A handle to the parent window of any dialog boxes that might be displayed during the operation.
		/// This parameter can be null.
		/// </param>
		/// <param name="pszRootPath">
		/// The address of a null-terminated string of maximum length MAX_PATH that contains the path of the
		/// root drive on which the Recycle Bin is located. This parameter can contain the address of a
		/// string formatted with the drive, folder, and subfolder names, for example c:\windows\system. It
		/// can also contain an empty string or null. If this value is an empty string or null, all Recycle
		/// Bins on all drives will be emptied.
		/// </param>
		/// <param name="dwFlags"></param>
		/// <returns></returns>
		[DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool SHEmptyRecycleBin(
			IntPtr hWnd,
			string pszRootPath,
			[MarshalAs(UnmanagedType.U4)] SHEmptyRecycleBinFlags dwFlags);


	}
}
