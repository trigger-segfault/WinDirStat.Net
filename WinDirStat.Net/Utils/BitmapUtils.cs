using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GdiPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace WinDirStat.Net.Utils {
	public static class BitmapUtils {
		/// <summary>Loads a bitmap from the specified resource path and assembly.</summary>
		public static BitmapSource FromResource(string resourcePath, Assembly assembly = null) {
			BitmapImage bitmapImage = null;
			assembly = assembly ?? Assembly.GetCallingAssembly();
			Application.Current.Dispatcher.Invoke(() => {
				bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.UriSource = MakePackUri(resourcePath, assembly);
				bitmapImage.EndInit();
				bitmapImage.Freeze();
			});
			return bitmapImage;
		}

		/// <summary>Creates a pack Uri for loading resource images.</summary>
		public static Uri MakePackUri(string resourcePath, Assembly assembly = null) {
			assembly = assembly ?? Assembly.GetCallingAssembly();
			// Pull out the short name.
			string assemblyShortName = assembly.ToString().Split(',')[0];
			string uriString = $"pack://application:,,,/{assemblyShortName};component/{resourcePath}";
			return new Uri(uriString);
		}

		public static BitmapSource FromGdiBitmap(Bitmap bitmap) {
			WriteableBitmap writeableBitmap = null;
			Rectangle rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			Int32Rect int32Rect = new Int32Rect(0, 0, bitmap.Width, bitmap.Height);
			BitmapData data = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly,
				GdiPixelFormat.Format32bppArgb);
			Application.Current.Dispatcher.Invoke(() => {
				try {
					writeableBitmap = new WriteableBitmap(bitmap.Width, bitmap.Height, 96, 96,
						PixelFormats.Bgra32, null);
					writeableBitmap.WritePixels(int32Rect, data.Scan0, data.Stride * data.Height, data.Stride);
				}
				finally {
					bitmap.UnlockBits(data);
				}
				writeableBitmap.Freeze();
			});
			return writeableBitmap;
		}

		public static BitmapSource FromFile(string filePath) {
			BitmapImage bitmapImage = null;
			Application.Current.Dispatcher.Invoke(() => {
				bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.UriSource = new Uri(filePath);
				bitmapImage.EndInit();
				bitmapImage.Freeze();
			});
			return bitmapImage;
		}
	}
}
