using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Utils {
	/// <summary>Simple additional math functions.</summary>
	public static class MathUtils {

		/// <summary>Converts degrees to radians.</summary>
		/// 
		/// <param name="degrees">The degress to convert.</param>
		/// <returns>The <paramref name="degrees"/> as radians.</returns>
		public static float DegToRad(float degrees) {
			return degrees * (float) Math.PI / 180f;
		}

		/// <summary>Converts degrees to radians.</summary>
		/// 
		/// <param name="degrees">The degress to convert.</param>
		/// <returns>The <paramref name="degrees"/> as radians.</returns>
		public static double DegToRad(double degrees) {
			return degrees * Math.PI / 180d;
		}

		/// <summary>Converts radians to degrees.</summary>
		/// 
		/// <param name="radians">The radians to convert.</param>
		/// <returns>The <paramref name="radians"/> as degrees.</returns>
		public static float RadToDeg(float radians) {
			return radians / (float) Math.PI * 180f;
		}

		/// <summary>Converts radians to degrees.</summary>
		/// 
		/// <param name="radians">The radians to convert.</param>
		/// <returns>The <paramref name="radians"/> as degrees.</returns>
		public static double RadToDeg(double radians) {
			return radians / Math.PI * 180d;
		}

		/// <summary>Rounds the value and returns it as an integer.</summary>
		/// 
		/// <param name="x">The value to round.</param>
		/// <returns>The rounded value as an integer.</returns>
		public static int Round(float x) {
			return Math.Sign(x) * (int) (Math.Abs(x) + 0.5f);
		}

		/// <summary>Rounds the value and returns it as an integer.</summary>
		/// 
		/// <param name="x">The value to round.</param>
		/// <returns>The rounded value as an integer.</returns>
		public static int Round(double x) {
			return Math.Sign(x) * (int) (Math.Abs(x) + 0.5);
		}
		
		/// <summary>Clamps the <see cref="IComparable{T}"/> values.</summary>
		/// 
		/// <typeparam name="T">The type to compare.</typeparam>
		/// <param name="value">The value to clamp.</param>
		/// <param name="min">The minimum value (has priority over <paramref name="max"/>).</param>
		/// <param name="max">The maximum value.</param>
		/// <returns>The value between <paramref name="min"/> and <paramref name="max"/>.</returns>
		public static T Clamp<T>(T value, T min, T max) where T : IComparable<T> {
			if (value.CompareTo(min) < 0)
				return min;
			else if (value.CompareTo(max) > 0)
				return max;
			else
				return value;
		}
	}
}
