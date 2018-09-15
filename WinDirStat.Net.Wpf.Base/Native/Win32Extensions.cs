using System;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace WinDirStat.Net.Native {
	/// <summary>Extentions for native Win32 structures.</summary>
	internal static class Win32Extensions {

		/// <summary>Converts the native <see cref="FILETIME"/> to a local <see cref="DateTime"/>.</summary>
		public static DateTime ToDateTime(this FILETIME fileTime) {
			long ticks = ((long) fileTime.dwHighDateTime << 32) | unchecked((uint) fileTime.dwLowDateTime);
			return DateTime.FromFileTime(ticks);
		}

		/// <summary>Converts the native <see cref="FILETIME"/> to a UTC <see cref="DateTime"/>.</summary>
		public static DateTime ToDateTimeUtc(this FILETIME fileTime) {
			long ticks = ((long) fileTime.dwHighDateTime << 32) | unchecked((uint) fileTime.dwLowDateTime);
			return DateTime.FromFileTimeUtc(ticks);
		}
	}
}
