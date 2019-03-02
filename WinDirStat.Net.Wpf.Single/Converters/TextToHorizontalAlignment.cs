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
	public class TextToHorizontalAlignment : MarkupExtension, IValueConverter {
		public static readonly TextToHorizontalAlignment Instance = new TextToHorizontalAlignment();

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is TextAlignment textAlignment) {
				switch (textAlignment) {
				case TextAlignment.Left: return HorizontalAlignment.Left;
				case TextAlignment.Center: return HorizontalAlignment.Center;
				case TextAlignment.Right: return HorizontalAlignment.Right;
				case TextAlignment.Justify: return HorizontalAlignment.Stretch;
				}
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is HorizontalAlignment horizontalAlignment) {
				switch (horizontalAlignment) {
				case HorizontalAlignment.Left: return TextAlignment.Left;
				case HorizontalAlignment.Center: return TextAlignment.Center;
				case HorizontalAlignment.Right: return TextAlignment.Right;
				case HorizontalAlignment.Stretch: return TextAlignment.Justify;
				}
			}
			return null;
		}
	}
}
