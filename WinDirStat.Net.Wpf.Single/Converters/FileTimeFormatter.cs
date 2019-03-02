using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace WinDirStat.Net.Wpf.Converters {
	public class FileTimeFormatter : MarkupExtension, IValueConverter {
		public static readonly FileTimeFormatter Instance = new FileTimeFormatter();

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			try {
				DateTime dt = (DateTime) value;
				if (dt == DateTime.MinValue)
					return "";
				string shortDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
				string dtFormatted = dt.ToString(shortDateFormat.Replace("yyyy", "yy"));
				return $"{dtFormatted} {dt.ToShortTimeString()}";
			}
			catch {
				return null;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
