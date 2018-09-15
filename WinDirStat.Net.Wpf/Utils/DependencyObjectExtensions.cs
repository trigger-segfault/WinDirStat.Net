using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WinDirStat.Net.Wpf.Utils {
	public static class DependencyObjectExtensions {


		public static bool IsValueUnset(this DependencyObject d, DependencyProperty dp) {
			return d.ReadLocalValue(dp) == DependencyProperty.UnsetValue;
		}

		public static bool IsValueUnsetAndNull(this DependencyObject d, DependencyProperty dp, object value) {
			if (d.ReadLocalValue(dp) == DependencyProperty.UnsetValue) {
				return (value == null || (value is string str && string.IsNullOrEmpty(str)));
			}
			return false;
		}
	}
}
