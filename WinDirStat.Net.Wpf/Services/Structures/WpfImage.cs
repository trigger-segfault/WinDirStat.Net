using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinDirStat.Net.Services;
using WinDirStat.Net.Services.Structures;
using WinDirStat.Net.Structures;
using WinDirStat.Net.Wpf.Utils;

namespace WinDirStat.Net.Wpf.Services.Structures {
	/// <summary>An implementation for a UI-independent WPF image.</summary>
	public class WpfImage : IImage {

		#region Fields

		/// <summary>Gets the actual image object.</summary>
		public ImageSource Source { get; }

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="WpfImage"/>.</summary>
		public WpfImage(ImageSource source) {
			Source = source;
		}

		#endregion

		#region Properties
		
		/// <summary>Gets the actual image object.</summary>
		object IImage.Source => Source;

		#endregion
	}
	
	/// <summary>An implementation for a UI-independent WPF bitmap.</summary>
	public class WpfBitmap : IBitmap {

		#region Fields

		/// <summary>Gets the actual bitmap object.</summary>
		public BitmapSource Source { get; }
		/// <summary>Gets the width of the bitmap.</summary>
		public int Width { get; }
		/// <summary>Gets the height of the bitmap.</summary>
		public int Height { get; }

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="WpfBitmap"/>.</summary>
		public WpfBitmap(BitmapSource source) {
			Source = source;
			Width = source.PixelWidth;
			Height = source.PixelHeight;
		}

		#endregion

		#region Properties

		/// <summary>Gets the size of the bitmap.</summary>
		public Point2I Size => new Point2I(Width, Height);
		/// <summary>Gets the bounds of the bitmap.</summary>
		public Rectangle2I Bounds => new Rectangle2I(Width, Height);
		/// <summary>Gets the actual image object.</summary>
		object IImage.Source => Source;

		#endregion
	}

	/// <summary>An implementation for a UI-independent WPF bitmap with writeable pixels.</summary>
	public class WpfWriteableBitmap : IWriteableBitmap {

		#region Fields

		/// <summary>Gets the actual bitmap object.</summary>
		public WriteableBitmap Source { get; }
		/// <summary>Gets the width of the bitmap.</summary>
		public int Width { get; }
		/// <summary>Gets the height of the bitmap.</summary>
		public int Height { get; }
		/// <summary>Gets the service for invoking UI actions.</summary>
		protected readonly IUIService ui;

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="WpfWriteableBitmap"/>.</summary>
		public WpfWriteableBitmap(IUIService ui, WriteableBitmap source) {
			Source = source;
			Width = source.PixelWidth;
			Height = source.PixelHeight;
			this.ui = ui;
		}

		#endregion

		#region Pixels

		/// <summary>Creates a new array of pixels for populating the bitmap.</summary>
		public Rgba32Color[] CreatePixels() {
			return new Rgba32Color[Width * Height * 4];
		}
		/// <summary>Gets the bitmap's pixels.</summary>
		public Rgba32Color[] GetPixels() {
			Rgba32Color[] pixels = CreatePixels();
			ui.Invoke(() => Source.CopyPixels(pixels, Width * 4, 0));
			return pixels;
		}
		/// <summary>Sets the bitmap's pixels.</summary>
		public void SetPixels(Rgba32Color[] pixels) {
			ui.Invoke(() => Source.WritePixels(Bounds.ToWpfInt32Rect(), pixels, Stride, 0));
		}
		/// <summary>Sets the bitmap's pixels.</summary>
		public unsafe void SetPixels(Rgba32Color* pixels) {
			ui.Invoke(() => Source.WritePixels(Bounds.ToWpfInt32Rect(), (IntPtr) pixels, BufferSize, Stride));
		}

		#endregion

		#region Properties
		
		/// <summary>Gets the stride for the bitmap.</summary>
		public int Stride => Width * 4;
		/// <summary>Gets the buffer size for the bitmap.</summary>
		public int BufferSize => Width * Height * 4;
		/// <summary>Gets the size of the bitmap.</summary>
		public Point2I Size => new Point2I(Width, Height);
		/// <summary>Gets the bounds of the bitmap.</summary>
		public Rectangle2I Bounds => new Rectangle2I(Width, Height);
		/// <summary>Gets the actual image object.</summary>
		object IImage.Source => Source;

		#endregion
	}
}
