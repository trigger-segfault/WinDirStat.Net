using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinDirStat.Net.Structures;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Services {
    /// <summary>An service for creating and loading bitmaps.</summary>
    public class BitmapFactory {

		#region Fields

		/// <summary>The service for performing UI actions such as dispatcher invoking.</summary>
		private readonly UIService ui;

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="BitmapFactory"/>.</summary>
		public BitmapFactory(UIService ui) {
			this.ui = ui;
		}

		#endregion

		#region Create

		/// <summary>Creates a new writeable bitmap.</summary>
		/// 
		/// <param name="size">The size of the bitmap.</param>
		/// <returns>The new writeable bitmap.</returns>
		public WriteableBitmap CreateBitmap(Point2I size) {
			return ui.Invoke(() => new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Bgra32, null));
		}

		#endregion

		#region From Source

		/// <summary>Loads a bitmap from the specified resource path.</summary>
		/// 
		/// <param name="resourcePath">The resource path to load the bitmap from.</param>
		/// <param name="assembly">The assembly to load teh embedded resource from.</param>
		/// <returns>The loaded bitmap.</returns>
		public BitmapSource FromResource(string resourcePath, Assembly assembly = null) {
			assembly = assembly ?? Assembly.GetCallingAssembly();
			return ui.Invoke(() => {
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.UriSource = WpfUtils.MakePackUri(resourcePath, assembly);
				bitmapImage.EndInit();
				bitmapImage.Freeze();
				return bitmapImage;
			});
		}

		/// <summary>Loads a bitmap from the specified file path.</summary>
		/// 
		/// <param name="filePath">The file path to load the bitmap from.</param>
		/// <returns>The loaded bitmap.</returns>
		public BitmapSource FromFile(string filePath) {
			return ui.Invoke(() => {
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.UriSource = new Uri(filePath);
				bitmapImage.EndInit();
				bitmapImage.Freeze();
				return bitmapImage;
			});
		}

		/// <summary>Loads a bitmap from the specified stream.</summary>
		/// 
		/// <param name="stream">The stream to load the bitmap from.</param>
		/// <returns>The loaded bitmap.</returns>
		public BitmapSource FromStream(Stream stream) {
			return ui.Invoke(() => {
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = stream;
				bitmapImage.EndInit();
				bitmapImage.Freeze();
				return bitmapImage;
			});
		}

		/// <summary>Loads a bitmap from the specified icon handle.</summary>
		/// 
		/// <param name="handle">The handle of the icon to load.</param>
		/// <returns>The loaded bitmap.</returns>
		public BitmapSource FromHIcon(IntPtr hIcon) {
			var bmpSource = Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bmpSource.Freeze();
            return bmpSource;
		}

		#endregion
	}

	/*/// <summary>An implementation for a UI-independent WPF bitmap.</summary>
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
		/// <summary>Gets the actual bitmap object.</summary>
		object IBitmap.Source => Source;

		#endregion
	}

	/// <summary>An implementation for a UI-independent WPF bitmap with writeable pixels.</summary>
	public class WpfWriteableBitmap : WpfBitmap, IWriteableBitmap {

		#region Fields

		/// <summary>Gets the service for invoking UI actions.</summary>
		protected readonly UIService ui;

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="WpfWriteableBitmap"/>.</summary>
		public WpfWriteableBitmap(UIService ui, WriteableBitmap source) : base(source) {
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
			ui.Invoke(() => Source.WritePixels((Int32Rect) Bounds, pixels, Stride, 0));
		}
		/// <summary>Sets the bitmap's pixels.</summary>
		public unsafe void SetPixels(Rgba32Color* pixels) {
			ui.Invoke(() => Source.WritePixels((Int32Rect) Bounds, (IntPtr) pixels, BufferSize, Stride));
		}

		#endregion

		#region Properties

		/// <summary>Gets the actual bitmap object.</summary>
		public new WriteableBitmap Source => (WriteableBitmap) base.Source;
		/// <summary>Gets the stride for the bitmap.</summary>
		public int Stride => Width * 4;
		/// <summary>Gets the buffer size for the bitmap.</summary>
		public int BufferSize => Width * Height * 4;

		#endregion
	}*/
}
