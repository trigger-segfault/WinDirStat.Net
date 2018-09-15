using System;
using System.Drawing;
using System.Runtime.InteropServices;

using WinDirStat.Net.Utils;

using GdiPoint = System.Drawing.Point;
using GdiPointF = System.Drawing.PointF;
using GdiSize = System.Drawing.Size;
using GdiSizeF = System.Drawing.SizeF;
using WpfPoint = System.Windows.Point;
using WpfSize = System.Windows.Size;

namespace WinDirStat.Net.Structures {
	/// <summary>Point structure for integer 2D positions (X, Y).</summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Point2I {

		//-----------------------------------------------------------------------------
		// Constants
		//-----------------------------------------------------------------------------

		/// <summary>Returns a point positioned at (0, 0).</summary>
		public static readonly Point2I Zero = new Point2I(0, 0);
		/// <summary>Returns a point positioned at (1, 1).</summary>
		public static readonly Point2I One = new Point2I(1, 1);
		/// <summary>Returns a point positioned at (1, 0).</summary>
		public static readonly Point2I OneX = new Point2I(1, 0);
		/// <summary>Returns a point positioned at (0, 1).</summary>
		public static readonly Point2I OneY = new Point2I(0, 1);


		//-----------------------------------------------------------------------------
		// Members
		//-----------------------------------------------------------------------------

		/// <summary>X coordinate of this point.</summary>
		public int X;
		/// <summary>Y coordinate of this point.</summary>
		public int Y;


		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------

		/// <summary>Constructs a <see cref="Point2I"/> from the X and Y coordinates.</summary>
		/// <param name="x">The X coordinate to use.</param>
		/// <param name="y">The Y coordinate to use.</param>
		public Point2I(int x, int y) {
			X = x;
			Y = y;
		}

		/// <summary>Constructs a <see cref="Point2I"/> from the same coordinates.</summary>
		/// <param name="uniform">The X and Y coordinate to use.</param>
		public Point2I(int uniform) {
			X = uniform;
			Y = uniform;
		}


		//-----------------------------------------------------------------------------
		// General
		//-----------------------------------------------------------------------------

		/// <summary>Convert to a human-readable string.</summary>
		/// <returns>A string that represents the point</returns>
		public override string ToString() => $"(X={X} Y={Y})";

		/// <summary>Returns the hash code of this point.</summary>
		public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

		/// <summary>Checks if the point is equal to the other point.</summary>
		public override bool Equals(object obj) {
			switch (obj) {
			case Point2I pt2i: return this == pt2i;
			case Point2F pt2f: return this == pt2f;
			default: return false;
			}
		}


		//-----------------------------------------------------------------------------
		// Operators
		//-----------------------------------------------------------------------------

		public static Point2I operator +(Point2I a) => a;

		public static Point2I operator -(Point2I a) => new Point2I(-a.X, -a.Y);

		public static Point2I operator ++(Point2I a) => new Point2I(++a.X, ++a.Y);

		public static Point2I operator --(Point2I a) => new Point2I(--a.X, --a.Y);

		//--------------------------------

		public static Point2I operator +(Point2I a, Point2I b) {
			return new Point2I(a.X + b.X, a.Y + b.Y);
		}

		public static Point2I operator +(int a, Point2I b) {
			return new Point2I(a + b.X, a + b.Y);
		}

		public static Point2I operator +(Point2I a, int b) {
			return new Point2I(a.X + b, a.Y + b);
		}

		public static Point2F operator +(float a, Point2I b) {
			return new Point2F(a + b.X, a + b.Y);
		}

		public static Point2F operator +(Point2I a, float b) {
			return new Point2F(a.X + b, a.Y + b);
		}


		public static Point2I operator -(Point2I a, Point2I b) {
			return new Point2I(a.X - b.X, a.Y - b.Y);
		}

		public static Point2I operator -(int a, Point2I b) {
			return new Point2I(a - b.X, a - b.Y);
		}

		public static Point2I operator -(Point2I a, int b) {
			return new Point2I(a.X - b, a.Y - b);
		}

		public static Point2F operator -(float a, Point2I b) {
			return new Point2F(a - b.X, a - b.Y);
		}

		public static Point2F operator -(Point2I a, float b) {
			return new Point2F(a.X - b, a.Y - b);
		}


		public static Point2I operator *(Point2I a, Point2I b) {
			return new Point2I(a.X * b.X, a.Y * b.Y);
		}

		public static Point2I operator *(int a, Point2I b) {
			return new Point2I(a * b.X, a * b.Y);
		}

		public static Point2I operator *(Point2I a, int b) {
			return new Point2I(a.X * b, a.Y * b);
		}

		public static Point2F operator *(float a, Point2I b) {
			return new Point2F(a * b.X, a * b.Y);
		}

		public static Point2F operator *(Point2I a, float b) {
			return new Point2F(a.X * b, a.Y * b);
		}


		public static Point2I operator /(Point2I a, Point2I b) {
			return new Point2I(a.X / b.X, a.Y / b.Y);
		}

		public static Point2I operator /(int a, Point2I b) {
			return new Point2I(a / b.X, a / b.Y);
		}

		public static Point2I operator /(Point2I a, int b) {
			return new Point2I(a.X / b, a.Y / b);
		}

		public static Point2F operator /(float a, Point2I b) {
			return new Point2F(a / b.X, a / b.Y);
		}

