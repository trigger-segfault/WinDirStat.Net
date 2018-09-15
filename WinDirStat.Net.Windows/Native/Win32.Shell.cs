using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Native {
	partial class Win32 {

		/// <summary>
		/// Requests the form of an item's display name to retrieve through <see cref="SHGetNameFromIDList"
		/// />.
		/// </summary>
		public enum SIGetDisplayName : uint {
			/// <summary>
			/// Returns the display name relative to the parent folder. In UI this name is generally ideal
			/// for display to the user.
			/// </summary>
			NormalDisplay = 0x00000000,
			/// <summary>
			/// Returns the parsing name relative to the parent folder. This name is not suitable for use in
			/// UI.
			/// </summary>
			ParentRelativeParsing = 0x80018001,
			/// <summary>
			/// Returns the parsing name relative to the desktop. This name is not suitable for use in UI.
			/// </summary>
			DesktopAbsoluteParsing = 0x80028000,
			/// <summary>
			/// Returns the editing name relative to the parent folder. In UI this name is suitable for
			/// display to the user.
			/// </summary>
			ParentRelativeEditing = 0x80031001,
			/// <summary>
			/// Returns the editing name relative to the desktop. In UI this name is suitable for display to
			/// the user.
			/// </summary>
			DesktopAbsoluteEditing = 0x8004c000,
			/// <summary>
			/// Returns the item's file system path, if it has one. Only items that report SFGAO_FILESYSTEM
			/// have a file system path. When an item does not have a file system path, a call to <see cref=
			/// "SHGetNameFromIDList"/> on that item will fail. In UI this name is suitable for display to
			/// the user in some cases, but note that it might not be specified for all items.
			/// </summary>
			FileSysPath = 0x80058000,
			/// <summary>
			/// Returns the item's URL, if it has one. Some items do not have a URL, and in those cases a
			/// call to <see cref="SHGetNameFromIDList"/> will fail. This name is suitable for display to the
			/// user in some cases, but note that it might not be specified for all items.
			/// </summary>
			Url = 0x80068000,
			/// <summary>
			/// Returns the path relative to the parent folder in a friendly format as displayed in an
			/// address bar. This name is suitable for display to the user.
			/// </summary>
			ParentRelativeForAddressBar = 0x8007c001,
			/// <summary>Returns the path relative to the parent folder.</summary>
			ParentRelative = 0x80080001,
			/// <summary>Introduced in Windows 8.</summary>
			ParentRelativeForUI = 0x80094001,
		}

		// https://stackoverflow.com/a/29198314/7517185

		/// <summary>
		/// Translates a Shell namespace object's display name into an item identifier list and returns the
		/// attributes of the object. This function is the preferred method to convert a string to a pointer
		/// to an item identifier list (PIDL).
		/// </summary>
		/// 
		/// <param name="pszName">A string that contains the display name to parse.</param>
		/// <param name="zero">This parameter is normally set to null.</param>
		/// <param name="ppidl"></param>
		/// <param name="sfgaoIn"></param>
		/// <param name="psfgaoOut"></param>
		/// <returns></returns>
		[DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool SHParseDisplayName(
			string pszName,
			IntPtr pbc,
			out IntPtr ppidl,
			int sfgaoIn,
			out int psfgaoOut);

		/// <summary>Retrieves the display name of an item identified by its IDList.</summary>
		/// 
		/// <param name="pidl">A PIDL that identifies the item.</param>
		/// <param name="sigdnName">
		/// A value from the <see cref="SIGetDisplayName"/> enumeration that specifies the type of display
		/// name to retrieve.
		/// </param>
		/// <param name="ppszName">
		/// A value that, when this function returns successfully, receives the display name.
		/// </param>
		/// <returns>If this function succeeds, it returns false. Otherwise, it returns true.</returns>
		[DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool SHGetNameFromIDList(
			IntPtr pidl,
			SIGetDisplayName sigdnName,
			out string ppszName);

	}
}
