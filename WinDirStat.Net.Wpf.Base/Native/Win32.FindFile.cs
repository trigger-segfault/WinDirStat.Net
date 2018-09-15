using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace WinDirStat.Net.Native {
	partial class Win32 {

		/// <summary>
		/// Searches a directory for a file or subdirectory with a name and attributes that match those
		/// specified.
		/// </summary>
		/// 
		/// <param name="lpFileName">
		/// The directory or path, and the file name. The file name can include wildcard characters, for
		/// example, an asterisk (*) or a question mark (?).
		/// </param>
		/// <param name="fInfoLevelId">The information level of the returned data.</param>
		/// <param name="lpFindFileData">
		/// A reference to the <see cref="Win32FindData"/> that receives the file data.
		/// </param>
		/// <param name="fSearchOp">
		/// The type of filtering to perform that is different from wildcard matching.
		/// </param>
		/// <param name="lpSearchFilter">
		/// A pointer to the search criteria if the specified fSearchOp needs structured search information.
		/// </param>
		/// <param name="dwAdditionalFlags">Specifies additional flags that control the search.</param>
		/// <returns>
		/// If the function succeeds, the return value is a search handle used in a subsequent call to
		/// <see cref="FindNextFile"/> or <see cref="FindClose"/>, and the <paramref name="lpFindFileData"/>
		/// parameter contains information about the first file or directory found.
		/// <para/>
		/// If the function fails or fails to locate files from the search string in the lpFileName
		/// parameter, the return value is <see cref="InvalidHandle"/>.
		/// </returns>
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr FindFirstFileEx(
			string lpFileName,
			FindExInfoLevels fInfoLevelId,
			out Win32FindData lpFindFileData,
			FindExSearchOps fSearchOp,
			IntPtr lpSearchFilter,
			[MarshalAs(UnmanagedType.U4)] FindExFlags dwAdditionalFlags);

		/// <summary>
		/// Continues a file search from a previous call to the <see cref="FindFirstFileEx"/> function.
		/// </summary>
		/// 
		/// <param name="hFindFile">
		/// The search handle returned by a previous call to the <see cref="FindFirstFileEx"/> function.
		/// </param>
		/// <param name="lpFindFileData">
		/// A pointer to the <see cref="Win32FindData"/> structure that receives information about the found
		/// file or subdirectory.
		/// </param>
		/// <returns>
		/// If the function succeeds, the return value is true and the <paramref name="lpFindFileData"/>
		/// parameter contains information about the next file or directory found.
		/// </returns>
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool FindNextFile(
			IntPtr hFindFile,
			out Win32FindData lpFindFileData);

		/// <summary>
		/// Closes a file search handle opened by the <see cref="FindFirstFileEx"/> function.
		/// </summary>
		/// 
		/// <param name="hFindFile">The file search handle.</param>
		/// <returns>If the function succeeds, the return value is true.</returns>
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool FindClose(
			IntPtr hFindFile);

		/// <summary>
		/// Contains information about the file that is found by the <see cref="FindFirstFileEx"/>, or <see
		/// cref="FindNextFile"/> function.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct Win32FindData {
			/// <summary>The file attributes of a file.</summary>
			[MarshalAs(UnmanagedType.U4)]
			public FileAttributes dwFileAttributes;
			/// <summary>
			/// A <see cref="FILETIME"/> structure that specifies when a file or directory was created.
			/// </summary>
			public FILETIME ftCreationTime;
			/// <summary>
			/// For a file, the structure specifies when the file was last read from, written to, or for
			/// executable files, run.
			/// <para/>
			/// For a directory, the structure specifies when the directory is created. If the underlying
			/// file system does not support last access time, this member is zero.
			/// <para/>
			/// On the FAT file system, the specified date for both files and directories is correct, but the
			/// time of day is always set to midnight.
			/// </summary>
			public FILETIME ftLastAccessTime;
			/// <summary>
			/// For a file, the structure specifies when the file was last written to, truncated, or
			/// overwritten. The date and time are not updated when file attributes or security descriptors
			/// are changed.
			/// </summary>
			public FILETIME ftLastWriteTime;
			/// <summary>The high-order <see cref="uint"/> value of the file size, in bytes.</summary>
			public int nFileSizeHigh;
			/// <summary>The low-order <see cref="uint"/> value of the file size, in bytes.</summary>
			public uint nFileSizeLow;
			/// <summary>Reserved for future use.</summary>
			public uint dwReserved0;
			/// <summary>Reserved for future use.</summary>
			public uint dwReserved1;
			/// <summary>The name of the file.</summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string cFileName;
			//[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
			//public string cAlternateFileName;

			/// <summary>True if the file is a directory.</summary>
			public bool IsDirectory {
				get => (dwFileAttributes.HasFlag(FileAttributes.Directory));
			}
			/// <summary>True if the file is a symbolic link.</summary>
			public bool IsSymbolicLink {
				get => (dwFileAttributes.HasFlag(FileAttributes.ReparsePoint));
			}
			/// <summary>True if the file is a relative directory and should be ignored.</summary>
			public bool IsRelativeDirectory {
				get => (cFileName == "." || cFileName == "..");
			}
			/// <summary>The full size of the file.</summary>
			public long Size {
				get => ((long) nFileSizeHigh << 32) | nFileSizeLow;
			}

			/// <summary>The UTC creation time of the file.</summary>
			public DateTime CreationTimeUtc {
				get => ftCreationTime.ToDateTimeUtc();
			}
			/// <summary>The local creation time of the file.</summary>
			public DateTime CreationTime {
				get => ftCreationTime.ToDateTime();
			}
			/// <summary>The UTC last access time of the file.</summary>
			public DateTime LastAccessTimeUtc {
				get => ftLastAccessTime.ToDateTimeUtc();
			}
			/// <summary>The local last access time of the file.</summary>
			public DateTime LastAccessTime {
				get => ftLastAccessTime.ToDateTime();
			}
			/// <summary>The UTC last write time of the file.</summary>
			public DateTime LastWriteTimeUtc {
				get => ftLastWriteTime.ToDateTimeUtc();
			}
			/// <summary>The local last write time of the file.</summary>
			public DateTime LastWriteTime {
				get => ftLastWriteTime.ToDateTime();
			}
		}

		public enum FindExInfoLevels {
			/// <summary>
			/// The FindFirstFileEx function retrieves a standard set of attribute information. The data is
			/// returned in a <see cref="Win32FindData"/> structure.
			/// </summary>
			Standard,
			/// <summary>
			/// The FindFirstFileEx function does not query the short file name, improving overall
			/// enumeration speed.The data is returned in a WIN32_FIND_DATA structure, and the
			/// cAlternateFileName member is always a NULL string. Windows Server 2008, Windows Vista,
			/// Windows Server 2003 and Windows XP:  This value is not supported until Windows Server 2008 R2
			/// and Windows 7.
			/// </summary>
			Basic,
			[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
			MaxInfoLevel,
		}
		public enum FindExSearchOps {
			/// <summary>
			/// The search for a file that matches a specified file name. The lpSearchFilter parameter of
			/// <see cref="FindFirstFileEx"/> must be NULL when this search operation is used.
			/// </summary>
			NameMatch,
			/// <summary>
			/// This is an advisory flag. If the file system supports directory filtering, the function
			/// searches for a file that matches the specified name and is also a directory. If the file
			/// system does not support directory filtering, this flag is silently ignored. The
			/// lpSearchFilter parameter of the FindFirstFileEx function must be NULL when this search value
			/// is used. If directory filtering is desired, this flag can be used on all file systems, but
			/// because it is an advisory flag and only affects file systems that support it, the application
			/// must examine the file attribute data stored in the lpFindFileData parameter of the <see cref=
			/// "FindFirstFileEx"/> function to determine whether the function has returned a handle to a
			/// directory.
			/// </summary>
			LimitToDirectories,
			/// <summary>This filtering type is not available.</summary>
			LimitToDevices,
			[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
			MaxSearchOp,
		}
		[Flags]
		public enum FindExFlags : uint {
			/// <summary>No flags.</summary>
			None = 0,
			/// <summary>Searches are case-sensitive.</summary>
			CaseSensitive = 1,
			/// <summary>
			/// Uses a larger buffer for directory queries, which can increase performance of the find
			/// operation. Windows Server 2008, Windows Vista, Windows Server 2003 and Windows XP: This
			/// value is not supported until Windows Server 2008 R2 and Windows 7.
			/// </summary>
			LargeFetch = 2,
			/// <summary>
			/// Limits the results to files that are physically on disk. This flag is only relevant when a
			/// file virtualization filter is present. 
			/// </summary>
			OnDiskEntriesOnly = 4,
		}
	}
}
