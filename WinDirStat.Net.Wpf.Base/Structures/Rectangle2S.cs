using System;
using System.Runtime.InteropServices;

using WinDirStat.Net.Utils;

using GdiRectangle = System.Drawing.Rectangle;
using GdiRectangleF = System.Drawing.RectangleF;
using WpfInt32Rect = System.Windows.Int32Rect;

namespace WinDirStat.Net.Structures {
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Rectangle2S {

		//-----------------------------------------------------------------------------
		// Constants
		//-----------------------------------------------------------------------------

		/// <summary>Returns a rectangle positioned at (0, 0) with a size of (0, 0).</summary>
		public static readonly Rectangle2S Empty = new Rectangle2S(0, 0, 0, 0);
		/// <summary>Returns a rectangle positioned at (0, 0) with a size of (1, 1).</summary>
		public static readonly Rectangle2S One = new Rectangle2S(0, 0, 1, 1);
		/// <summary>
		/// Returns a rectangle positioned at (<see cref="ushort.MaxValue"/>, <see cref="ushort.MaxValue"/>)
		/// with a size of (0, 0).
		/// </summary>
		public static readonly Rectangle2S Invalid = new Rectangle2S(ushort.MaxValue, ushort.MaxValue, 0, 0);

		//-----------------------------------------------------------------------------
		// Members
		//-----------------------------------------------------------------------------

		/// <summary>X coordinate of this rectangle.</summary>
		public ushort X;
		/// <summary>Y coordinate of this rectangle.</summary>
		public ushort Y;
		/// <summary>Width of this rectangle.</summary>
		public ushort Width;
		/// <summary>Height of this rectangle.</summary>
		public ushort Height;


		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------

		public Rectangle2S(int width, int height) {
			X = 0;
			Y = 0;
			Width = (ushort) width;
			Height = (ushort) height;
		}

		public Rectangle2S(int x, int y, int width, int height) {
			X = (ushort) (x == -1 ? ushort.MaxValue : x);
			Y = (ushort) (y == -1 ? ushort.MaxValue : y);
			Width = (ushort) width;
			Height = (ushort) height;
		}

		public Rectangle2S(Point2I size) {
			X = 0;
			Y = 0;
			Width = (ushort) size.X;
			Height = (ushort) size.Y;
		}

		public Rectangle2S(Point2I point, Point2I size) {
			X = (ushort) (point.X == -1 ? ushort.MaxValue : point.X);
			Y = (ushort) (point.Y == -1 ? ushort.MaxValue : point.Y);
			Width = (ushort) size.X;
			Height = (ushort) size.Y;
		}

		public static Rectangle2S FromLTRB(int left, int top, int right, int bottom) {
			return new Rectangle2S(left, top, right - left, bottom - top);
		}

		public static explicit operator GdiRectangle(Rectangle2S rect) {
			return new GdiRectangle(
				(rect.X == ushort.MaxValue ? -1 : rect.X),
				(rect.Y == ushort.MaxValue ? -1 : rect.Y),
				rect.Width,
				rect.Height);
		}

		public static implicit operator Rectangle2S(GdiRectangle rect) {
			return new Rectangle2S(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static explicit operator Rectangle2S(GdiRectangleF rect) {
			return new Rectangle2S((int) rect.X, (int) rect.Y, (int) rect.Width, (int) rect.Height);
		}

		public static implicit operator Rectangle2S(WpfInt32Rect rect) {
			return new Rectangle2S(rect.X, rect.Y, rect.Width, rect.Height);
		}
	}
}
