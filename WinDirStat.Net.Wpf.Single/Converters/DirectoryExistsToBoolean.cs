using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace WinDirStat.Net.Wpf.Converters {
	public class DirectoryExistsToBoolean : MarkupExtension, IValueConverter {
		public static readonly DirectoryExistsToBoolean Instance = new DirectoryExistsToBoolean();

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			try {
				string fullPath = Path.GetFullPath(value.ToString());
				return Directory.Exists(fullPath);
			}
			catch {
				// Path must be invalid
				return false;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
