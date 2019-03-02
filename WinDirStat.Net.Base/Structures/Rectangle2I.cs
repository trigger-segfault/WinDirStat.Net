using System;
using System.Runtime.InteropServices;

namespace WinDirStat.Net.Structures {
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Rectangle2I {

		#region Constants

		/// <summary>Returns a rectangle positioned at (0, 0) with a size of (0, 0).</summary>
		public static readonly Rectangle2I Empty = new Rectangle2I(0, 0, 0, 0);
		/// <summary>Returns a rectangle positioned at (0, 0) with a size of (1, 1).</summary>
		public static readonly Rectangle2I One = new Rectangle2I(0, 0, 1, 1);
		/// <summary>Returns a rectangle positioned at (-1, -1) with a size of (0, 0).</summary>
		public static readonly Rectangle2I Invalid = new Rectangle2I(-1, -1, 0, 0);

		#endregion

		#region Fields

		/// <summary>X coordinate of this rectangle.</summary>
		public int X;
		/// <summary>Y coordinate of this rectangle.</summary>
		public int Y;
		/// <summary>Width of this rectangle.</summary>
		public int Width;
		/// <summary>Height of this rectangle.</summary>
		public int Height;

		#endregion

		#region Constructors

		public Rectangle2I(int width, int height) {
			X = 0;
			Y = 0;
			Width = width;
			Height = height;
		}

		public Rectangle2I(int x, int y, int width, int height) {
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public Rectangle2I(Point2I size) {
			X = 0;
			Y = 0;
			Width = size.X;
			Height = size.Y;
		}

		public Rectangle2I(Point2I point, Point2I size) {
			X = point.X;
			Y = point.Y;
			Width = size.X;
			Height = size.Y;
		}

		#endregion

		#region Static Constructors

		public static Rectangle2I FromLTRB(int left, int top, int right, int bottom) {
			return new Rectangle2I(left, top, right - left, bottom - top);
		}

		#endregion

		#region Properties

		public int Left {
			get => X;
			set {
				// Keep the right position
				Width += X - value;
				X = value;
			}
		}

		public int Top {
			get => Y;
			set {
				// Keep the right position
				Height += Y - value;
				Y = value;
			}
		}

		public int Right {
			get => X + Width;
			set => Width = value - X;
		}

		public int Bottom {
			get => Y + Height;
			set => Height = value - Y;
		}

		#endregion

		#region Casting

		public static implicit operator Rectangle2I(Rectangle2S rect) {
			return new Rectangle2I(
				(rect.X == ushort.MaxValue ? -1 : rect.X),
				(rect.Y == ushort.MaxValue ? -1 : rect.Y),
				rect.Width,
				rect.Height);
		}

		public static explicit operator Rectangle2S(Rectangle2I rect) {
			return new Rectangle2S(rect.X, rect.Y, rect.Width, rect.Height);
		}

		#endregion

		#region Accessors

		public bool Contains(int x, int y) {
			return Contains(new Point2I(x, y));
		}

		public bool Contains(Point2I point) {
			return (point.X >= Left && point.X <= Right &&
					point.Y >= Top && point.Y <= Bottom);
		}

		#endregion

		#region Mutators

		public void Inflate(int x, int y) {
			X -= x;
			Y -= y;
			Width += x * 2;
			Height += y * 2;
		}

		public void Deflate(int x, int y) {
			X += x;
			Y += y;
			Width -= x * 2;
			Height -= y * 2;
		}

		#endregion
	}
}
