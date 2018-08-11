using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Utils {
	public static class ByteFormatter {
		public static readonly ReadOnlyCollection<string> Abbreviations =
			Array.AsReadOnly(new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" });

		public const string SingleFormat = "N0";
		public const string MultiFormat = "N1";

		public static string Format(int bytes) {
			return Format(Convert.ToDouble(bytes));
		}
		public static string Format(uint bytes) {
			return Format(Convert.ToDouble(bytes));
		}
		public static string Format(long bytes) {
			return Format(Convert.ToDouble(bytes));
		}
		public static string Format(ulong bytes) {
			return Format(Convert.ToDouble(bytes));
		}
		public static string Format(decimal bytes) {
			return Format(Convert.ToDouble(bytes));
		}

		public static string Format(double bytes) {
			return Format(bytes, SingleFormat, MultiFormat);
		}
		public static string Format(double bytes, string singleFormat, string multiFormat) {
			int order;
			for (order = 0; order < Abbreviations.Count && bytes >= 1024; order++) {
				bytes /= 1024;
			}

			// Don't show decimal places for Bytes
			string format = (order == 0 ? singleFormat : multiFormat);
			
			return string.Format("{0:" + format + "} {1}", bytes, Abbreviations[order]);
		}
	}
}
