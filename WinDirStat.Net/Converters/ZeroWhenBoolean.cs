using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace WinDirStat.Net.Converters {
	public class ZeroWhenFalse : MarkupExtension, IValueConverter {
		public static ZeroWhenFalse Instance = new ZeroWhenFalse();

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return (bool) value ? parameter : 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	public class ZeroWhenTrue : MarkupExtension, IValueConverter {
		public static ZeroWhenTrue Instance = new ZeroWhenTrue();

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return (bool) value ? 0 : parameter;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
