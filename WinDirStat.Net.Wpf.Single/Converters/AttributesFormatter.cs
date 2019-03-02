using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Markup;

namespace WinDirStat.Net.Wpf.Converters {
	public class AttributesFormatter : MarkupExtension, IValueConverter {
		public static readonly AttributesFormatter Instance = new AttributesFormatter();

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is FileAttributes attr) {
				string s = "";
				if (attr.HasFlag(FileAttributes.ReadOnly)) s += "R";
				if (attr.HasFlag(FileAttributes.Hidden)) s += "H";
				if (attr.HasFlag(FileAttributes.System)) s += "S";
				if (attr.HasFlag(FileAttributes.Archive)) s += "A";
				if (attr.HasFlag(FileAttributes.Compressed)) s += "C";
				if (attr.HasFlag(FileAttributes.Encrypted)) s += "E";
				return s;
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
