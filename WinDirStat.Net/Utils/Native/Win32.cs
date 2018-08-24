using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Utils.Native {
	public static partial class Win32 {

		/// <summary>An invalid handle value of -1 returned from some functions.</summary>
		public static readonly IntPtr InvalidHandle = new IntPtr(-1);

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


		/// <summary>Destroys an icon and frees any memory the icon occupied.</summary>
		/// 
		/// <param name="hIcon">A handle to the icon to be destroyed. The icon must not be in use.</param>
		/// <returns>If the function succeeds, the return value is true.</returns>
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool DestroyIcon(IntPtr hIcon);

		/// <summary>
		/// The DeleteObject function deletes a logical pen, brush, font, bitmap, region, or palette, freeing
		/// all system resources associated with the object. After the object is deleted, the specified
		/// handle is no longer valid.
		/// </summary>
		/// 
		/// <param name="hObject">A handle to a logical pen, brush, font, bitmap, region, or palette.</param>
		/// <returns>If the function succeeds, the return value is true.</returns>
		[DllImport("gdi32.dll", SetLastError = true)]
		public static extern bool DeleteObject(IntPtr hObject);

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
