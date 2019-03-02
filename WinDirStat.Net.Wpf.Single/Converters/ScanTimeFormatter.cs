using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace WinDirStat.Net.Wpf.Converters {
	public class ScanTimeFormatter : MarkupExtension, IValueConverter {
		public static readonly ScanTimeFormatter Instance = new ScanTimeFormatter();

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is TimeSpan time) {
				// Time
				//TimeSpan time = node.Document.ScanTime;
				string s = $"{time.Seconds:00}";
				string m = $"{time.Minutes}:";
				string h = "";
				if (time.TotalHours >= 1) {
					m = m.PadLeft(2, '0');
					h = $"{time.TotalHours:N0}:";
				}
				return $"[ {h}{m}{s} s ]";
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
