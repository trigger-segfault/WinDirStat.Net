using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Settings.Geometry;
using GdiColor = System.Drawing.Color;
using WpfColor = System.Windows.Media.Color;

namespace WinDirStat.Net.Drawing {
	public static class ColorSpace {

		public static float GetBrightness(Rgb24Color color) {
			return (color.R + color.G + color.B) / 255f / 3f;
		}

		public static float GetBrightness(Rgba32Color color) {
			return (color.R + color.G + color.B) / 255f / 3f;
		}

		public static float GetBrightness(GdiColor color) {
			return (color.R + color.G + color.B) / 255f / 3f;
		}

		public static Rgb24Color SetBrightness(Rgb24Color color, float brightness) {
			Debug.Assert(brightness >= 0f);
			Debug.Assert(brightness <= 1f);

			float redf = color.R / 255f;
			float greenf = color.G / 255f;
			float bluef = color.B / 255f;

			float f = 3f * brightness / (redf + greenf + bluef);
			redf *= f;
			greenf *= f;
			bluef *= f;

			int red = (int) (redf * 255);
			int green = (int) (greenf * 255);
			int blue = (int) (bluef * 255);

			NormalizeColor(ref red, ref green, ref blue);

			return new Rgb24Color(red, green, blue);
		}

		public static Rgba32Color SetBrightness(Rgba32Color color, float brightness) {
			Debug.Assert(brightness >= 0f);
			Debug.Assert(brightness <= 1f);

			float redf = color.R / 255f;
			float greenf = color.G / 255f;
			float bluef = color.B / 255f;

			float f = 3f * brightness / (redf + greenf + bluef);
			redf *= f;
			greenf *= f;
			bluef *= f;

			int red = (int) (redf * 255);
			int green = (int) (greenf * 255);
			int blue = (int) (bluef * 255);

			NormalizeColor(ref red, ref green, ref blue);

			return new Rgba32Color(red, green, blue);
		}

		public static GdiColor SetBrightness(GdiColor color, float brightness) {
			Debug.Assert(brightness >= 0f);
			Debug.Assert(brightness <= 1f);

			float redf = color.R / 255f;
			float greenf = color.G / 255f;
			float bluef = color.B / 255f;

			float f = 3f * brightness / (redf + greenf + bluef);
			redf *= f;
			greenf *= f;
			bluef *= f;

			int red = (int) (redf * 255);
			int green = (int) (greenf * 255);
			int blue = (int) (bluef * 255);

			NormalizeColor(ref red, ref green, ref blue);

			return GdiColor.FromArgb(red, green, blue);
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
