using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WinDirStat.Net.Wpf.Controls {
	public static class ListSettings {

		public static readonly DependencyProperty InactiveSelectionProperty =
			DependencyProperty.RegisterAttached("InactiveSelection", typeof(bool), typeof(ListSettings),
				new FrameworkPropertyMetadata(true));

		public static bool GetInactiveSelection(DependencyObject d) {
			return (bool) d.GetValue(InactiveSelectionProperty);
		}

		public static void SetInactiveSelection(DependencyObject d, bool value) {
			d.SetValue(InactiveSelectionProperty, value);
		}
	}
}
