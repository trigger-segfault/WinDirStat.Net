using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Native {
	partial class Win32 {

		/// <summary>File Operation Function Type for SHFileOperation.</summary>
		public enum FileOperationFunc : uint {
			/// <summary>Move the objects.</summary>
			Move = 0x0001,
			/// <summary>Copy the objects.</summary>
			Copy = 0x0002,
			/// <summary>Delete (or recycle) the objects.</summary>
			Delete = 0x0003,
			/// <summary>Rename the objects.</summary>
			Rename = 0x0004,
		}

		/// <summary>Possible flags for the SHFileOperation method.</summary>
		[Flags]
		public enum FileOperationFlags : ushort {
			/// <summary>No flags.</summary>
			None = 0,

			/// <summary>Do not show a dialog during the process</summary>
			Silent = 0x0004,
			/// <summary>Do not ask the user to confirm selection.</summary>
			NoConfirmation = 0x0010,
			/// <summary>
			/// Delete the file to the recycle bin. (Required flag to send a file to the bin)
			/// </summary>
			AllowUndo = 0x0040,
			/// <summary>Do not show the names of the files or folders that are being recycled.</summary>
			SimpleProgress = 0x0100,
			/// <summary>Surpress errors, if any occur during the process.</summary>
			NoErrorUI = 0x0400,
			/// <summary>
			/// Warn if files are too big to fit in the recycle bin and will need to be deleted completely.
			/// </summary>
			WarnFilesTooBigForRecycleBin = 0x4000,
		}

		/// <summary>SHFILEOPSTRUCT for SHFileOperation from COM.</summary>
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct SHFileOperationStruct {
			public IntPtr hwnd;
			[MarshalAs(UnmanagedType.U4)]
			public FileOperationFunc wFunc;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string pFrom;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string pTo;
			[MarshalAs(UnmanagedType.U2)]
			public FileOperationFlags fFlags;
			[MarshalAs(UnmanagedType.Bool)]
			public bool fAnyOperationsAborted;
			public IntPtr hNameMappings;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpszProgressTitle;
		}

		/// <summary>
		/// Copies, moves, renames, or deletes a file system object. This function has been replaced in
		/// Windows Vista by IFileOperation.
		/// </summary>
		/// 
		/// <param name="FileOp">
		/// A reference to an <see cref="SHFileOperationStruct"/> that contains information this function
		/// needs to carry out the specified operation. This parameter must contain a valid value that is not
		/// nulll. You are responsible for validating the value. If you do not validate it, you will
		/// experience unexpected results.
		/// </param>
		/// <returns>False on success.</returns>
		[DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool SHFileOperation(ref SHFileOperationStruct FileOp);
	}
}