		public static Point2F operator /(Point2I a, float b) {
			return new Point2F(a.X / b, a.Y / b);
		}


		public static Point2I operator %(Point2I a, Point2I b) {
			return new Point2I(a.X % b.X, a.Y % b.Y);
		}

		public static Point2I operator %(int a, Point2I b) {
			return new Point2I(a % b.X, a % b.Y);
		}

		public static Point2I operator %(Point2I a, int b) {
			return new Point2I(a.X % b, a.Y % b);
		}

		public static Point2F operator %(float a, Point2I b) {
			return new Point2F(a % b.X, a % b.Y);
		}

		public static Point2F operator %(Point2I a, float b) {
			return new Point2F(a.X % b, a.Y % b);
		}

		//--------------------------------

		public static bool operator ==(Point2I a, Point2I b) {
			return (a.X == b.X && a.Y == b.Y);
		}

		public static bool operator !=(Point2I a, Point2I b) {
			return (a.X != b.X || a.Y != b.Y);
		}

		public static bool operator <(Point2I a, Point2I b) {
			return (a.X < b.X && a.Y < b.Y);
		}

		public static bool operator >(Point2I a, Point2I b) {
			return (a.X > b.X && a.Y > b.Y);
		}

		public static bool operator <=(Point2I a, Point2I b) {
			return (a.X <= b.X && a.Y <= b.Y);
		}

		public static bool operator >=(Point2I a, Point2I b) {
			return (a.X >= b.X && a.Y >= b.Y);
		}


		//-----------------------------------------------------------------------------
		// Casting
		//-----------------------------------------------------------------------------

		/// <summary>Casts the <see cref="Point2I"/> to a Gdi <see cref="GdiPointF"/>.</summary>
		public static explicit operator GdiPointF(Point2I point) {
			return new PointF(point.X, point.Y);
		}

		/// <summary>Casts the <see cref="Point2I"/> to a Gdi <see cref="GdiPoint"/>.</summary>
		public static explicit operator GdiPoint(Point2I point) {
			return new Point(point.X, point.Y);
		}

		/// <summary>Casts the <see cref="Point2I"/> to a Gdi <see cref="GdiSizeF"/>.</summary>
		public static explicit operator GdiSizeF(Point2I point) {
			return new SizeF(point.X, point.Y);
		}

		/// <summary>Casts the <see cref="Point2I"/> to a Gdi <see cref="GdiSize"/>.</summary>
		public static explicit operator GdiSize(Point2I point) {
			return new Size(point.X, point.Y);
		}

		/// <summary>Casts the <see cref="Point2I"/> to a Gdi <see cref="WpfPoint"/>.</summary>
		public static explicit operator WpfPoint(Point2I point) {
			return new WpfPoint(point.X, point.Y);
		}

		/// <summary>Casts the <see cref="Point2I"/> to a Gdi <see cref="WpfSize"/>.</summary>
		public static explicit operator WpfSize(Point2I point) {
			return new WpfSize(point.X, point.Y);
		}

		/// <summary>Casts the Gdi <see cref="GdiPointF"/> to a <see cref="Point2I"/>.</summary>
		public static explicit operator Point2I(GdiPointF point) {
			return new Point2I((int) point.X, (int) point.Y);
		}

		/// <summary>Casts the Gdi <see cref="GdiPoint"/> to a <see cref="Point2I"/>.</summary>
		public static implicit operator Point2I(GdiPoint point) {
			return new Point2I(point.X, point.Y);
		}

		/// <summary>Casts the Gdi <see cref="GdiSizeF"/> to a <see cref="Point2I"/>.</summary>
		public static explicit operator Point2I(GdiSizeF size) {
			return new Point2I((int) size.Width, (int) size.Height);
		}

		/// <summary>Casts the Gdi <see cref="GdiSize"/> to a <see cref="Point2I"/>.</summary>
		public static implicit operator Point2I(GdiSize size) {
			return new Point2I(size.Width, size.Height);
		}

		/// <summary>Casts the Gdi <see cref="WpfPoint"/> to a <see cref="Point2I"/>.</summary>
		public static explicit operator Point2I(WpfPoint point) {
			return new Point2I((int) point.X, (int) point.Y);
		}

		/// <summary>Casts the Gdi <see cref="WpfSize"/> to a <see cref="Point2I"/>.</summary>
		public static explicit operator Point2I(WpfSize size) {
			return new Point2I((int) size.Width, (int) size.Height);
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		/// <summary>Gets the coordinate at the specified index.</summary>
		public int this[int index] {
			get {
				switch (index) {
				case 0: return X;
				case 1: return Y;
				default: throw new ArgumentOutOfRangeException(nameof(index));
				}
			}
			set {
				switch (index) {
				case 0: X = value; break;
				case 1: Y = value; break;
				default: throw new ArgumentOutOfRangeException(nameof(index));
				}
			}
		}

		/// <summary>Returns true if the point is positioned at (0, 0).</summary>
		public bool IsZero => (X == 0 && Y == 0);

		/// <summary>Returns true if either X or Y is positioned at 0.</summary>
		public bool IsAnyZero => (X == 0 || Y == 0);

		/// <summary>Returns the perpendicular point.</summary>
		public Point2I Perpendicular => new Point2I(-Y, X);
	}
}
