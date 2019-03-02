using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace WinDirStat.Net.Wpf.Converters {
	public class CollapsedWhenFalse : MarkupExtension, IValueConverter {
		public static readonly CollapsedWhenFalse Instance = new CollapsedWhenFalse();

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return (bool) value ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	public class CollapsedWhenTrue : MarkupExtension, IValueConverter {
		public static readonly CollapsedWhenTrue Instance = new CollapsedWhenTrue();

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return (bool) value ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
