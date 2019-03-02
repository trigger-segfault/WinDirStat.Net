using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace WinDirStat.Net.Wpf.Converters {
	public class RadioButtonConverter : MarkupExtension, IValueConverter {
		public static readonly RadioButtonConverter Instance = new RadioButtonConverter();

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return (parameter?.ToString() == value?.ToString());
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			if ((bool) value) {
				if (targetType == typeof(string))
					return parameter.ToString();
				else if (targetType.IsEnum)
					return Enum.Parse(targetType, parameter.ToString());
				else
					return System.Convert.ChangeType(parameter, targetType);
			}
			return Binding.DoNothing;
		}
	}
}
