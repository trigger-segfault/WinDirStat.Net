using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Structures;

#if DOUBLE
using Number = System.Double;
#else
using Number = System.Single;
#endif

namespace WinDirStat.Net.Rendering {
	public static class ColorSpace {
		
		public const Number PaletteBrightness = (Number) 0.6;

		public static float GetBrightness(Rgb24Color color) {
			return (color.R + color.G + color.B) / (Number) 255 / 3;
		}

		public static Number GetBrightness(Rgba32Color color) {
			return (color.R + color.G + color.B) / (Number) 255 / 3;
		}

		public static Rgb24Color SetBrightness(Rgb24Color color, float brightness) {
			Debug.Assert(brightness >= 0);
			Debug.Assert(brightness <= 1);

			Number redf		= color.R / 255f;
			Number greenf	= color.G / 255f;
			Number bluef	= color.B / 255f;

			Number f = 3 * brightness / (redf + greenf + bluef);

			int red		= (int) (redf * f * 255);
			int green	= (int) (greenf * f * 255);
			int blue	= (int) (bluef * f * 255);

			NormalizeColor(ref red, ref green, ref blue);

			return new Rgb24Color(red, green, blue);
		}

		public static Rgba32Color SetBrightness(Rgba32Color color, float brightness) {
			Debug.Assert(brightness >= 0);
			Debug.Assert(brightness <= 1);

			Number redf		= color.R / 255f;
			Number greenf	= color.G / 255f;
			Number bluef	= color.B / 255f;

			Number f = 3 * brightness / (redf + greenf + bluef);

			int red		= (int) (redf * f * 255);
			int green	= (int) (greenf * f * 255);
			int blue	= (int) (bluef * f * 255);

			NormalizeColor(ref red, ref green, ref blue);

			return new Rgba32Color(red, green, blue, color.A);
		}

		public static Rgba32Color[] EqualizeColors(IEnumerable<Rgba32Color> colors) {
			List<Rgba32Color> result = new List<Rgba32Color>();
			foreach (Rgba32Color color in colors)
				result.Add(SetBrightness(color, PaletteBrightness));
			return result.ToArray();
		}

		public static Rgb24Color[] EqualizeColors(IEnumerable<Rgb24Color> colors) {
			List<Rgb24Color> result = new List<Rgb24Color>();
			foreach (Rgb24Color color in colors)
				result.Add(SetBrightness(color, PaletteBrightness));
			return result.ToArray();
		}

		public static void NormalizeColor(ref int red, ref int green, ref int blue) {
			Debug.Assert(red + green + blue <= 3 * 255);

			if (red > 255) {
				DistributeFirst(ref red, ref green, ref blue);
			}
			else if (green > 255) {
				DistributeFirst(ref green, ref red, ref blue);
			}
			else if (blue > 255) {
				DistributeFirst(ref blue, ref red, ref green);
			}
		}

		private static void DistributeFirst(ref int first, ref int second, ref int third) {
			int h = (first - 255) / 2;
			first = 255;
			second += h;
			third += h;

			if (second > 255) {
				h = second - 255;
				second = 255;
				third += h;
				Debug.Assert(third <= 255);
			}
			else if (third > 255) {
				h = third - 255;
				third = 255;
				second += h;
				Debug.Assert(second <= 255);
			}
		}
	}
}
