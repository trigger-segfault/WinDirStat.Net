using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace WinDirStat.Net.Wpf.Converters {
	public class ScaleConverter : MarkupExtension, IValueConverter {
		public static readonly ScaleConverter Instance = new ScaleConverter();

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (targetType == typeof(float))
				return System.Convert.ToSingle(value) * System.Convert.ToSingle(parameter);
			else if (targetType == typeof(double))
				return System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter);
			else if (targetType == typeof(decimal))
				return System.Convert.ToDecimal(value) * System.Convert.ToDecimal(parameter);
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			if (targetType == typeof(float))
				return System.Convert.ToSingle(value) / System.Convert.ToSingle(parameter);
			else if (targetType == typeof(double))
				return System.Convert.ToDouble(value) / System.Convert.ToDouble(parameter);
			else if (targetType == typeof(decimal))
				return System.Convert.ToDecimal(value) / System.Convert.ToDecimal(parameter);
			return null;
		}
	}
}
