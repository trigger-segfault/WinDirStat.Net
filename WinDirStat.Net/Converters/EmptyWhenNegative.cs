using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace WinDirStat.Net.Converters {
	public class EmptyWhenNegative : MarkupExtension, IValueConverter {
		public static EmptyWhenNegative Instance = new EmptyWhenNegative();

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is int i) {
				if (i == -1)
					return "";
				return i.ToString("N0");
			}
			else if (value is long l) {
				if (l == -1)
					return "";
				return l.ToString("N0");
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
