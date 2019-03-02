using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Services.Structures;
using WinDirStat.Net.Structures;

namespace WinDirStat.Net.Services {
	/// <summary>An interface for creating and loading bitmaps.</summary>
	public interface IBitmapFactory {

		#region Create

		/// <summary>Creates a new writeable bitmap.</summary>
		/// 
		/// <param name="size">The size of the bitmap.</param>
		/// <returns>The new writeable bitmap.</returns>
		IWriteableBitmap CreateBitmap(Point2I size);

		#endregion

		#region From Source

		/// <summary>Loads a bitmap from the specified resource path.</summary>
		/// 
		/// <param name="resourcePath">The resource path to load the bitmap from.</param>
		/// <param name="assembly">
		/// The assembly to load the resource from. The calling assembly is used if null.
		/// </param>
		/// <returns>The loaded bitmap.</returns>
		IBitmap FromResource(string resourcePath, Assembly assembly = null);

		/// <summary>Loads a bitmap from the specified file path.</summary>
		/// 
		/// <param name="filePath">The file path to load the bitmap from.</param>
		/// <returns>The loaded bitmap.</returns>
		IBitmap FromFile(string filePath);

		/// <summary>Loads a bitmap from the specified stream.</summary>
		/// 
		/// <param name="stream">The stream to load the bitmap from.</param>
		/// <returns>The loaded bitmap.</returns>
		IBitmap FromStream(Stream stream);

#if WINDOWS

		/// <summary>Loads a bitmap from the specified icon handle.</summary>
		/// 
		/// <param name="handle">The handle of the icon to load.</param>
		/// <returns>The loaded bitmap.</returns>
		IBitmap FromHIcon(IntPtr handle);

#endif

	#endregion
	}
}
