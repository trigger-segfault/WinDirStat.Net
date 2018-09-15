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
	/// <summary>Point structure for floating point 2D positions (X, Y).</summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Point2F {

		//-----------------------------------------------------------------------------
		// Constants
		//-----------------------------------------------------------------------------

		/// <summary>Returns a point positioned at (0, 0).</summary>
		public static readonly Point2F Zero = new Point2F(0f, 0f);
		/// <summary>Returns a point positioned at (0.5, 0.5).</summary>
		public static readonly Point2F Half = new Point2F(0.5f, 0.5f);
		/// <summary>Returns a point positioned at (0.5, 0).</summary>
		public static readonly Point2F HalfX = new Point2F(0.5f, 0f);
		/// <summary>Returns a point positioned at (0, 0.5).</summary>
		public static readonly Point2F HalfY = new Point2F(0f, 0.5f);
		/// <summary>Returns a point positioned at (1, 1).</summary>
		public static readonly Point2F One = new Point2F(1f, 1f);
		/// <summary>Returns a point positioned at (1, 0).</summary>
		public static readonly Point2F OneX = new Point2F(1f, 0f);
		/// <summary>Returns a point positioned at (0, 1).</summary>
		public static readonly Point2F OneY = new Point2F(0f, 1f);


		//-----------------------------------------------------------------------------
		// Members
		//-----------------------------------------------------------------------------

		/// <summary>X coordinate of this point.</summary>
		public float X;
		/// <summary>Y coordinate of this point.</summary>
		public float Y;


		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------

		/// <summary>Constructs a <see cref="Point2F"/> from the X and Y coordinates.</summary>
		/// <param name="x">The X coordinate to use.</param>
		/// <param name="y">The Y coordinate to use.</param>
		public Point2F(float x, float y) {
			X = x;
			Y = y;
		}

		/// <summary>Constructs a <see cref="Point2F"/> from the same coordinates.</summary>
		/// <param name="uniform">The X and Y coordinate to use.</param>
		public Point2F(float uniform) {
			X = uniform;
			Y = uniform;
		}

		/// <summary>Constructs a <see cref="Point2F"/> from polar coordinates.</summary>
		/// <param name="length">The length of the polar point.</param>
		/// <param name="radians">The radians of the polar point.</param>
		/// <returns>The constructed <see cref="Point2F"/>.</returns>
		public static Point2F FromPolarRad(float length, float radians) {
			if (length == 0)
				return Point2F.Zero;
			return new Point2F(
				(float) (length * Math.Cos(radians)),
				(float) (length * Math.Sin(radians)));
		}

		/// <summary>Constructs a <see cref="Point2F"/> from polar coordinates.</summary>
		/// <param name="length">The length of the polar point.</param>
		/// <param name="degrees">The degrees of the polar point.</param>
		/// <returns>The constructed <see cref="Point2F"/>.</returns>
		public static Point2F FromPolarDeg(float length, float degrees) {
			return FromPolarRad(length, (float) MathUtils.DegToRad(degrees));
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

		public static Point2F operator +(Point2F a) => a;

		public static Point2F operator -(Point2F a) => new Point2F(-a.X, -a.Y);

		public static Point2F operator ++(Point2F a) => new Point2F(++a.X, ++a.Y);

		public static Point2F operator --(Point2F a) => new Point2F(--a.X, --a.Y);

		//--------------------------------

		public static Point2F operator +(Point2F a, Point2F b) {
			return new Point2F(a.X + b.X, a.Y + b.Y);
		}

		public static Point2F operator +(Point2F a, float b) {
			return new Point2F(a.X + b, a.Y + b);
		}

		public static Point2F operator +(float a, Point2F b) {
			return new Point2F(a + b.X, a + b.Y);
		}


		public static Point2F operator -(Point2F a, Point2F b) {
			return new Point2F(a.X - b.X, a.Y - b.Y);
		}

		public static Point2F operator -(Point2F a, float b) {
			return new Point2F(a.X - b, a.Y - b);
		}

		public static Point2F operator -(float a, Point2F b) {
			return new Point2F(a - b.X, a - b.Y);
		}


		public static Point2F operator *(Point2F a, Point2F b) {
			return new Point2F(a.X * b.X, a.Y * b.Y);
		}

		public static Point2F operator *(float a, Point2F b) {
			return new Point2F(a * b.X, a * b.Y);
		}

		public static Point2F operator *(Point2F a, float b) {
			return new Point2F(a.X * b, a.Y * b);
		}


		public static Point2F operator /(Point2F a, Point2F b) {
			return new Point2F(a.X / b.X, a.Y / b.Y);
		}

		public static Point2F operator /(float a, Point2F b) {
			return new Point2F(a / b.X, a / b.Y);
		}

		public static Point2F operator /(Point2F a, float b) {
			return new Point2F(a.X / b, a.Y / b);
		}


		public static Point2F operator %(Point2F a, Point2F b) {
			return new Point2F(a.X % b.X, a.Y % b.Y);
		}

		public static Point2F operator %(float a, Point2F b) {
			return new Point2F(a % b.X, a % b.Y);
		}

		public static Point2F operator %(Point2F a, float b) {
			return new Point2F(a.X % b, a.Y % b);
		}

		//--------------------------------

		public static bool operator ==(Point2F a, Point2F b) {
			return (a.X == b.X && a.Y == b.Y);
		}

		public static bool operator ==(float a, Point2F b) {
			return (a == b.X && a == b.Y);
		}

		public static bool operator ==(Point2F a, float b) {
			return (a.X == b && a.Y == b);
		}

		public static bool operator !=(Point2F a, Point2F b) {
			return (a.X != b.X || a.Y != b.Y);
		}

		public static bool operator !=(float a, Point2F b) {
			return (a != b.X || a != b.Y);
		}

		public static bool operator !=(Point2F a, float b) {
			return (a.X != b || a.Y != b);
		}

		public static bool operator <(Point2F a, Point2F b) {
			return (a.X < b.X && a.Y < b.Y);
		}

		public static bool operator <(float a, Point2F b) {
			return (a < b.X && a < b.Y);
		}

		public static bool operator <(Point2F a, float b) {
			return (a.X < b && a.Y < b);
		}

		public static bool operator >(Point2F a, Point2F b) {
			return (a.X > b.X && a.Y > b.Y);
		}

		public static bool operator >(float a, Point2F b) {
			return (a > b.X && a > b.Y);
		}

		public static bool operator >(Point2F a, float b) {
			return (a.X > b && a.Y > b);
		}

		public static bool operator <=(Point2F a, Point2F b) {
			return (a.X <= b.X && a.Y <= b.Y);
		}

		public static bool operator <=(float a, Point2F b) {
			return (a <= b.X && a <= b.Y);
		}

		public static bool operator <=(Point2F a, float b) {
			return (a.X <= b && a.Y <= b);
		}

		public static bool operator >=(Point2F a, Point2F b) {
			return (a.X >= b.X && a.Y >= b.Y);
		}

		public static bool operator >=(float a, Point2F b) {
			return (a >= b.X && a >= b.Y);
		}

		public static bool operator >=(Point2F a, float b) {
			return (a.X >= b && a.Y >= b);
		}


		//-----------------------------------------------------------------------------
		// Casting
		//-----------------------------------------------------------------------------

		/// <summary>Casts the <see cref="Point2I"/> to a <see cref="Point2F"/>.</summary>
		public static implicit operator Point2F(Point2I point) {
			return new Point2F(point.X, point.Y);
		}

		/// <summary>Casts the <see cref="Point2F"/> to a <see cref="Point2I"/>.</summary>
		public static explicit operator Point2I(Point2F point) {
			return new Point2I((int) point.X, (int) point.Y);
		}

		//--------------------------------

		/// <summary>Casts the <see cref="Point2F"/> to a Gdi <see cref="GdiPointF"/>.</summary>
		public static explicit operator GdiPointF(Point2F point) {
			return new GdiPointF(point.X, point.Y);
		}

		/// <summary>Casts the <see cref="Point2F"/> to a Gdi <see cref="GdiPoint"/>.</summary>
		public static explicit operator GdiPoint(Point2F point) {
			return new GdiPoint((int) point.X, (int) point.Y);
		}

		/// <summary>Casts the <see cref="Point2F"/> to a Gdi <see cref="GdiSizeF"/>.</summary>
		public static explicit operator GdiSizeF(Point2F point) {
			return new GdiSizeF(point.X, point.Y);
		}

		/// <summary>Casts the <see cref="Point2F"/> to a Gdi <see cref="GdiSize"/>.</summary>
		public static explicit operator GdiSize(Point2F point) {
			return new GdiSize((int) point.X, (int) point.Y);
		}

		/// <summary>Casts the <see cref="Point2F"/> to a Wpf <see cref="WpfPoint"/>.</summary>
		public static explicit operator WpfPoint(Point2F point) {
			return new WpfPoint(point.X, point.Y);
		}

		/// <summary>Casts the <see cref="Point2F"/> to a Wpf <see cref="WpfSize"/>.</summary>
		public static explicit operator WpfSize(Point2F point) {
			return new WpfSize(point.X, point.Y);
		}

		/// <summary>Casts the Gdi <see cref="GdiPointF"/> to a <see cref="Point2F"/>.</summary>
		public static implicit operator Point2F(GdiPointF point) {
			return new Point2F(point.X, point.Y);
		}

		/// <summary>Casts the Gdi <see cref="GdiPoint"/> to a <see cref="Point2F"/>.</summary>
		public static implicit operator Point2F(GdiPoint point) {
			return new Point2F(point.X, point.Y);
		}

		/// <summary>Casts the Gdi <see cref="GdiSizeF"/> to a <see cref="Point2F"/>.</summary>
		public static implicit operator Point2F(GdiSizeF size) {
			return new Point2F(size.Width, size.Height);
		}

		/// <summary>Casts the Gdi <see cref="GdiSize"/> to a <see cref="Point2F"/>.</summary>
		public static implicit operator Point2F(GdiSize size) {
			return new Point2F(size.Width, size.Height);
		}

		/// <summary>Casts the Gdi <see cref="WpfPoint"/> to a <see cref="Point2F"/>.</summary>
		public static implicit operator Point2F(WpfPoint point) {
			return new Point2F((float) point.X, (float) point.Y);
		}

		/// <summary>Casts the Gdi <see cref="WpfSize"/> to a <see cref="Point2F"/>.</summary>
		public static implicit operator Point2F(WpfSize size) {
			return new Point2F((float) size.Width, (float) size.Height);
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		/// <summary>Gets or sets the direction of the point in degrees.</summary>
		public float DirectionDeg {
			get {
				if (IsZero)
					return 0f;
				return (float) MathUtils.RadToDeg(Math.Atan2(Y, X));
			}
			set {
				if (!IsZero) {
					float length = Length;
					X = (float) (length * Math.Cos(value));
					Y = (float) (length * Math.Sin(value));
				}
			}
		}

		/// <summary>Gets or sets the direction of the point in radians.</summary>
		public float DirectionRad {
			get {
				if (IsZero)
					return 0f;
				return (float) Math.Atan2(Y, X);
			}
			set {
				if (!IsZero) {
					double radians = MathUtils.DegToRad(value);
					float length = Length;
					X = (float) (length * Math.Cos(radians));
					Y = (float) (length * Math.Sin(radians));
				}
			}
		}

		/// <summary>Gets or sets the length of the point.</summary>
		public float Length {
			get => (float) Math.Sqrt((X * X) + (Y * Y));
			set {
				if (!IsZero) {
					float oldLength = Length;
					X *= value / oldLength;
					Y *= value / oldLength;
				}
				else {
					X = value;
					Y = 0f;
				}
			}
		}

		/// <summary>Gets the squared length of the point.</summary>
		public float LengthSquared {
			get { return ((X * X) + (Y * Y)); }
		}

		/// <summary>Gets the coordinate at the specified index.</summary>
		public float this[int index] {
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
		public bool IsZero => (X == 0f && Y == 0f);

		/// <summary>Returns true if either X or Y is positioned at 0.</summary>
		public bool IsAnyZero => (X == 0f || Y == 0f);

		/// <summary>Returns the perpendicular point.</summary>
		public Point2F Perpendicular => new Point2F(-Y, X);
	}
}
