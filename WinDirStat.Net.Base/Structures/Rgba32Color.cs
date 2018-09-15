using System;
using System.Runtime.InteropServices;
using WinDirStat.Net.Utils;
using GdiColor = System.Drawing.Color;
using WpfColor = System.Windows.Media.Color;

namespace WinDirStat.Net.Structures {
	

	/// <summary>An unmanaged 32-bit RGBA color with.</summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Rgba32Color {
		
		public static readonly Rgba32Color Transparent = new Rgba32Color(0, 0, 0, 0);
		public static readonly Rgba32Color Black = new Rgba32Color(0, 0, 0);
		public static readonly Rgba32Color White = new Rgba32Color(255, 255, 255);

		/// <summary>The blue channel for the color.</summary>
		public byte B;
		/// <summary>The green channel for the color.</summary>
		public byte G;
		/// <summary>The red channel for the color.</summary>
		public byte R;
		/// <summary>The alpha channel for the color.</summary>
		public byte A;

		/// <summary>Constructs the <see cref="Rgba32Color"/> with an alpha of 255.</summary>
		/// <param name="red">The red channel.</param>
		/// <param name="green">The green channel.</param>
		/// <param name="blue">The blue channel.</param>
		public Rgba32Color(int red, int green, int blue) {
			R = (byte) red;
			G = (byte) green;
			B = (byte) blue;
			A = byte.MaxValue;
		}

		/// <summary>Constructs the <see cref="Rgba32Color"/>.</summary
		/// <param name="red">The red channel.</param>
		/// <param name="green">The green channel.</param>
		/// <param name="blue">The blue channel.</param>
		/// <param name="alpha">The alpha channel.</param>
		public Rgba32Color(int red, int green, int blue, int alpha) {
			R = (byte) red;
			G = (byte) green;
			B = (byte) blue;
			A = (byte) alpha;
		}

		/// <summary>Constructs the (0.0-1.0) <see cref="Rgba32Color"/> with an alpha of 1.0.</summary>
		/// <param name="red">The red channel.</param>
		/// <param name="green">The green channel.</param>
		/// <param name="blue">The blue channel.</param>
		public Rgba32Color(float red, float green, float blue) {
			R = (byte) MathUtils.Round(red * 255);
			G = (byte) MathUtils.Round(green * 255);
			B = (byte) MathUtils.Round(blue * 255);
			A = byte.MaxValue;
		}

		/// <summary>Constructs the (0.0-1.0) <see cref="Rgba32Color"/>.</summary>
		/// <param name="red">The red channel.</param>
		/// <param name="green">The green channel.</param>
		/// <param name="blue">The blue channel.</param>
		public Rgba32Color(float red, float green, float blue, float alpha) {
			R = (byte) MathUtils.Round(red * 255);
			G = (byte) MathUtils.Round(green * 255);
			B = (byte) MathUtils.Round(blue * 255);
			A = (byte) MathUtils.Round(alpha * 255);
		}

		/// <summary>Constructs the (0.0-1.0) <see cref="Rgba32Color"/> with an alpha of 1.0.</summary>
		/// <param name="red">The red channel.</param>
		/// <param name="green">The green channel.</param>
		/// <param name="blue">The blue channel.</param>
		public Rgba32Color(double red, double green, double blue) {
			R = (byte) MathUtils.Round(red * 255);
			G = (byte) MathUtils.Round(green * 255);
			B = (byte) MathUtils.Round(blue * 255);
			A = byte.MaxValue;
		}

		/// <summary>Constructs the (0.0-1.0) <see cref="Rgba32Color"/>.</summary>
		/// <param name="red">The red channel.</param>
		/// <param name="green">The green channel.</param>
		/// <param name="blue">The blue channel.</param>
		public Rgba32Color(double red, double green, double blue, double alpha) {
			R = (byte) MathUtils.Round(red * 255);
			G = (byte) MathUtils.Round(green * 255);
			B = (byte) MathUtils.Round(blue * 255);
			A = (byte) MathUtils.Round(alpha * 255);
		}

		/// <summary>Constructs the (0.0-1.0) <see cref="Rgba32Color"/>.</summary>
		/// <param name="red">The red channel.</param>
		/// <param name="green">The green channel.</param>
		/// <param name="blue">The blue channel.</param>
		public Rgba32Color(int rgba) {
			B = (byte) ((rgba >> 24) & 0xFF);
			G = (byte) ((rgba >> 16) & 0xFF);
			R = (byte) ((rgba >> 8)  & 0xFF);
			A = (byte) ((rgba)       & 0xFF);
		}

		public int PackedValue {
			get => (B << 24) | (G << 16) | (R << 8) | A;
			set {
				B = (byte) ((value >> 24) & 0xFF);
				G = (byte) ((value >> 16) & 0xFF);
				R = (byte) ((value >> 8)  & 0xFF);
				A = (byte) ((value)       & 0xFF);
			}
		}

		/// <summary>Casts the <see cref="Rgba32Color"/> to a <see cref="GdiColor"/>.</summary>
		public static explicit operator GdiColor(Rgba32Color color) {
			return GdiColor.FromArgb(color.A, color.R, color.G, color.B);
		}

		/// <summary>Casts the <see cref="Rgba32Color"/> to a <see cref="WpfColor"/>.</summary>
		public static explicit operator WpfColor(Rgba32Color color) {
			return WpfColor.FromArgb(color.A, color.R, color.G, color.B);
		}

		/// <summary>Casts the <see cref="Rgba32Color"/> to an <see cref="Rgb24Color"/>.</summary>
		public static explicit operator Rgb24Color(Rgba32Color color) {
			return new Rgb24Color(color.R, color.G, color.B);
		}

		/// <summary>Casts the <see cref="GdiColor"/> to a <see cref="Rgba32Color"/>.</summary>
		public static implicit operator Rgba32Color(GdiColor color) {
			return new Rgba32Color(color.R, color.G, color.B, color.A);
		}

		/// <summary>Casts the <see cref="WpfColor"/> to a <see cref="Rgba32Color"/>.</summary>
		public static implicit operator Rgba32Color(WpfColor color) {
			return new Rgba32Color(color.R, color.G, color.B, color.A);
		}

		/// <summary>Casts the <see cref="Rgb24Color"/> to a <see cref="Rgba32Color"/>.</summary>
		public static implicit operator Rgba32Color(Rgb24Color color) {
			return new Rgba32Color(color.R, color.G, color.B);
		}
	}
}
