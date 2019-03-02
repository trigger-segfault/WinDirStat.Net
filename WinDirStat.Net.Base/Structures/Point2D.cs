using System;
using System.Runtime.InteropServices;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Structures {
	/// <summary>Point structure for double floating point 2D positions (X, Y).</summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Point2D {
		
		#region Constants

		/// <summary>Returns a point positioned at (0, 0).</summary>
		public static readonly Point2D Zero = new Point2D(0, 0);
		/// <summary>Returns a point positioned at (0.5, 0.5).</summary>
		public static readonly Point2D Half = new Point2D(0.5d, 0.5d);
		/// <summary>Returns a point positioned at (0.5, 0).</summary>
		public static readonly Point2D HalfX = new Point2D(0.5d, 0);
		/// <summary>Returns a point positioned at (0, 0.5).</summary>
		public static readonly Point2D HalfY = new Point2D(0, 0.5d);
		/// <summary>Returns a point positioned at (1, 1).</summary>
		public static readonly Point2D One = new Point2D(1, 1);
		/// <summary>Returns a point positioned at (1, 0).</summary>
		public static readonly Point2D OneX = new Point2D(1, 0);
		/// <summary>Returns a point positioned at (0, 1).</summary>
		public static readonly Point2D OneY = new Point2D(0, 1);

		#endregion

		#region Fields

		/// <summary>X coordinate of this point.</summary>
		public double X;
		/// <summary>Y coordinate of this point.</summary>
		public double Y;

		#endregion

		#region Constructors
		
		/// <summary>Constructs a <see cref="Point2D"/> from the X and Y coordinates.</summary>
		/// <param name="x">The X coordinate to use.</param>
		/// <param name="y">The Y coordinate to use.</param>
		public Point2D(double x, double y) {
			X = x;
			Y = y;
		}

		/// <summary>Constructs a <see cref="Point2D"/> from the same coordinates.</summary>
		/// <param name="uniform">The X and Y coordinate to use.</param>
		public Point2D(double uniform) {
			X = uniform;
			Y = uniform;
		}

		#endregion

		#region Static Constructors

		/// <summary>Constructs a <see cref="Point2D"/> from polar coordinates.</summary>
		/// <param name="length">The length of the polar point.</param>
		/// <param name="radians">The radians of the polar point.</param>
		/// <returns>The constructed <see cref="Point2D"/>.</returns>
		public static Point2D FromPolarRad(double length, double radians) {
			if (length == 0)
				return Zero;
			return new Point2D(
				(double) (length * Math.Cos(radians)),
				(double) (length * Math.Sin(radians)));
		}

		/// <summary>Constructs a <see cref="Point2D"/> from polar coordinates.</summary>
		/// <param name="length">The length of the polar point.</param>
		/// <param name="degrees">The degrees of the polar point.</param>
		/// <returns>The constructed <see cref="Point2D"/>.</returns>
		public static Point2D FromPolarDeg(double length, double degrees) {
			return FromPolarRad(length, MathUtils.DegToRad(degrees));
		}
		
		#endregion

		#region Object Overrides

		/// <summary>Convert to a human-readable string.</summary>
		public override string ToString() => $"(X={X} Y={Y})";

		/// <summary>Returns the hash code of this point.</summary>
		public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

		/// <summary>Checks if the point is equal to the other point.</summary>
		public override bool Equals(object obj) {
			switch (obj) {
			case Point2I pt2i: return this == pt2i;
			case Point2F pt2f: return this == pt2f;
			case Point2D pt2d: return this == pt2d;
			default: return false;
			}
		}
		
		#endregion

		#region Operators

		#region Unary Arithmetic Operators

		public static Point2D operator +(Point2D a) => a;
		public static Point2D operator -(Point2D a) => new Point2D(-a.X, -a.Y);

		public static Point2D operator ++(Point2D a) => new Point2D(++a.X, ++a.Y);
		public static Point2D operator --(Point2D a) => new Point2D(--a.X, --a.Y);

		#endregion

		#region Binary Arithmetic Operators

		public static Point2D operator +(Point2D a, Point2D b) => new Point2D(a.X + b.X, a.Y + b.Y);
		public static Point2D operator +(Point2D a, double b)  => new Point2D(a.X + b,   a.Y + b  );
		public static Point2D operator +(double a, Point2D b)  => new Point2D(a   + b.X, a   + b.Y);

		public static Point2D operator -(Point2D a, Point2D b) => new Point2D(a.X - b.X, a.Y - b.Y);
		public static Point2D operator -(Point2D a, double b)  => new Point2D(a.X - b,   a.Y - b  );
		public static Point2D operator -(double a, Point2D b)  => new Point2D(a   - b.X, a   - b.Y);

		public static Point2D operator *(Point2D a, Point2D b) => new Point2D(a.X * b.X, a.Y * b.Y);
		public static Point2D operator *(Point2D a, double b)  => new Point2D(a.X * b,   a.Y * b  );
		public static Point2D operator *(double a, Point2D b)  => new Point2D(a   * b.X, a   * b.Y);

		public static Point2D operator /(Point2D a, Point2D b) => new Point2D(a.X / b.X, a.Y / b.Y);
		public static Point2D operator /(Point2D a, double b)  => new Point2D(a.X / b,   a.Y / b  );
		public static Point2D operator /(double a, Point2D b)  => new Point2D(a   / b.X, a   / b.Y);

		public static Point2D operator %(Point2D a, Point2D b) => new Point2D(a.X % b.X, a.Y % b.Y);
		public static Point2D operator %(Point2D a, double b)  => new Point2D(a.X % b,   a.Y % b  );
		public static Point2D operator %(double a, Point2D b)  => new Point2D(a   % b.X, a   % b.Y);

		#endregion

		#region Binary Logic Operators

		public static bool operator ==(Point2D a, Point2D b) => (a.X == b.X && a.Y == b.Y);
		public static bool operator ==(Point2D a, double b)  => (a.X == b   && a.Y == b  );
		public static bool operator ==(double a, Point2D b)  => (a   == b.X && a   == b.Y);

		public static bool operator !=(Point2D a, Point2D b) => (a.X != b.X || a.Y != b.Y);
		public static bool operator !=(Point2D a, double b)  => (a.X != b   || a.Y != b  );
		public static bool operator !=(double a, Point2D b)  => (a   != b.X || a   != b.Y);

		public static bool operator <(Point2D a, Point2D b) => (a.X < b.X && a.Y < b.Y);
		public static bool operator <(Point2D a, double b)  => (a.X < b   && a.Y < b  );
		public static bool operator <(double a, Point2D b)  => (a   < b.X && a   < b.Y);

		public static bool operator >(Point2D a, Point2D b) => (a.X > b.X && a.Y > b.Y);
		public static bool operator >(Point2D a, double b)  => (a.X > b   && a.Y > b  );
		public static bool operator >(double a, Point2D b)  => (a   > b.X && a   > b.Y);

		public static bool operator <=(Point2D a, Point2D b) => (a.X <= b.X && a.Y <= b.Y);
		public static bool operator <=(Point2D a, double b)  => (a.X <= b &&   a.Y <= b  );
		public static bool operator <=(double a, Point2D b)  => (a   <= b.X && a   <= b.Y);
		
		public static bool operator >=(Point2D a, Point2D b) => (a.X >= b.X && a.Y >= b.Y);
		public static bool operator >=(Point2D a, double b)   => (a.X >= b   && a.Y >= b  );
		public static bool operator >=(double a, Point2D b)   => (a   >= b.X && a   >= b.Y);

		#endregion

		#endregion

		#region Casting

		/// <summary>Casts the <see cref="Point2I"/> to a <see cref="Point2D"/>.</summary>
		public static implicit operator Point2D(Point2I point) {
			return new Point2D(point.X, point.Y);
		}

		/// <summary>Casts the <see cref="Point2F"/> to a <see cref="Point2D"/>.</summary>
		public static implicit operator Point2D(Point2F point) {
			return new Point2D(point.X, point.Y);
		}

		/// <summary>Casts the <see cref="Point2D"/> to a <see cref="Point2I"/>.</summary>
		public static explicit operator Point2I(Point2D point) {
			return new Point2I((int) point.X, (int) point.Y);
		}

		/// <summary>Casts the <see cref="Point2D"/> to a <see cref="Point2F"/>.</summary>
		public static explicit operator Point2F(Point2D point) {
			return new Point2F((float) point.X, (float) point.Y);
		}

		#endregion

		#region Properties

		/// <summary>Gets or sets the direction of the point in degrees.</summary>
		public double DirectionDeg {
			get {
				if (IsZero)
					return 0;
				return (double) MathUtils.RadToDeg(Math.Atan2(Y, X));
			}
			set {
				if (!IsZero) {
					double length = Length;
					X = (double) (length * Math.Cos(value));
					Y = (double) (length * Math.Sin(value));
				}
			}
		}

		/// <summary>Gets or sets the direction of the point in radians.</summary>
		public double DirectionRad {
			get {
				if (IsZero)
					return 0;
				return (double) Math.Atan2(Y, X);
			}
			set {
				if (!IsZero) {
					double radians = MathUtils.DegToRad(value);
					double length = Length;
					X = (double) (length * Math.Cos(radians));
					Y = (double) (length * Math.Sin(radians));
				}
			}
		}

		/// <summary>Gets or sets the length of the point.</summary>
		public double Length {
			get => (double) Math.Sqrt((X * X) + (Y * Y));
			set {
				if (!IsZero) {
					double oldLength = Length;
					X *= value / oldLength;
					Y *= value / oldLength;
				}
				else {
					X = value;
					Y = 0;
				}
			}
		}

		/// <summary>Gets the squared length of the point.</summary>
		public double LengthSquared => ((X * X) + (Y * Y));

		/// <summary>Gets the coordinate at the specified index.</summary>
		public double this[int index] {
			get {
				switch (index) {
				case 0: return X;
				case 1: return Y;
				default: throw new ArgumentOutOfRangeException(nameof(index));
				}
			}
			set {
				switch (index) {
				case 0: X = value; return;
				case 1: Y = value; return;
				default: throw new ArgumentOutOfRangeException(nameof(index));
				}
			}
		}

		/// <summary>Returns true if the point is positioned at (0, 0).</summary>
		public bool IsZero => (X == 0 && Y == 0);

		/// <summary>Returns true if either X or Y is positioned at 0.</summary>
		public bool IsAnyZero => (X == 0 || Y == 0);

		/// <summary>Returns the perpendicular point.</summary>
		public Point2D Perpendicular => new Point2D(-Y, X);

		#endregion
	}
}
