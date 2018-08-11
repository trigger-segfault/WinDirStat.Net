using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Utils {
	public static class MathUtils {

		public static float DegToRad(float degrees) {
			return degrees * (float) Math.PI / 180f;
		}

		public static double DegToRad(double degrees) {
			return degrees * Math.PI / 180d;
		}

		public static float RadToDeg(float radians) {
			return radians / (float) Math.PI * 180f;
		}

		public static double RadToDeg(double radians) {
			return radians / Math.PI * 180d;
		}

		public static int Round(float f) {
			return Math.Sign(f) * (int) (Math.Abs(f) + 0.5f);
		}

		public static int Round(double d) {
			return Math.Sign(d) * (int) (Math.Abs(d) + 0.5);
		}
	}
}
