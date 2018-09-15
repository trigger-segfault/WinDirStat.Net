using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Utils {
	/// <summary>A formatter for displaying bytes with their abbreviations.</summary>
	public static class FormatBytes {

		#region Constants

		/// <summary>All supported byte abbrevations.</summary>
		public static readonly ReadOnlyCollection<string> Abbreviations =
			Array.AsReadOnly(new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" });

		/// <summary>The default format for abbreviations of just bytes.</summary>
		public const string SingleFormat = "N0";
		/// <summary>The default format for abbreviations greater than just bytes.</summary>
		public const string MultiFormat = "N1";

		#endregion

		#region Default Format

		/// <summary>Formats the bytes using the default formats.</summary>
		public static string Format(int bytes) {
			return Format(Convert.ToDouble(bytes));
		}
		/// <summary>Formats the bytes using the default formats.</summary>
		public static string Format(uint bytes) {
			return Format(Convert.ToDouble(bytes));
		}
		/// <summary>Formats the bytes using the default formats.</summary>
		public static string Format(long bytes) {
			return Format(Convert.ToDouble(bytes));
		}
		/// <summary>Formats the bytes using the default formats.</summary>
		public static string Format(ulong bytes) {
			return Format(Convert.ToDouble(bytes));
		}
		/// <summary>Formats the bytes using the default formats.</summary>
		public static string Format(float bytes) {
			return Format(bytes, SingleFormat, MultiFormat);
		}
		/// <summary>Formats the bytes using the default formats.</summary>
		public static string Format(decimal bytes) {
			return Format(Convert.ToDouble(bytes));
		}
		/// <summary>Formats the bytes using the default formats.</summary>
		public static string Format(double bytes) {
			return Format(bytes, SingleFormat, MultiFormat);
		}

		#endregion

		#region Custom Format

		/// <summary>Formats the bytes using the specified formats.</summary>
		public static string Format(int bytes, string singleFormat, string multiFormat) {
			return Format(Convert.ToDouble(bytes), singleFormat, multiFormat);
		}
		/// <summary>Formats the bytes using the specified formats.</summary>
		public static string Format(uint bytes, string singleFormat, string multiFormat) {
			return Format(Convert.ToDouble(bytes), singleFormat, multiFormat);
		}
		/// <summary>Formats the bytes using the specified formats.</summary>
		public static string Format(long bytes, string singleFormat, string multiFormat) {
			return Format(Convert.ToDouble(bytes), singleFormat, multiFormat);
		}
		/// <summary>Formats the bytes using the specified formats.</summary>
		public static string Format(ulong bytes, string singleFormat, string multiFormat) {
			return Format(Convert.ToDouble(bytes), singleFormat, multiFormat);
		}
		/// <summary>Formats the bytes using the specified formats.</summary>
		public static string Format(float bytes, string singleFormat, string multiFormat) {
			return Format(Convert.ToDouble(bytes), singleFormat, multiFormat);
		}
		/// <summary>Formats the bytes using the specified formats.</summary>
		public static string Format(decimal bytes, string singleFormat, string multiFormat) {
			return Format(Convert.ToDouble(bytes), singleFormat, multiFormat);
		}
		/// <summary>Formats the bytes using the specified formats.</summary>
		public static string Format(double bytes, string singleFormat, string multiFormat) {
			int order;
			int sign = Math.Sign(bytes);
			bytes = Math.Abs(bytes);
			for (order = 0; order < Abbreviations.Count && bytes >= 1024; order++) {
				bytes /= 1024;
			}

			// Don't show decimal places for Bytes
			string format = (order == 0 ? singleFormat : multiFormat);
			
			return string.Format("{0:" + format + "} {1}", sign * bytes, Abbreviations[order]);
		}

		#endregion
	}
}
