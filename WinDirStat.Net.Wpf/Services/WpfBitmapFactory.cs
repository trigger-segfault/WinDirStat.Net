using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinDirStat.Net.Services;
using WinDirStat.Net.Services.Structures;
using WinDirStat.Net.Structures;
using WinDirStat.Net.Wpf.Services.Structures;
using WinDirStat.Net.Wpf.Utils;

namespace WinDirStat.Net.Wpf.Services {
	/// <summary>An service for creating and loading bitmaps.</summary>
	public class WpfBitmapFactory : IBitmapFactory {

		#region Fields

		/// <summary>The service for performing UI actions such as dispatcher invoking.</summary>
		private readonly IUIService ui;

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="WpfBitmapFactory"/>.</summary>
		public WpfBitmapFactory(IUIService ui) {
			this.ui = ui;
		}

		#endregion

		#region Create

		/// <summary>Creates a new writeable bitmap.</summary>
		/// 
		/// <param name="size">The size of the bitmap.</param>
		/// <returns>The new writeable bitmap.</returns>
		public IWriteableBitmap CreateBitmap(Point2I size) {
			return ui.Invoke(() => new WpfWriteableBitmap(ui,
				new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Bgra32, null)));
		}

		#endregion

		#region From Source

		/// <summary>Loads a bitmap from the specified resource path.</summary>
		/// 
		/// <param name="resourcePath">The resource path to load the bitmap from.</param>
		/// <param name="assembly">The assembly to load teh embedded resource from.</param>
		/// <returns>The loaded bitmap.</returns>
		public IBitmap FromResource(string resourcePath, Assembly assembly = null) {
			assembly = assembly ?? Assembly.GetCallingAssembly();
			return ui.Invoke(() => {
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.UriSource = WpfUtils.MakePackUri(resourcePath, assembly);
				bitmapImage.EndInit();
				bitmapImage.Freeze();
				return new WpfBitmap(bitmapImage);
			});
		}

		/// <summary>Loads a bitmap from the specified file path.</summary>
		/// 
		/// <param name="filePath">The file path to load the bitmap from.</param>
		/// <returns>The loaded bitmap.</returns>
		public IBitmap FromFile(string filePath) {
			return ui.Invoke(() => {
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.UriSource = new Uri(filePath);
				bitmapImage.EndInit();
				bitmapImage.Freeze();
				return new WpfBitmap(bitmapImage);
			});
		}

		/// <summary>Loads a bitmap from the specified stream.</summary>
		/// 
		/// <param name="stream">The stream to load the bitmap from.</param>
		/// <returns>The loaded bitmap.</returns>
		public IBitmap FromStream(Stream stream) {
			return ui.Invoke(() => {
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = stream;
				bitmapImage.EndInit();
				bitmapImage.Freeze();
				return new WpfBitmap(bitmapImage);
			});
		}

		/// <summary>Loads a bitmap from the specified icon handle.</summary>
		/// 
		/// <param name="handle">The handle of the icon to load.</param>
		/// <returns>The loaded bitmap.</returns>
		public IBitmap FromHIcon(IntPtr hIcon) {
			return ui.Invoke(() => {
				return new WpfBitmap(
					Imaging.CreateBitmapSourceFromHIcon(
						hIcon,
						Int32Rect.Empty,
						BitmapSizeOptions.FromEmptyOptions()));
			});
		}

		#endregion
	}
}
