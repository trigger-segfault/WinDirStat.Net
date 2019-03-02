using WinDirStat.Net.Structures;

using WpfColor = System.Windows.Media.Color;
using WpfPoint = System.Windows.Point;
using WpfSize = System.Windows.Size;
using WpfRect = System.Windows.Rect;
using WpfInt32Rect = System.Windows.Int32Rect;

namespace WinDirStat.Net.Wpf.Utils {
	/// <summary>Static extensions for casting between Wpf and WinDirStat.Net values.</summary>
	public static class WpfCasting {

		#region Color

		/// <summary>Casts the <see cref="Rgb24Color"/> to a <see cref="WpfColor"/>.</summary>
		public static WpfColor ToWpfColor(this Rgb24Color color) {
			return WpfColor.FromRgb(color.R, color.G, color.B);
		}

		/// <summary>Casts the <see cref="Rgba32Color"/> to a <see cref="WpfColor"/>.</summary>
		public static WpfColor ToWpfColor(this Rgba32Color color) {
			return WpfColor.FromArgb(color.A, color.R, color.G, color.B);
		}

		/// <summary>Casts the <see cref="WpfColor"/> to an <see cref="Rgb24Color"/>.</summary>
		public static Rgb24Color ToRgb24Color(this WpfColor color) {
			return new Rgb24Color(color.R, color.G, color.B);
		}

		/// <summary>Casts the <see cref="WpfColor"/> to an <see cref="Rgba32Color"/>.</summary>
		public static Rgba32Color ToRgba32Color(this WpfColor color) {
			return new Rgba32Color(color.R, color.G, color.B, color.A);
		}

		#endregion

		#region Point

		/// <summary>Casts the <see cref="Point2I"/> to a <see cref="WpfPoint"/>.</summary>
		public static WpfPoint ToWpfPoint(this Point2I point) {
			return new WpfPoint(point.X, point.Y);
		}

		/// <summary>Casts the <see cref="Point2F"/> to a <see cref="WpfPoint"/>.</summary>
		public static WpfPoint ToWpfPoint(this Point2F point) {
			return new WpfPoint(point.X, point.Y);
		}

		/// <summary>Casts the <see cref="WpfPoint"/> to a <see cref="Point2I"/>.</summary>
		public static Point2I ToPoint2I(this WpfPoint point) {
			return new Point2I((int) point.X, (int) point.Y);
		}

		/// <summary>Casts the <see cref="WpfPoint"/> to a <see cref="Point2F"/>.</summary>
		public static Point2F ToPoint2F(this WpfPoint point) {
			return new Point2F((float) point.X, (float) point.Y);
		}

		#endregion

		#region Size
		
		/// <summary>Casts the <see cref="Point2I"/> to a <see cref="WpfSize"/>.</summary>
		public static WpfSize ToWpfSize(this Point2I point) {
			return new WpfSize(point.X, point.Y);
		}

		/// <summary>Casts the <see cref="Point2F"/> to a <see cref="WpfSize"/>.</summary>
		public static WpfSize ToWpfSize(this Point2F point) {
			return new WpfSize(point.X, point.Y);
		}

		/// <summary>Casts the <see cref="WpfSize"/> to a <see cref="Point2I"/>.</summary>
		public static Point2I ToPoint2I(this WpfSize size) {
			return new Point2I((int) size.Width, (int) size.Height);
		}

		/// <summary>Casts the <see cref="WpfSize"/> to a <see cref="Point2F"/>.</summary>
		public static Point2F ToPoint2F(this WpfSize size) {
			return new Point2F((float) size.Width, (float) size.Height);
		}

		#endregion

		#region Rect

		/// <summary>Casts the <see cref="Rectangle2S"/> to a <see cref="WpfRect"/>.</summary>
		public static WpfRect ToWpfRect(this Rectangle2S rect) {
			return new WpfRect(rect.X, rect.Y, rect.Width, rect.Height);
		}

		/// <summary>Casts the <see cref="Rectangle2I"/> to a <see cref="WpfRect"/>.</summary>
		public static WpfRect ToWpfRect(this Rectangle2I rect) {
			return new WpfRect(rect.X, rect.Y, rect.Width, rect.Height);
		}

		/// <summary>Casts the <see cref="WpfRect"/> to a <see cref="Rectangle2S"/>.</summary>
		public static Rectangle2S ToRectangle2S(this WpfRect rect) {
			return new Rectangle2S((int) rect.X,		(int) rect.Y,
								   (int) rect.Width,	(int) rect.Height);
		}

		/// <summary>Casts the <see cref="WpfRect"/> to a <see cref="Rectangle2I"/>.</summary>
		public static Rectangle2I ToRectangle2I(this WpfRect rect) {
			return new Rectangle2I((int) rect.X,		(int) rect.Y,
								   (int) rect.Width,	(int) rect.Height);
		}

		#endregion

		#region Int32Rect

		/// <summary>Casts the <see cref="Rectangle2S"/> to a <see cref="WpfInt32Rect"/>.</summary>
		public static WpfInt32Rect ToWpfInt32Rect(this Rectangle2S rect) {
			return new WpfInt32Rect(rect.X, rect.Y, rect.Width, rect.Height);
		}

		/// <summary>Casts the <see cref="Rectangle2I"/> to a <see cref="WpfInt32Rect"/>.</summary>
		public static WpfInt32Rect ToWpfInt32Rect(this Rectangle2I rect) {
			return new WpfInt32Rect(rect.X, rect.Y, rect.Width, rect.Height);
		}

		/// <summary>Casts the <see cref="WpfInt32Rect"/> to a <see cref="Rectangle2S"/>.</summary>
		public static Rectangle2S ToRectangle2S(this WpfInt32Rect rect) {
			return new Rectangle2S(rect.X, rect.Y, rect.Width, rect.Height);
		}

		/// <summary>Casts the <see cref="WpfInt32Rect"/> to a <see cref="Rectangle2I"/>.</summary>
		public static Rectangle2I ToRectangle2I(this WpfInt32Rect rect) {
			return new Rectangle2I(rect.X, rect.Y, rect.Width, rect.Height);
		}

		#endregion
	}
}
