using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WinDirStat.Net.Wpf.Controls {
	public static class FocusExtension {
		public static bool? GetIsFocused(DependencyObject obj) {
			return (bool?) obj.GetValue(IsFocusedProperty);
		}

		public static void SetIsFocused(DependencyObject obj, bool? value) {
			obj.SetValue(IsFocusedProperty, value);
		}

		public static readonly DependencyProperty IsFocusedProperty =
			DependencyProperty.RegisterAttached(
				"IsFocused", typeof(bool?), typeof(FocusExtension),
				new FrameworkPropertyMetadata(null, OnIsFocusedPropertyChanged));

		private static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			FrameworkElement element = (FrameworkElement) d;
			if (e.OldValue == null) {
				element.GotFocus += OnGotFocus;
				element.LostFocus += OnLostFocus;
				element.GotKeyboardFocus += OnGotKeyboardFocus;
				element.LostKeyboardFocus += OnLostKeyboardFocus;
			}
			else if (e.NewValue == null) {
				element.GotFocus -= OnGotFocus;
				element.LostFocus -= OnLostFocus;
				element.GotKeyboardFocus -= OnGotKeyboardFocus;
				element.LostKeyboardFocus -= OnLostKeyboardFocus;
			}

			if (((bool?) e.NewValue) ?? false) {
				element.Focus(); // Don't care about false values.
				//Keyboard.Focus(element);
			}
		}

		private static void OnGotFocus(object sender, RoutedEventArgs e) {
			FrameworkElement element = (FrameworkElement) sender;
			Console.WriteLine("GotFocus");
			SetIsFocused(element, true);
		}
		private static void OnLostFocus(object sender, RoutedEventArgs e) {
			FrameworkElement element = (FrameworkElement) sender;
			Console.WriteLine("LostFocus");
			SetIsFocused(element, false);
		}

		private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
			FrameworkElement element = (FrameworkElement) sender;
			//SetIsFocused(element, true);
			Console.WriteLine("LostKeyboardFocus");
		}

		private static void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
			FrameworkElement element = (FrameworkElement) sender;
			//SetIsFocused(element, false);
			Console.WriteLine("GotKeyboardFocus");
		}
	}
}
