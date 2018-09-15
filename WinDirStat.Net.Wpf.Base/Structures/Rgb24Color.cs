using System;
using System.Runtime.InteropServices;
using GdiColor = System.Drawing.Color;
using WpfColor = System.Windows.Media.Color;

namespace WinDirStat.Net.Structures {
	/// <summary>An unmanaged 24-bit RGB color.</summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Rgb24Color {
		public static readonly Rgb24Color Black = new Rgb24Color(0, 0, 0);
		public static readonly Rgb24Color White = new Rgb24Color(255, 255, 255);

		/// <summary>The blue channel for the color.</summary>
		public byte B;
		/// <summary>The green channel for the color.</summary>
		public byte G;
		/// <summary>The red channel for the color.</summary>
		public byte R;

		/// <summary>Constructs the <see cref="Rgb24Color"/>.</summary>
		/// <param name="red">The red channel.</param>
		/// <param name="green">The green channel.</param>
		/// <param name="blue">The blue channel.</param>
		public Rgb24Color(int red, int green, int blue) {
			R = (byte) red;
			G = (byte) green;
			B = (byte) blue;
		}

		/// <summary>Casts the <see cref="Rgb24Color"/> to a <see cref="GdiColor"/>.</summary>
		public static explicit operator GdiColor(Rgb24Color color) {
			return GdiColor.FromArgb(color.R, color.G, color.B);
		}

		/// <summary>Casts the <see cref="Rgb24Color"/> to a <see cref="WpfColor"/>.</summary>
		public static explicit operator WpfColor(Rgb24Color color) {
			return WpfColor.FromRgb(color.R, color.G, color.B);
		}

		/// <summary>Casts the <see cref="GdiColor"/> to a <see cref="Rgb24Color"/>.</summary>
		public static implicit operator Rgb24Color(GdiColor color) {
			return new Rgb24Color(color.R, color.G, color.B);
		}

		/// <summary>Casts the <see cref="WpfColor"/> to a <see cref="Rgb24Color"/>.</summary>
		public static implicit operator Rgb24Color(WpfColor color) {
			return new Rgb24Color(color.R, color.G, color.B);
		}
	}
}
