using System;
using System.Runtime.InteropServices;

namespace WinDirStat.Net.Structures {
	/// <summary>An unmanaged 24-bit RGB color.</summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Rgb24Color {

		#region Constants

		public static readonly Rgb24Color Black = new Rgb24Color(0, 0, 0);
		public static readonly Rgb24Color White = new Rgb24Color(255, 255, 255);

		#endregion

		#region Fields

		/// <summary>The blue channel for the color.</summary>
		public byte B;
		/// <summary>The green channel for the color.</summary>
		public byte G;
		/// <summary>The red channel for the color.</summary>
		public byte R;

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="Rgb24Color"/>.</summary>
		/// <param name="red">The red channel.</param>
		/// <param name="green">The green channel.</param>
		/// <param name="blue">The blue channel.</param>
		public Rgb24Color(int red, int green, int blue) {
			R = (byte) red;
			G = (byte) green;
			B = (byte) blue;
		}

		#endregion
		
		#region Properties

		public int PackedValue {
			get => (B << 16) | (G << 8) | R;
			set {
				B = (byte) ((value >> 16) & 0xFF);
				G = (byte) ((value >> 8)  & 0xFF);
				R = (byte) ((value)       & 0xFF);
			}
		}

		#endregion
	}
}
