using WinDirStat.Net.Structures;

namespace WinDirStat.Net.Services.Structures {
	/// <summary>An interface for a UI-independent image.</summary>
	public interface IImage {

		#region Properties
		
		/// <summary>Gets the actual image object.</summary>
		object Source { get; }

		#endregion
	}

	/// <summary>An interface for a UI-independent bitmap.</summary>
	public interface IBitmap : IImage {

		#region Properties

		/// <summary>Gets the width of the bitmap.</summary>
		int Width { get; }
		/// <summary>Gets the height of the bitmap.</summary>
		int Height { get; }
		/// <summary>Gets the size of the bitmap.</summary>
		Point2I Size { get; }
		/// <summary>Gets the bounds of the bitmap.</summary>
		Rectangle2I Bounds { get; }
		
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
