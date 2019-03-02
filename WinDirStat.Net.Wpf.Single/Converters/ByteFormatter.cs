using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Wpf.Converters {
	public class ByteFormatter : MarkupExtension, IValueConverter {
		public static readonly ByteFormatter Instance = new ByteFormatter();

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			try {
				return FormatBytes.Format(System.Convert.ToDouble(value));
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
