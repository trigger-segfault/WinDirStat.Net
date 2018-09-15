using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace WinDirStat.Net.Wpf.Converters {
	public class ZeroGridLengthWhenFalse : MarkupExtension, IValueConverter {
		public static ZeroGridLengthWhenFalse Instance = new ZeroGridLengthWhenFalse();

		private GridLengthConverter converter = new GridLengthConverter();

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return (bool) value ? converter.ConvertFrom(parameter) : new GridLength(0);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	public class ZeroGridLengthWhenTrue : MarkupExtension, IValueConverter {
		public static ZeroGridLengthWhenTrue Instance = new ZeroGridLengthWhenTrue();

		private GridLengthConverter converter = new GridLengthConverter();

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return (bool) value ? new GridLength(0) : converter.ConvertFrom(parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
