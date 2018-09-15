using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Native {
	/// <summary>A static class for native methods, structs, and enums.</summary>
	public static partial class Win32 {

		/// <summary>An invalid handle value of -1 returned from some functions.</summary>
		public static readonly IntPtr InvalidHandle = new IntPtr(-1);

		/// <summary>Invalid low-order file size returned by <see cref="GetCompressedFileSize"/>.</summary>
		public const uint InvalidFileSize = 0xFFFFFFFF;

		/// <summary>
		/// Retrieves information about the amount of space that is available on a disk volume, which is the
		/// total amount of space, the total amount of free space, and the total amount of free space
		/// available to the user that is associated with the calling thread.
		/// </summary>
		/// 
		/// <param name="lpDirectoryName">A directory on the disk.</param>
		/// <param name="lpFreeBytesAvailable">
		/// Receives the total number of free bytes on a disk that are available to the user who is
		/// associated with the calling thread.
		/// </param>
		/// <param name="lpTotalNumberOfBytes">
		/// A pointer to a variable that receives the total number of bytes on a disk that are available to
		/// the user who is associated with the calling thread.
		/// </param>
		/// <param name="lpTotalNumberOfFreeBytes">Receives the total number of free bytes on a disk.</param>
		/// <returns>If the function succeeds, the return value is true.</returns>
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetDiskFreeSpaceEx(
			string lpDirectoryName,
			out ulong lpFreeBytesAvailable,
			out ulong lpTotalNumberOfBytes,
			out ulong lpTotalNumberOfFreeBytes);

		/// <summary>
		/// Retrieves the actual number of bytes of disk storage used to store a specified file. If the file
		/// is located on a volume that supports compression and the file is compressed, the value obtained
		/// is the compressed size of the specified file. If the file is located on a volume that supports
		/// sparse files and the file is a sparse file, the value obtained is the sparse size of the
		/// specified file.
		/// </summary>
		/// 
		/// <param name="lpFileName">The name of the file.</param>
		/// <param name="lpFileSizeHigh">
		/// The high-order DWORD of the compressed file size. The function's return value is the low-order
		/// DWORD of the compressed file size.
		/// </param>
		/// <returns>
		/// If the function succeeds, the return value is the low-order DWORD of the actual number of bytes
		/// of disk storage used to store the specified file, and if lpFileSizeHigh is non-NULL, the function
		/// puts the high-order DWORD of that actual value into the DWORD pointed to by that parameter. This
		/// is the compressed file size for compressed files, the actual file size for noncompressed files.
		/// <para/>
		/// If the function fails, and lpFileSizeHigh is NULL, the return value is INVALID_FILE_SIZE. To get
		/// extended error information, call GetLastError.
		/// </returns>
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint GetCompressedFileSize(
			string lpFileName,
			out int lpFileSizeHigh);
		
		/// <summary>Destroys an icon and frees any memory the icon occupied.</summary>
		/// 
		/// <param name="hIcon">A handle to the icon to be destroyed. The icon must not be in use.</param>
		/// <returns>If the function succeeds, the return value is true.</returns>
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool DestroyIcon(
			IntPtr hIcon);

		/// <summary>
		/// The DeleteObject function deletes a logical pen, brush, font, bitmap, region, or palette, freeing
		/// all system resources associated with the object. After the object is deleted, the specified
		/// handle is no longer valid.
		/// </summary>
		/// 
		/// <param name="hObject">A handle to a logical pen, brush, font, bitmap, region, or palette.</param>
		/// <returns>If the function succeeds, the return value is true.</returns>
		[DllImport("gdi32.dll", SetLastError = true)]
		public static extern bool DeleteObject(
			IntPtr hObject);

		/// <summary>
		/// Retrieves the path of the system directory. The system directory contains system files such as
		/// dynamic-link libraries and drivers.
		/// </summary>
		/// 
		/// <param name="lpBuffer">Receives the path</param>
		/// <param name="uSize">The maximum size of the buffer.</param>
		/// <returns>If the function succeeds, the return value is the length.</returns>
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint GetSystemDirectory(out string lpBuffer, int uSize);

	}
}
