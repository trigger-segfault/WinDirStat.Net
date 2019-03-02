using System;
using System.Runtime.InteropServices;

namespace WinDirStat.Net.Structures {
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Rectangle2S {

		#region Constants

		/// <summary>Returns a rectangle positioned at (0, 0) with a size of (0, 0).</summary>
		public static readonly Rectangle2S Empty = new Rectangle2S(0, 0, 0, 0);
		/// <summary>Returns a rectangle positioned at (0, 0) with a size of (1, 1).</summary>
		public static readonly Rectangle2S One = new Rectangle2S(0, 0, 1, 1);
		/// <summary>
		/// Returns a rectangle positioned at (<see cref="ushort.MaxValue"/>, <see cref="ushort.MaxValue"/>)
		/// with a size of (0, 0).
		/// </summary>
		public static readonly Rectangle2S Invalid = new Rectangle2S(ushort.MaxValue, ushort.MaxValue, 0, 0);

		#endregion

		#region Fields

		/// <summary>X coordinate of this rectangle.</summary>
		public ushort X;
		/// <summary>Y coordinate of this rectangle.</summary>
		public ushort Y;
		/// <summary>Width of this rectangle.</summary>
		public ushort Width;
		/// <summary>Height of this rectangle.</summary>
		public ushort Height;

		#endregion

		#region Constructors

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

		#endregion

		#region Static Constructors

		public static Rectangle2S FromLTRB(int left, int top, int right, int bottom) {
			return new Rectangle2S(left, top, right - left, bottom - top);
		}

		#endregion
	}
}
