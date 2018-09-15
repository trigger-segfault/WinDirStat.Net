using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Structures;

namespace WinDirStat.Net.Services {
	/// <summary>An interface for creating and loading bitmaps.</summary>
	public interface IBitmapFactory {

		/// <summary>Creates a new writeable bitmap.</summary>
		/// 
		/// <param name="size">The size of the bitmap.</param>
		/// <returns>The new writeable bitmap.</returns>
		IWriteableBitmap CreateBitmap(Point2I size);

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

		/// <summary>Loads a bitmap from the specified icon handle.</summary>
		/// 
		/// <param name="handle">The handle of the icon to load.</param>
		/// <returns>The loaded bitmap.</returns>
		IBitmap FromHIcon(IntPtr handle);
	}

	/// <summary>An interface for a UI-independent bitmap.</summary>
	public interface IBitmap {

		#region Properties

		/// <summary>Gets the width of the bitmap.</summary>
		int Width { get; }
		/// <summary>Gets the height of the bitmap.</summary>
		int Height { get; }
		/// <summary>Gets the size of the bitmap.</summary>
		Point2I Size { get; }
		/// <summary>Gets the bounds of the bitmap.</summary>
		Rectangle2I Bounds { get; }

		/// <summary>Gets the actual bitmap object.</summary>
		object Source { get; }

		#endregion
	}

	/// <summary>An interface for a UI-independent bitmap with writeable pixels.</summary>
	public interface IWriteableBitmap : IBitmap {

		#region Pixels

		/// <summary>Creates a new array of pixels for populating the bitmap.</summary>
		Rgba32Color[] CreatePixels();
		/// <summary>Gets the bitmap's pixels.</summary>
		Rgba32Color[] GetPixels();
		/// <summary>Sets the bitmap's pixels.</summary>
		void SetPixels(Rgba32Color[] pixels);
		/// <summary>Sets the bitmap's pixels.</summary>
		unsafe void SetPixels(Rgba32Color* pixels);

		#endregion
	}
}
