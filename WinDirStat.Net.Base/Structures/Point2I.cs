using System;
using System.Runtime.InteropServices;

namespace WinDirStat.Net.Structures {
	/// <summary>Point structure for integer 2D positions (X, Y).</summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Point2I {
		
		#region Constants

		/// <summary>Returns a point positioned at (0, 0).</summary>
		public static readonly Point2I Zero = new Point2I(0, 0);
		/// <summary>Returns a point positioned at (1, 1).</summary>
		public static readonly Point2I One = new Point2I(1, 1);
		/// <summary>Returns a point positioned at (1, 0).</summary>
		public static readonly Point2I OneX = new Point2I(1, 0);
		/// <summary>Returns a point positioned at (0, 1).</summary>
		public static readonly Point2I OneY = new Point2I(0, 1);
		
		#endregion

		#region Fields

		/// <summary>X coordinate of this point.</summary>
		public int X;
		/// <summary>Y coordinate of this point.</summary>
		public int Y;
		
		#endregion
		
		#region Constructors

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

		public static Point2I operator +(Point2I a) => a;
		public static Point2I operator -(Point2I a) => new Point2I(-a.X, -a.Y);

		public static Point2I operator ++(Point2I a) => new Point2I(++a.X, ++a.Y);
		public static Point2I operator --(Point2I a) => new Point2I(--a.X, --a.Y);

		#endregion

		#region Binary Arithmetic Operators

		public static Point2I operator +(Point2I a, Point2I b) => new Point2I(a.X + b.X, a.Y + b.Y);
		public static Point2I operator +(Point2I a, int b)     => new Point2I(a.X + b,   a.Y + b  );
		public static Point2I operator +(int a, Point2I b)     => new Point2I(a   + b.X, a   + b.Y);

		public static Point2I operator -(Point2I a, Point2I b) => new Point2I(a.X - b.X, a.Y - b.Y);
		public static Point2I operator -(Point2I a, int b)     => new Point2I(a.X - b,   a.Y - b  );
		public static Point2I operator -(int a, Point2I b)     => new Point2I(a   - b.X, a   - b.Y);

		public static Point2I operator *(Point2I a, Point2I b) => new Point2I(a.X * b.X, a.Y * b.Y);
		public static Point2I operator *(Point2I a, int b)     => new Point2I(a.X * b,   a.Y * b  );
		public static Point2I operator *(int a, Point2I b)     => new Point2I(a   * b.X, a   * b.Y);

		public static Point2I operator /(Point2I a, Point2I b) => new Point2I(a.X / b.X, a.Y / b.Y);
		public static Point2I operator /(Point2I a, int b)     => new Point2I(a.X / b,   a.Y / b  );
		public static Point2I operator /(int a, Point2I b)     => new Point2I(a   / b.X, a   / b.Y);

		public static Point2I operator %(Point2I a, Point2I b) => new Point2I(a.X % b.X, a.Y % b.Y);
		public static Point2I operator %(Point2I a, int b)     => new Point2I(a.X % b,   a.Y % b  );
		public static Point2I operator %(int a, Point2I b)     => new Point2I(a   % b.X, a   % b.Y);

		#endregion

		#region Binary Logic Operators

		public static bool operator ==(Point2I a, Point2I b) => (a.X == b.X && a.Y == b.Y);
		public static bool operator ==(Point2I a, int b)     => (a.X == b   && a.Y == b  );
		public static bool operator ==(int a, Point2I b)     => (a   == b.X && a   == b.Y);

		public static bool operator !=(Point2I a, Point2I b) => (a.X != b.X || a.Y != b.Y);
		public static bool operator !=(Point2I a, int b)     => (a.X != b   || a.Y != b  );
		public static bool operator !=(int a, Point2I b)     => (a   != b.X || a   != b.Y);

		public static bool operator <(Point2I a, Point2I b) => (a.X < b.X && a.Y < b.Y);
		public static bool operator <(Point2I a, int b)     => (a.X < b   && a.Y < b  );
		public static bool operator <(int a, Point2I b)     => (a   < b.X && a   < b.Y);

		public static bool operator >(Point2I a, Point2I b) => (a.X > b.X && a.Y > b.Y);
		public static bool operator >(Point2I a, int b)     => (a.X > b   && a.Y > b  );
		public static bool operator >(int a, Point2I b)     => (a   > b.X && a   > b.Y);

		public static bool operator <=(Point2I a, Point2I b) => (a.X <= b.X && a.Y <= b.Y);
		public static bool operator <=(Point2I a, int b)     => (a.X <= b &&   a.Y <= b  );
		public static bool operator <=(int a, Point2I b)     => (a   <= b.X && a   <= b.Y);
		
		public static bool operator >=(Point2I a, Point2I b) => (a.X >= b.X && a.Y >= b.Y);
		public static bool operator >=(Point2I a, int b)     => (a.X >= b   && a.Y >= b  );
		public static bool operator >=(int a, Point2I b)     => (a   >= b.X && a   >= b.Y);

		#endregion

		#endregion

		#region Properties

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
		public Point2I Perpendicular => new Point2I(-Y, X);

		#endregion
	}
}
