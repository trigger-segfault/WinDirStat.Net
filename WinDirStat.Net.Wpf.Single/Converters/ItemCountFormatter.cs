using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace WinDirStat.Net.Wpf.Converters {
	public class ItemCountFormatter : MarkupExtension, IValueConverter {
		public static readonly ItemCountFormatter Instance = new ItemCountFormatter();

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			try {
				long l = System.Convert.ToInt64(value);
				if (l == -1)
					return "";
				return l.ToString("N0");
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
